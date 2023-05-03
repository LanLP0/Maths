using System.Diagnostics;
using System.Text;
using Common.Results;
using LCalc.Extension;
using LCalc.Helpers;
using LCalc.MathTree.Nodes;

namespace LCalc.MathTree;

internal sealed class MathTree
{
    // The order of operation
    public const int SpecialNodePriority = -1;
    public const int PlusMinusNodePriority = 0;
    public const int MulDivModNodePriority = 1;
    public const int ExpFacNodePriority = 2;
    public const int BitwiseNodePriority = 3;
    public const int ValueNodePriority = 4;
    
    public readonly Scope Scope;
    private readonly NodeStack _stack;
    private bool _spaceBeforeToken;

    private CompareNode? _compareNode;
    private IMathNode? _root;

    public MathTree(Scope? scope = null)
    {
        Scope = scope ?? new Scope();
        _stack = new NodeStack();
    }

    public Result Parse(ReadOnlySpan<char> math)
    {
        _compareNode?.Clear();
        _stack.MoveToStart();
        _stack.ClearLevel();

        var buffer = new StringBuilder();
        var tokenType = TokenType.Empty;

        for (var i = 0; i < math.Length; i++)
        {
            var c = math[i];
            var spaceBeforeTokenTmp = false;

            switch ((int)c)
            {
                case >= 48 and <= 57: // 0-9
                {
                    var result = Ok();
                    if (tokenType is not (TokenType.Number or TokenType.VariableSet or TokenType.SpecialNumber))
                        result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    buffer.Append(c);

                    if (tokenType is TokenType.Empty)
                    {
                        if (c == '0' && math.TryGetValueAt(i + 1, out c))
                        {
                            if (c is not ('x' or 'b' or 'o'))
                            {
                                tokenType = TokenType.Number;
                                break;
                            }

                            tokenType = TokenType.SpecialNumber;
                            buffer.Append(c);
                            i++;
                            break;
                        }

                        tokenType = TokenType.Number;
                    }

                    break;
                }
                case 43: // +
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = AddNode(new PlusNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 42: // *
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = AddNode(new MultiplyNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 47: // /
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = AddNode(new DivideNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 124: // |
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = AddNode(new BitwiseOrNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 37: // %
                {
                    Result result;
                    if (!math.TryGetValueAt(i + 1, out var nextChar))
                    {
                        if (tokenType is TokenType.Number)
                        {
                            buffer.Append(c);
                            result = ParseAndSetNode(buffer, ref tokenType, Scope);
                            if (result.Faulted)
                                return result;
                            break;
                        }

                        if (tokenType != TokenType.Variable)
                            return Err($"Invalid operator % at char {i + 1}");

                        result = ParseAndSetNode(buffer, ref tokenType, Scope);
                        if (result.Faulted)
                            return result;

                        var levelStack = _stack.CurrentLevel;
                        AddFnNode(levelStack, new DivideNode());
                        AddValueNode(levelStack, new ValueNode(100));
                        break;
                    }

                    if (!nextChar.IsLowerLetter() && !nextChar.IsDigit())
                    {
                        if (tokenType is TokenType.Number)
                        {
                            buffer.Append(c);
                            result = ParseAndSetNode(buffer, ref tokenType, Scope);
                            if (result.Faulted)
                                return result;
                            break;
                        }

                        if (tokenType != TokenType.Variable)
                            return Err($"Invalid operator % at char {i + 1}");

                        result = ParseAndSetNode(buffer, ref tokenType, Scope);
                        if (result.Faulted)
                            return result;

                        var levelStack = _stack.CurrentLevel;
                        AddFnNode(levelStack, new DivideNode());
                        AddValueNode(levelStack, new ValueNode(100));
                        break;
                    }

                    result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = AddNode(new ModuloNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 45: // -
                {
                    if (tokenType is TokenType.VariableSet)
                    {
                        buffer.Append(c);
                        break;
                    }

                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    var levelStack = _stack.CurrentLevel;
                    IMathNode node = new MinusNode();
                    if (levelStack.Count is 0)
                    {
                        node.AddNode(EmptyNode.Shared);
                        levelStack.Add(node);
                        break;
                    }

                    var addResult = AddFnNode(levelStack, node);
                    if (!addResult.Faulted)
                        break;

                    node.AddNode(EmptyNode.Shared);
                    if (AddValueNode(levelStack, node))
                        break;

                    return Err($"Invalid operator - at char {i + 1}");
                }
                case 126: // ~
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    var levelStack = _stack.CurrentLevel;
                    var bitwiseNotNode = new BitwiseNotNode();
                    if (levelStack.Count is 0)
                    {
                        levelStack.Add(bitwiseNotNode);
                        break;
                    }

                    if (!AddValueNode(levelStack, bitwiseNotNode))
                        return Err($"Invalid operator ~ at char {i + 1}");

                    break;
                }
                case 40: // (
                {
                    if (tokenType is TokenType.Variable) // A function call
                    {
                        MoveDownLevelWithFnCallNode(buffer.ToString());
                        buffer.Clear();
                        tokenType = TokenType.Empty;
                        break;
                    }

                    ParseAndSetNode(buffer, ref tokenType, Scope);
                    MoveDownLevel();
                    break;
                }
                case 41: // )
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = MoveUpLevel();
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 32: // ' '
                {
                    spaceBeforeTokenTmp = true;
                    ParseAndSetNode(buffer, ref tokenType, Scope);
                    break;
                }
                case 44: // ,
                {
                    var levelStack = _stack.CurrentLevel;
                    if (levelStack.Count is 0)
                        return Err("',' can only be used in function calls");
                    
                    if (levelStack.First() is not FunctionCallNode functionCallNode)
                        return Err("',' can only be used in function calls");

                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    levelStack.Clear();
                    levelStack.Add(functionCallNode);

                    break;
                }
                case 38: // &
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    if (math.TryGetValueAt(i + 1, out var nextChar) && nextChar.IsLowerLetter())
                    {
                        buffer.Append(c);
                        tokenType = TokenType.CalculatorOption;
                        break;
                    }

                    result = AddNode(new BitwiseAndNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 46: // .
                {
                    if (tokenType is not (TokenType.Empty or TokenType.Number or TokenType.VariableSet))
                    {
                        var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                        if (result.Faulted)
                            return result;
                    }

                    if (tokenType is TokenType.Empty)
                    {
                        buffer.Append('0');
                        tokenType = TokenType.Number;
                    }

                    buffer.Append('.');
                    break;
                }
                case >= 97 and <= 122: // a-z
                {
                    if (tokenType is not (TokenType.Empty or TokenType.SpecialNumber or TokenType.Variable
                        or TokenType.CalculatorOption
                        or TokenType.AdvancedCalculatorOption))
                    {
                        var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                        if (result.Faulted)
                            return result;
                    }

                    if (tokenType is TokenType.Empty)
                        tokenType = TokenType.Variable;

                    buffer.Append(c);
                    break;
                }
                case 61: // =
                {
                    Result<bool> result;
                    if (tokenType is not (TokenType.Empty or TokenType.CalculatorOption))
                    {
                        result = ParseAndSetNode(buffer, ref tokenType, Scope);
                        if (result.Faulted)
                            return result;
                    }

                    if (!math.TryGetValueAt(i + 1, out var nextChar))
                    {
                        if (tokenType is TokenType.CalculatorOption)
                            return Err("Missing variable value");
                        return Err($"Invalid symbol = at char {i + 1}");
                    }

                    if (tokenType is TokenType.CalculatorOption)
                    {
                        if (nextChar.IsDigit() || nextChar == '-')
                        {
                            tokenType = TokenType.VariableSet;
                            buffer.Append(c);
                            break;
                        }

                        if (!nextChar.IsLowerLetter())
                            return Err($"Invalid symbol = at char {i + 1}");

                        tokenType = TokenType.AdvancedCalculatorOption;
                        buffer.Append(c);
                        break;
                    }

                    if (nextChar != '=')
                        return Err($"Invalid symbol = at char {i + 1}");

                    // the op is ==
                    result = AddToCompare(CompareOp.Equal);
                    if (result.Faulted)
                        return result;
                    if (!result.Value)
                        return Err($"Invalid operator == at char {i + 1}");
                    i++;
                    break;
                }
                case 62: // >
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    if (!math.TryGetValueAt(i + 1, out c))
                        return Err($"Invalid operator > at char {i + 1}");

                    switch (c)
                    {
                        case '>':
                        {
                            i++;
                            result = AddNode(new RightShiftNode());
                            if (result.Faulted)
                                return result;
                            break;
                        }
                        case '=':
                        {
                            var result1 = AddToCompare(CompareOp.GreaterThanOrEqual);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err($"Invalid operator >= at char {i + 1}");
                            i++;
                            break;
                        }
                        default:
                        {
                            var result1 = AddToCompare(CompareOp.GreaterThan);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err($"Invalid operator > at char {i + 1}");
                            break;
                        }
                    }

                    break;
                }
                case 60: // <
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    if (!math.TryGetValueAt(i + 1, out c))
                        return Err($"Invalid operator < at char {i + 1}");

                    switch (c)
                    {
                        case '<':
                        {
                            i++;
                            result = AddNode(new LeftShiftNode());
                            if (result.Faulted)
                                return result;
                            break;
                        }
                        case '=':
                        {
                            var result1 = AddToCompare(CompareOp.LessThanOrEqual);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err($"Invalid operator <= at char {i + 1}");
                            i++;
                            break;
                        }
                        default:
                        {
                            var result1 = AddToCompare(CompareOp.LessThan);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err($"Invalid operator < at char {i + 1}");
                            break;
                        }
                    }

                    break;
                }
                case 33: // !
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    if (!math.TryGetValueAt(i + 1, out c))
                    {
                        result = AddNode(new FactorialNode());
                        if (result.Faulted)
                            return result;
                        break;
                    }

                    if (c == '=')
                    {
                        var result1 = AddToCompare(CompareOp.Difference);
                        if (result1.Faulted)
                            return result1;
                        if (!result1.Value)
                            return Err($"Invalid operator != at char {i + 1}");
                        i++;
                        break;
                    }

                    result = AddNode(new FactorialNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 94: // ^
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    if (!math.TryGetValueAt(i + 1, out c))
                        return Err($"Invalid operator ^ at char {i + 1}");

                    if (c == '^')
                    {
                        result = AddNode(new BitwiseXorNode());
                        if (result.Faulted)
                            return result;
                        i++;
                        break;
                    }

                    result = AddNode(new ExponentNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 91: // [
                {
                    var getFnCollectionResult = Scope.GetFnCollection();
                    if (getFnCollectionResult.Faulted)
                        return getFnCollectionResult;

                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    var end = math.IndexOf(']', i + 1);
                    if (end is -1)
                        return Err("No matching end square bracket found");

                    var body = math.Slice(i + 1, end - i - 1);

                    var parseFnResult = CustomFunction.CustomFunction.Parse(body, getFnCollectionResult.Value!);
                    if (parseFnResult.Faulted)
                        return parseFnResult;

                    result = Scope.AddFn(parseFnResult.Value!);
                    if (result.Faulted)
                        return result;
                    i = end;
                    break;
                }
                case 93: // ]
                    return Err($"Invalid square bracket at char {i + 1}");
            }
            
            if (tokenType is TokenType.Empty)
                _spaceBeforeToken = spaceBeforeTokenTmp;
        }

        var result2 = ParseAndSetNode(buffer, ref tokenType, Scope);
        if (result2.Faulted)
            return result2;

        if (_stack.PreviousLevel is not null) // If not at the first level
            return Err("Invalid number of braces");

        var rootStack = _stack.CurrentLevel;
        if (rootStack.Count is 0)
            return Err("No expression found");

        _root = rootStack[0];
        _compareNode?.AddNode(_root);
        Scope.EndInit();

        return Ok();
    }

    private Result ParseAndSetNode(StringBuilder buffer, scoped ref TokenType tokenType, Scope scope)
    {
        IMathNode resultNode = EmptyNode.Shared;
        switch (tokenType)
        {
            case TokenType.Empty:
                return Ok();
            case TokenType.Number:
            case TokenType.SpecialNumber:
                Span<char> value = stackalloc char[buffer.Length];
                buffer.CopyTo(0, value, value.Length);
                var result1 = ValueNode.Parse(value);
                if (result1.Faulted)
                    return Err(result1.Exception!);

                resultNode = result1.Value!;
                break;
            case TokenType.Variable:
                resultNode = new VariableNode(buffer.ToString());
                break;
            case TokenType.CalculatorOption:
                buffer.Remove(0, 1);
                var op = buffer.ToString();

                Result result;
                switch (op)
                {
                    case "raw":
                        result = scope.SetRawValueOpt();
                        break;
                    case "step":
                        result = scope.SetStepByStepOpt();
                        break;
                    case "tree":
                        scope.SetStepByStepOpt();
                        result = scope.SetShowTreeOpt();
                        break;
                    case "solve":
                        result = scope.SetSolveOpt();
                        break;
                    default:
                        result = scope.IsCalculatorOptionAllowed
                            ? Ok()
                            : Err("Calculator option not allowed in this scope");
                        break;
                }

                if (result.Faulted)
                    return result;

                break;
            case TokenType.VariableSet:
                buffer.Remove(0, 1);
                Span<char> str = stackalloc char[buffer.Length];
                buffer.CopyTo(0, str, str.Length);

                var splitIndex = str.IndexOf('=');
                var firstHalf = str.Slice(0, splitIndex);
                var secondHalf = str.Slice(splitIndex + 1);
                var parseResult = CalculatorHelpers.Parse(secondHalf);
                if (parseResult.Faulted)
                    return Err(parseResult.Exception!);

                var setVarResult = scope.SetVariable(firstHalf.ToString(), parseResult.Value);
                if (setVarResult.Faulted)
                    return setVarResult;

                break;
            case TokenType.AdvancedCalculatorOption:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null);
        }

        buffer.Clear();
        tokenType = TokenType.Empty;
        return AddNode(resultNode);
    }

    private Result AddNode(IMathNode node)
    {
        if (node is EmptyNode)
            return Ok();

        var levelStack = _stack.CurrentLevel;
        if (levelStack.Count is 0)
        {
            if (node.Priority != ValueNodePriority)
            {
                if (node is not MinusNode minusNode)
                    return node.GenerateMissingValueError();

                minusNode.AddNode(EmptyNode.Shared);
            }

            levelStack.Add(node);
            return Ok();
        }

        if (node.Priority != ValueNodePriority)
        {
            var result = AddFnNode(levelStack, node);
            if (result.Faulted)
                return result;

            if (node is FactorialNode)
                node.Priority = ValueNodePriority;

            return result;
        }

        if (AddValueNode(levelStack, node))
            return Ok();

        if (_spaceBeforeToken)
            return Err("Missing operator");

        AddFnNode(levelStack, node is ValueNode ? new ExponentNode() : new MultiplyNode());

        AddValueNode(levelStack, node);
        return Ok();
    }

    private Result AddFnNode(List<IMathNode> levelStack, IMathNode node)
    {
        for (var stackIndex = 0; stackIndex < levelStack.Count;)
        {
            AddOp op;
            var node1 = levelStack[stackIndex];

            if (node1 is ExponentNode && node is ExponentNode) // Special case
                op = AddOp.StepIn;
            else if (stackIndex == levelStack.Count - 1)
                op = AddOp.Interact;
            else if (node1.Priority >= node.Priority)
                op = AddOp.Interact;
            else
                op = AddOp.StepIn;

            switch (op)
            {
                case AddOp.StepIn:
                    stackIndex++;
                    break;
                case AddOp.Interact:
                    if (node1.Priority == SpecialNodePriority)
                        return node.GenerateMissingValueError();

                    if (node1.Priority != ValueNodePriority && !node1.IsFull())
                        return node1.GenerateMissingValueError();

                    node.AddNode(node1);
                    if (stackIndex is not 0) levelStack[stackIndex - 1].ChangeLastNodeTo(node);

                    levelStack.Insert(stackIndex, node);
                    return Ok();
            }
        }

        throw new UnreachableException();
    }

    private bool AddValueNode(List<IMathNode> levelStack, IMathNode node)
    {
        for (var i = levelStack.Count - 1; i >= 0; i--)
        {
            var node1 = levelStack[i];
            if (node1.IsFull()) continue;

            node1.AddNode(node);

            if (i == levelStack.Count - 1)
            {
                levelStack.Add(node);
                return true;
            }

            levelStack.RemoveRange(i + 1, levelStack.Count - i - 1);
            levelStack.Add(node);
            return true;
        }

        return false;
    }

    private void MoveDownLevel()
    {
        if (_stack.MoveDownAndClear())
            return;
        
        _stack.AddLevel();
        _stack.MoveDown();
    }

    private void MoveDownLevelWithFnCallNode(string fnName)
    {
        MoveDownLevel();

        var fnCallNode = new FunctionCallNode(fnName);
        _stack.CurrentLevel.Add(fnCallNode);
    }

    private Result MoveUpLevel()
    {
        var currentLevel = _stack.CurrentLevel;
        if (!_stack.MoveUp())
            return Err("Invalid expression");

        if (currentLevel.Count is 0)
            return Err("Invalid expression");

        var node = currentLevel.First();
        node.Priority = ValueNodePriority;

        return AddNode(node);
    }

    private Result<bool> AddToCompare(CompareOp? op = null)
    {
        if (!Scope.IsCompareAllowed)
            return Err<bool>("Compare not allowed in this scope");

        while (_stack.PreviousLevel is not null)
        {
            var result = MoveUpLevel();
            if (result.Faulted)
                return result;
        }

        var levelStack = _stack.CurrentLevel;
        if (levelStack.Count is 0)
            return false;

        var node = levelStack[0];
        levelStack.Clear();

        _compareNode ??= new CompareNode();

        _compareNode.AddNode(node);
        if (op.HasValue)
            _compareNode.AddOp(op.Value);
        return true;
    }

    public CalcResult Calc()
    {
        if (_compareNode is not null)
        {
            var result1 = _compareNode.Calc(Scope);
            if (result1.Faulted)
                return result1.Exception!;
            return result1.Value;
        }

        var result2 = _root!.Calc(Scope);
        if (result2.Faulted)
            return result2.Exception!;
        return result2.Value;
    }

    public IMathNode GetTopNode()
    {
        return _compareNode ?? _root!;
    }
}

file enum AddOp
{
    StepIn,
    Interact
}