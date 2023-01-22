using System.Diagnostics;
using System.Text;
using Common.Results;
using LCalc.Extension;
using LCalc.Helpers;
using LCalc.MathTree.Nodes;
using OneOf;

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
    private readonly List<List<IMathNode>> _stack;
    public readonly Scope Scope;
    private bool _isPrevCharSpace;
    public CompareNode? CompareNode;

    public IMathNode? Root;

    public MathTree(Scope? scope = null)
    {
        Scope = scope ?? Scope.Create();
        _stack = new List<List<IMathNode>> { new() };
    }

    public Result Parse(ReadOnlySpan<char> math, Scope? scope = null)
    {
        CompareNode?.Clear();
        if (_stack.Count is not 0) _stack[0].Clear();

        var isPrevCharSpace = false;
        var buffer = new StringBuilder();
        var tokenType = TokenType.Empty;
        var level = 0;

        for (var i = 0; i < math.Length; i++)
        {
            var c = math[i];

            switch ((int)c)
            {
                case >= 48 and <= 57: // 0-9
                {
                    var result = Ok();
                    if (tokenType is not (TokenType.Number or TokenType.VariableSet or TokenType.SpecialNumber))
                        result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
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
                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = AddNode(level, new PlusNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 42: // *
                {
                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = AddNode(level, new MultiplyNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 47: // /
                {
                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = AddNode(level, new DivideNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 124: // |
                {
                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = AddNode(level, new BitwiseOrNode());
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
                            result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                            if (result.Faulted)
                                return result;
                            break;
                        }

                        if (tokenType != TokenType.Variable)
                            return Err($"Invalid operator % at char {i + 1}");

                        result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                        if (result.Faulted)
                            return result;
                        AddFnNode(level, new DivideNode());
                        AddValueNode(level, new ValueNode(100));
                        break;
                    }

                    if (!nextChar.IsLowerLetter() && !nextChar.IsDigit())
                    {
                        if (tokenType is TokenType.Number)
                        {
                            buffer.Append(c);
                            result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                            if (result.Faulted)
                                return result;
                            break;
                        }

                        if (tokenType != TokenType.Variable)
                            return Err($"Invalid operator % at char {i + 1}");

                        result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                        if (result.Faulted)
                            return result;
                        AddFnNode(level, new DivideNode());
                        AddValueNode(level, new ValueNode(100));
                        break;
                    }

                    result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = AddNode(level, new ModuloNode());
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

                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    var levelStack = _stack[level];
                    IMathNode node = new MinusNode();
                    if (levelStack.Count is 0)
                    {
                        node.AddNode(EmptyNode.Shared);
                        levelStack.Add(node);
                        break;
                    }

                    var addResult = AddFnNode(level, node);
                    if (!addResult.Faulted)
                        break;

                    node.AddNode(EmptyNode.Shared);
                    if (AddValueNode(level, node))
                        break;

                    return Err($"Invalid operator - at char {i + 1}");
                }
                case 126: // ~
                {
                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    var levelStack = _stack[level];
                    var bitwiseNotNode = new BitwiseNotNode();
                    if (levelStack.Count is 0)
                    {
                        levelStack.Add(bitwiseNotNode);
                        break;
                    }

                    if (!AddValueNode(level, bitwiseNotNode))
                        return Err($"Invalid operator ~ at char {i + 1}");

                    break;
                }
                case 40: // (
                {
                    if (tokenType is TokenType.Variable) // A function call
                    {
                        MoveDownLevelWithFnCallNode(ref level, buffer.ToString());
                        buffer.Clear();
                        tokenType = TokenType.Empty;
                        break;
                    }

                    ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    MoveDownLevel(ref level);
                    break;
                }
                case 41: // )
                {
                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    result = MoveUpLevel(ref level);
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 32: // ' '
                {
                    isPrevCharSpace = true;
                    ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    break;
                }
                case 44: // ,
                {
                    var levelStack = _stack[level];
                    if (levelStack.First() is not FunctionCallNode functionCallNode)
                        return Err("Coma can only be used inside of a function call");

                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    levelStack.Clear();
                    levelStack.Add(functionCallNode);

                    break;
                }
                case 38: // &
                {
                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    if (math.TryGetValueAt(i + 1, out var nextChar) && nextChar.IsLowerLetter())
                    {
                        buffer.Append(c);
                        tokenType = TokenType.CalculatorOption;
                        break;
                    }

                    result = AddNode(level, new BitwiseAndNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 46: // .
                {
                    if (tokenType is not (TokenType.Empty or TokenType.Number or TokenType.VariableSet))
                    {
                        var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
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
                        var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
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
                        result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
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
                    result = AddToCompare(ref level, CompareOp.Equal);
                    if (result.Faulted)
                        return result;
                    if (!result.Value)
                        return Err($"Invalid operator == at char {i + 1}");
                    i++;
                    break;
                }
                case 62: // >
                {
                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    if (!math.TryGetValueAt(i + 1, out c))
                        return Err($"Invalid operator > at char {i + 1}");

                    switch (c)
                    {
                        case '>':
                        {
                            i++;
                            result = AddNode(level, new RightShiftNode());
                            if (result.Faulted)
                                return result;
                            break;
                        }
                        case '=':
                        {
                            var result1 = AddToCompare(ref level, CompareOp.GreaterThanOrEqual);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err($"Invalid operator >= at char {i + 1}");
                            i++;
                            break;
                        }
                        default:
                        {
                            var result1 = AddToCompare(ref level, CompareOp.GreaterThan);
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
                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    if (!math.TryGetValueAt(i + 1, out c))
                        return Err($"Invalid operator < at char {i + 1}");

                    switch (c)
                    {
                        case '<':
                        {
                            i++;
                            result = AddNode(level, new LeftShiftNode());
                            if (result.Faulted)
                                return result;
                            break;
                        }
                        case '=':
                        {
                            var result1 = AddToCompare(ref level, CompareOp.LessThanOrEqual);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err($"Invalid operator <= at char {i + 1}");
                            i++;
                            break;
                        }
                        default:
                        {
                            var result1 = AddToCompare(ref level, CompareOp.LessThan);
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
                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    if (!math.TryGetValueAt(i + 1, out c))
                    {
                        result = AddNode(level, new FactorialNode());
                        if (result.Faulted)
                            return result;
                        break;
                    }

                    if (c == '=')
                    {
                        var result1 = AddToCompare(ref level, CompareOp.Difference);
                        if (result1.Faulted)
                            return result1;
                        if (!result1.Value)
                            return Err($"Invalid operator != at char {i + 1}");
                        i++;
                        break;
                    }

                    result = AddNode(level, new FactorialNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 94: // ^
                {
                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;

                    if (!math.TryGetValueAt(i + 1, out c))
                        return Err($"Invalid operator ^ at char {i + 1}");

                    if (c == '^')
                    {
                        result = AddNode(level, new BitwiseXorNode());
                        if (result.Faulted)
                            return result;
                        i++;
                        break;
                    }

                    result = AddNode(level, new ExponentNode());
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 91: // [
                {
                    var getFnCollectionResult = Scope.GetFnCollection();
                    if (getFnCollectionResult.Faulted)
                        return getFnCollectionResult;

                    var result = ParseAndSetNode(level, buffer, ref tokenType, Scope);
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

            if (isPrevCharSpace)
                _isPrevCharSpace = true;
            else if (_isPrevCharSpace)
                _isPrevCharSpace = false;
        }

        var result2 = ParseAndSetNode(level, buffer, ref tokenType, Scope);
        if (result2.Faulted)
            return result2;

        if (level is not 0)
            return Err("Invalid number of braces");

        var rootStack = _stack[0];
        if (rootStack.Count is 0)
            return Err("No expression found");

        Root = rootStack[0];
        CompareNode?.AddNode(Root);
        Scope.EndInit();

        return Ok();
    }

    private Result ParseAndSetNode(int level, StringBuilder buffer, scoped ref TokenType tokenType, Scope scope)
    {
        IMathNode resultNode = EmptyNode.Shared;
        switch (tokenType)
        {
            case TokenType.Empty:
                return Ok();
            case TokenType.Number:
            case TokenType.SpecialNumber:
                Span<char> value = stackalloc char[buffer.Length];
                buffer.CopyTo(0, value, buffer.Length);
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
                        result = scope.SetRawValueOpt(true);
                        break;
                    case "step":
                        result = scope.SetStepByStepOpt(true);
                        break;
                    case "tree":
                        result = scope.SetShowTreeOpt(true);
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
        return AddNode(level, resultNode);
    }

    private Result AddNode(int level, IMathNode node)
    {
        if (node is EmptyNode)
            return Ok();

        var levelStack = _stack[level];
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
            var result = AddFnNode(level, node);
            if (result.Faulted)
                return result;

            if (node is FactorialNode)
                node.Priority = ValueNodePriority;

            return result;
        }

        if (AddValueNode(level, node))
            return Ok();

        if (_isPrevCharSpace)
            return Err("Missing operator");

        AddFnNode(level, node is ValueNode ? new ExponentNode() : new MultiplyNode());

        AddValueNode(level, node);
        return Ok();
    }

    private Result AddFnNode(int level, IMathNode node)
    {
        var levelStack = _stack[level];
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

    private bool AddValueNode(int level, IMathNode node)
    {
        var levelStack = _stack[level];
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

    private void MoveDownLevel(ref int level)
    {
        if (_stack.Count == level + 1)
            _stack.Add(new List<IMathNode>());

        var levelStack = _stack[++level];
        if (levelStack.Count is not 0)
            levelStack.Clear();
    }

    private void MoveDownLevelWithFnCallNode(ref int level, string fnName)
    {
        if (_stack.Count == level + 1)
            _stack.Add(new List<IMathNode>());

        var levelStack = _stack[++level];
        if (levelStack.Count is not 0)
            levelStack.Clear();

        var fnCallNode = new FunctionCallNode();
        fnCallNode.SetName(fnName);
        levelStack.Add(fnCallNode);
    }

    private Result MoveUpLevel(scoped ref int level)
    {
        if (level is 0)
            return Err("Invalid expression");

        var node = _stack[level].First();
        node.Priority = ValueNodePriority;

        return AddNode(--level, node);
    }

    private Result<bool> AddToCompare(scoped ref int level, CompareOp? op = null)
    {
        if (!Scope.IsCompareAllowed)
            return Err<bool>("Compare not allowed in this scope");

        while (level is not 0)
        {
            var result = MoveUpLevel(ref level);
            if (result.Faulted)
                return result;
        }

        var levelStack = _stack[0];
        if (levelStack.Count is 0)
            return false;

        var node = levelStack[0];
        levelStack.Clear();

        CompareNode ??= new CompareNode();

        CompareNode.AddNode(node);
        if (op.HasValue)
            CompareNode.AddOp(op.Value);
        return true;
    }

    public OneOf<Exception, bool, double> Calc()
    {
        if (CompareNode != null)
        {
            var result1 = CompareNode.Calc(Scope);
            if (result1.Faulted)
                return result1.Exception!;
            return result1.Value!;
        }

        var result2 = Root!.Calc(Scope);
        if (result2.Faulted)
            return result2.Exception!;
        return result2.Value!;
    }
}

file enum AddOp
{
    StepIn,
    Interact
}