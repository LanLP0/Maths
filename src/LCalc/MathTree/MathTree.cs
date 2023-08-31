using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Common;
using Common.Results;
using LCalc.Extension;
using LCalc.Helpers;
using LCalc.MathTree.Nodes;
using LCalc.Variables;

namespace LCalc.MathTree;

internal sealed class MathTree
{
    // The order of operation
    // Lower is more likely to interact
    public const int SpecialNodePriority = -1;
    public const int PlusMinusNodePriority = 0;
    public const int MulDivModNodePriority = 1;
    public const int ExpFacNodePriority = 2;
    public const int BitwiseNodePriority = 3;
    public const int ValueNodePriority = 4;
    private readonly NodeStack _stack = new();

    public readonly Scope Scope;

    private MathComparer? _comparer;
    private IMathNode? _root;
    private bool _spaceBeforeToken;

    public MathTree() : this(new Scope())
    {
    }

    public MathTree(Scope scope)
    {
        Scope = scope;
    }

    /// <summary>
    ///     Parse the math string into a tree
    /// </summary>
    /// <remarks>This method should only be called once</remarks>
    public Result Parse(ReadOnlySpan<char> math)
    {
        Span<char> buf = stackalloc char[math.Length];
        var buffer = new ValueStringBuilder(buf);
        var tokenType = TokenType.Empty;
        var isInsideCustomFunction = false;

        for (var i = 0; i < math.Length; i++)
        {
            var c = math[i];
            var spaceBeforeTokenTmp = false;

            switch ((int)c)
            {
                case 48: // 0
                case 49: // 1
                case 50: // 2
                case 51: // 3
                case 52: // 4
                case 53: // 5
                case 54: // 6
                case 55: // 7
                case 56: // 8
                case 57: // 9
                {
                    if (tokenType is not (TokenType.Empty or TokenType.Number or TokenType.VariableSet or
                        TokenType.SpecialNumber))
                    {
                        var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                        if (result.Faulted)
                            return result;
                        buffer.Clear();
                    }

                    buffer.Append(c);

                    if (tokenType is TokenType.Empty)
                    {
                        if (c is '0' && math.TryGetValueAt(i + 1, out c))
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
                    buffer.Clear();

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
                    buffer.Clear();

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
                    buffer.Clear();

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
                    buffer.Clear();

                    if (math.TryGetValueAt(i + 1, out var nextChar) && nextChar is '|')
                    {
                        result = AddNode(new BitwiseOrNode());
                        if (result.Faulted)
                            return result;

                        // Skip over two
                        i++;
                        break;
                    }

                    // Abs
                    if (_stack.CurrentLevel.Count is 0)
                    {
                        MoveDownLevelWithFnCallNode("$abs");
                        break;
                    }

                    var firstNode = _stack.CurrentLevel[0];

                    if (firstNode is not FunctionCallNode fnNode)
                    {
                        MoveDownLevelWithFnCallNode("$abs");
                        break;
                    }

                    if (fnNode.Name is not "$abs")
                    {
                        MoveDownLevelWithFnCallNode("$abs");
                        break;
                    }

                    fnNode.Name = "abs";
                    MoveUpLevel();

                    break;
                }
                case 37: // %
                {
                    Result result;
                    if (math.TryGetValueAt(i + 1, out var nextChar) &&
                        (nextChar.IsLowerLetter() || nextChar.IsDigit()))
                    {
                        result = ParseAndSetNode(buffer, ref tokenType, Scope);
                        if (result.Faulted)
                            return result;
                        buffer.Clear();

                        result = AddNode(new ModuloNode());
                        if (result.Faulted)
                            return result;
                        break;
                    }

                    // Percentage

                    result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;
                    buffer.Clear();

                    var levelStack = _stack.CurrentLevel;
                    if (levelStack.Count is 0 || levelStack.Last().Priority is not ValueNodePriority)
                        return Err("Invalid operator '%'");

                    var divNode = new DivideNode();
                    divNode.Priority = ValueNodePriority;
                    AddFnNode(levelStack, divNode);
                    AddValueNode(levelStack, new ValueNode(100));

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
                    buffer.Clear();

                    var node = new MinusNode();
                    if (_stack.CurrentLevel.Count is 0)
                    {
                        node.AddNode(EmptyNode.Shared);
                        node.Priority = ValueNodePriority;

                        _stack.CurrentLevel.Add(node);
                        break;
                    }

                    var lastNonValueNode = GetLastNonValueNode();
                    if (lastNonValueNode is null)
                    {
                        // Try to add as a operator first
                        var rs = AddFnNode(_stack.CurrentLevel, node);
                        if (rs.Success)
                            break;

                        // Add as a negative value
                        node.AddNode(EmptyNode.Shared);
                        node.Priority = ValueNodePriority;

                        if (AddValueNode(_stack.CurrentLevel, node))
                            break;

                        return Err("Invalid operator -");
                    }

                    if (lastNonValueNode.IsFull())
                    {
                        // Then this is an operator

                        var rs = AddFnNode(_stack.CurrentLevel, node);
                        if (rs.Faulted)
                            return rs;

                        break;
                    }

                    if (lastNonValueNode is FunctionCallNode)
                    {
                        // Try to add as a operator first
                        var rs = AddFnNode(_stack.CurrentLevel, node);
                        if (rs.Success)
                            break;

                        // Add as a negative value
                        node.AddNode(EmptyNode.Shared);
                        node.Priority = ValueNodePriority;

                        if (AddValueNode(_stack.CurrentLevel, node))
                            break;

                        return Err("Invalid operator -");
                    }

                    // This is a negative value
                    node.AddNode(EmptyNode.Shared);
                    node.Priority = ValueNodePriority;

                    if (AddValueNode(_stack.CurrentLevel, node))
                        break;

                    return Err("Invalid operator -");
                }
                case 126: // ~
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;
                    buffer.Clear();

                    var levelStack = _stack.CurrentLevel;
                    var node = new BitwiseNotNode();
                    if (levelStack.Count is 0)
                    {
                        levelStack.Add(node);
                        break;
                    }

                    if (!AddValueNode(levelStack, node))
                        return Err("Invalid operator ~");

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
                    buffer.Clear();
                    MoveDownLevel();
                    break;
                }
                case 41: // )
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;
                    buffer.Clear();

                    if (_stack.CurrentLevel.Count is 0)
                        return Err("Invalid expression");

                    var firstNode = _stack.CurrentLevel[0];

                    if (firstNode is FunctionCallNode { Name: "$abs" })
                        return Err("Invalid abs syntax");

                    result = MoveUpLevel();
                    if (result.Faulted)
                        return result;
                    break;
                }
                case 32: // ' '
                {
                    spaceBeforeTokenTmp = true;
                    ParseAndSetNode(buffer, ref tokenType, Scope);
                    buffer.Clear();
                    break;
                }
                case 44: // ,
                {
                    var levelStack = _stack.CurrentLevel;
                    if (levelStack.Count is 0)
                        return Err("',' can only be used in function calls");

                    if (levelStack.First() is not FunctionCallNode fnNode)
                        return Err("',' can only be used in function calls");

                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;
                    buffer.Clear();

                    if (levelStack.Count is 1)
                        return Err("No value before ','");

                    levelStack.Clear();
                    levelStack.Add(fnNode);

                    break;
                }
                case 38: // &
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;
                    buffer.Clear();

                    if (math.TryGetValueAt(i + 1, out var nextChar) && nextChar.IsLowerLetter())
                    {
                        if (isInsideCustomFunction)
                            return Err("Cannot set variable/option inside custom function");

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
                        buffer.Clear();
                    }

                    if (tokenType is TokenType.Empty)
                    {
                        buffer.Append('0');
                        tokenType = TokenType.Number;
                    }

                    buffer.Append('.');
                    break;
                }
                case 97:  // a
                case 98:  // b
                case 99:  // c
                case 100: // d
                case 101: // e
                case 102: // f
                case 103: // g
                case 104: // h
                case 105: // i
                case 106: // j
                case 107: // k
                case 108: // l
                case 109: // m
                case 110: // n
                case 111: // o
                case 112: // p
                case 113: // q
                case 114: // r
                case 115: // s
                case 116: // t
                case 117: // u
                case 118: // v
                case 119: // w
                case 120: // x
                case 121: // y
                case 122: // z
                {
                    if (tokenType is not (TokenType.Empty or TokenType.SpecialNumber or TokenType.Variable
                        or TokenType.CalculatorOption or TokenType.AdvancedCalculatorOption))
                    {
                        var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                        if (result.Faulted)
                            return result;
                        buffer.Clear();
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
                        buffer.Clear();
                    }

                    if (!math.TryGetValueAt(i + 1, out var nextChar))
                    {
                        if (tokenType is TokenType.CalculatorOption)
                            return Err("Missing variable/option value");

                        return Err("Invalid symbol =");
                    }

                    if (tokenType is TokenType.CalculatorOption)
                    {
                        if (nextChar.IsDigit() || nextChar is '-')
                        {
                            tokenType = TokenType.VariableSet;
                            buffer.Append(c);
                            break;
                        }

                        if (!nextChar.IsLowerLetter())
                            return Err("Invalid character '='");

                        tokenType = TokenType.AdvancedCalculatorOption;
                        buffer.Append(c);
                        break;
                    }

                    if (nextChar is not '=')
                        return Err("Invalid character '='");

                    // the op is ==
                    if (isInsideCustomFunction)
                        return Err("Comparison is not allowed inside custom function");

                    result = AddToCompare(CompareOp.Equal);
                    if (result.Faulted)
                        return result;
                    if (!result.Value)
                        return Err("Invalid operator ==");
                    i++;
                    break;
                }
                case 62: // >
                {
                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;
                    buffer.Clear();

                    if (!math.TryGetValueAt(i + 1, out c))
                        return Err("Invalid operator >");

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
                            if (isInsideCustomFunction)
                                return Err("Comparison is not allowed inside custom function");

                            var result1 = AddToCompare(CompareOp.GreaterThanOrEqual);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err("Invalid operator >=");
                            i++;
                            break;
                        }
                        default:
                        {
                            if (isInsideCustomFunction)
                                return Err("Comparison is not allowed inside custom function");

                            var result1 = AddToCompare(CompareOp.GreaterThan);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err("Invalid operator >");
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
                    buffer.Clear();

                    if (!math.TryGetValueAt(i + 1, out c))
                        return Err("Invalid operator <");

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
                            if (isInsideCustomFunction)
                                return Err("Comparison is not allowed inside custom function");

                            var result1 = AddToCompare(CompareOp.LessThanOrEqual);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err("Invalid operator <=");
                            i++;
                            break;
                        }
                        default:
                        {
                            if (isInsideCustomFunction)
                                return Err("Comparison is not allowed inside custom function");

                            var result1 = AddToCompare(CompareOp.LessThan);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err("Invalid operator <");
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
                    buffer.Clear();

                    if (!math.TryGetValueAt(i + 1, out c))
                    {
                        result = AddNode(new FactorialNode());
                        if (result.Faulted)
                            return result;
                        break;
                    }

                    if (c is '=')
                    {
                        if (isInsideCustomFunction)
                            return Err("Comparison is not allowed inside custom function");

                        var result1 = AddToCompare(CompareOp.Difference);
                        if (result1.Faulted)
                            return result1;
                        if (!result1.Value)
                            return Err("Invalid operator !=");
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
                    buffer.Clear();

                    if (!math.TryGetValueAt(i + 1, out c))
                        return Err("Invalid operator ^");

                    if (c is '^')
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
                    if (!Scope.IsCustomFunctionAllowed)
                        return Err("Custom function is not allowed");

                    if (isInsideCustomFunction)
                        return Err("Invalid char '['");

                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;
                    buffer.Clear();

                    i++;
                    if (i >= math.Length)
                        return Err("Invalid char '['");

                    ReadLettersToBuffer(ref buffer, math, ref i);
                    if (buffer.Length is 0)
                        return Err("Custom function name cannot be empty");

                    if (math.Length <= i)
                        return Err("Invalid custom function syntax");

                    if (math[i] is not '(')
                        return Err("Invalid custom function syntax");

                    i++;
                    var argEnd = math.IndexOf(')', i);
                    if (argEnd is -1)
                        return Err("Invalid custom function syntax");

                    var end = math.IndexOf(']', i);
                    if (end is -1)
                        return Err("Invalid custom function syntax");

                    if (argEnd > end) // Prevent [abc(]...)
                        return Err("Invalid custom function syntax");

                    var name = buffer.ToString();
                    buffer.Clear();

                    var args = new VariableCollection();
                    for (; i < argEnd; i++)
                    {
                        var chr = math[i];
                        switch ((int)chr)
                        {
                            case 97:  // a
                            case 98:  // b
                            case 99:  // c
                            case 100: // d
                            case 101: // e
                            case 102: // f
                            case 103: // g
                            case 104: // h
                            case 105: // i
                            case 106: // j
                            case 107: // k
                            case 108: // l
                            case 109: // m
                            case 110: // n
                            case 111: // o
                            case 112: // p
                            case 113: // q
                            case 114: // r
                            case 115: // s
                            case 116: // t
                            case 117: // u
                            case 118: // v
                            case 119: // w
                            case 120: // x
                            case 121: // y
                            case 122: // z
                            {
                                buffer.Append(chr);
                                break;
                            }
                            case 32: // ' '
                            {
                                if (buffer.Length is 0)
                                    break;

                                if (!args.TryAdd(buffer.ToString(), 0))
                                    return Err("Duplicated variables in custom function");

                                buffer.Clear();
                                break;
                            }
                            default:
                                return Err($"Invalid character in custom function arg space '{chr}'");
                        }
                    }

                    if (buffer.Length is not 0)
                    {
                        if (!args.TryAdd(buffer.ToString(), 0))
                            return Err("Duplicated variables in custom function");

                        buffer.Clear();
                    }

                    i = argEnd + 1;
                    if (math[i] is not ('=' or ' '))
                        return Err("Invalid custom function syntax");

                    MoveDownLevel();
                    _stack.CurrentLevel.Add(new CustomFunctionNode(name, args));

                    isInsideCustomFunction = true;
                    break;
                }
                case 93: // ]
                {
                    if (_stack.CurrentLevel.FirstOrDefault() is not CustomFunctionNode cfnNode)
                        return Err("Invalid custom function");

                    var result = ParseAndSetNode(buffer, ref tokenType, Scope);
                    if (result.Faulted)
                        return result;
                    buffer.Clear();

                    if (!cfnNode.IsFull())
                        return Err("Missing custom function body");

                    var fn = cfnNode.ToCustomFunction(Scope.CustomFunctions!);
                    Scope.CustomFunctions!.Add(fn);

                    _stack.MoveUp();
                    isInsideCustomFunction = false;
                    break;
                }
                default:
                {
                    return Err($"Invalid character '{math[i]}'");
                }
            }

            if (tokenType is TokenType.Empty)
                _spaceBeforeToken = spaceBeforeTokenTmp;
        }

        var result2 = ParseAndSetNode(buffer, ref tokenType, Scope);
        if (result2.Faulted)
            return result2;

        var moveUpRs = MoveUpToTop();
        if (moveUpRs.Faulted)
            return moveUpRs;

        var rootStack = _stack.CurrentLevel;
        if (rootStack.Count is 0)
            return Err("No expression found");

        _root = rootStack[0];
        _comparer?.AddNode(_root);
        Scope.EndInit();

        return Ok();
    }

    private Result ParseAndSetNode(scoped ValueStringBuilder buffer, scoped ref TokenType tokenType, Scope scope)
    {
        IMathNode resultNode = EmptyNode.Shared;
        switch (tokenType)
        {
            case TokenType.Empty:
                return Ok();
            case TokenType.Number:
            case TokenType.SpecialNumber:
                var value = buffer.AsSpan();
                var result1 = ValueNode.Parse(value);
                if (result1.Faulted)
                    return result1.Exception!;

                resultNode = result1.Value!;
                break;
            case TokenType.Variable:
                resultNode = new VariableNode(buffer.ToString());
                break;
            case TokenType.CalculatorOption:
                if (!scope.IsCalculatorOptionAllowed)
                    return Err("Calculator option not allowed");

                var op = buffer.AsSpan().Slice(1).ToString();

                switch (op)
                {
                    case "raw":
                        scope.Format = Format.Raw;
                        break;
                    case "step":
                        scope.SetOpt(CalculatorOption.Step);
                        break;
                    case "tree":
                        scope.SetOpt(CalculatorOption.Step | CalculatorOption.Tree);
                        break;
                    case "solve":
                        scope.SetOpt(CalculatorOption.Solve);
                        break;
                    case "latex":
                        scope.SetOpt(CalculatorOption.LaTeX);
                        break;
                    case "latexdoc":
                        scope.SetOpt(CalculatorOption.Step | CalculatorOption.LaTeX | CalculatorOption.LaTeXDoc);
                        break;
                    case "render":
                        scope.SetOpt(CalculatorOption.Render);
                        break;
                }

                break;
            case TokenType.AdvancedCalculatorOption:
                if (!scope.IsCalculatorOptionAllowed)
                    return Err("Calculator option not allowed");

                var opt = buffer.AsSpan().Slice(1);

                var splitPos = opt.IndexOf('=');

                var key = opt.Slice(0, splitPos);
                var val = opt.Slice(splitPos + 1);

                switch (key)
                {
                    case "format":
                    case "fmt":
                        switch (val)
                        {
                            case "human":
                                scope.Format = Format.Human;
                                break;
                            case "none":
                            case "raw":
                                scope.Format = Format.Raw;
                                break;
                            case "hex":
                                scope.Format = Format.Hex;
                                break;
                            case "octal":
                            case "oct":
                                scope.Format = Format.Octal;
                                break;
                            case "binary":
                            case "bin":
                                scope.Format = Format.Binary;
                                break;
                            default:
                                return Err($"Unknown format: {val}");
                        }

                        break;
                }

                break;
            case TokenType.VariableSet:
                if (!scope.IsVariableAllowed)
                    return Err("Variable not allowed");

                var str = buffer.AsSpan().Slice(1);

                var splitIndex = str.IndexOf('=');
                var firstHalf = str.Slice(0, splitIndex);
                var secondHalf = str.Slice(splitIndex + 1);

                var parseResult = CalculatorHelpers.ParseNumber(secondHalf);
                if (parseResult.Faulted)
                    return parseResult.Exception!;

                var setVarResult = scope.SetVariable(firstHalf.ToString(), parseResult.Value);
                if (setVarResult.Faulted)
                    return setVarResult;

                break;
            default:
                throw new UnreachableException();
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
                return node.GenerateMissingValueError();

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
        var stack = CollectionsMarshal.AsSpan(levelStack);
        var i = 0;
        if (node is ExponentNode)
            for (;;)
            {
                var node1 = stack[i];

                if (node1 is not ExponentNode)
                    break;

                i++;
                if (i < stack.Length)
                    continue;

                i--;
                break;
            }

        for (; i < stack.Length;)
        {
            var node1 = stack[i];

            if (i != levelStack.Count - 1 && node1.Priority < node.Priority)
            {
                i++;
                continue;
            }

            // Interact

            if (node1.Priority is SpecialNodePriority)
                return node.GenerateMissingValueError();

            if (node1.Priority is not ValueNodePriority && !node1.IsFull())
                return node1.GenerateMissingValueError();

            node.AddNode(node1);
            if (i is not 0) levelStack[i - 1].ChangeLastNodeTo(node);

            levelStack.Insert(i, node);
            return Ok();
        }

        return Err();
    }

    private bool AddValueNode(List<IMathNode> levelStack, IMathNode node)
    {
        var stack = CollectionsMarshal.AsSpan(levelStack);
        for (var i = stack.Length - 1; i >= 0; i--)
        {
            var node1 = stack[i];
            if (node1.IsFull()) continue;

            node1.AddNode(node);

            if (i == stack.Length - 1)
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
        _stack.MoveDownAndClear();
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
        if (node is CustomFunctionNode)
            return Err("Invalid amount of braces in custom function");

        node.Priority = ValueNodePriority;

        return AddNode(node);
    }

    private Result MoveUpToTop()
    {
        var currentLevel = _stack.CurrentLevel;
        while (_stack.MoveUp())
        {
            if (currentLevel.Count is 0)
                return Err("Invalid expression");

            var node = currentLevel.First();
            if (node is CustomFunctionNode)
                return Err("Invalid amount of braces in custom function");

            if (node is FunctionCallNode fnNode && fnNode.Name[0] is '$')
                fnNode.Name = fnNode.Name.Substring(1);

            node.Priority = ValueNodePriority;

            var rs = AddNode(node);
            if (rs.Faulted)
                return rs;

            currentLevel = _stack.CurrentLevel;
        }

        return Ok();
    }

    private Result<bool> AddToCompare(CompareOp? op = null)
    {
        if (!Scope.IsCompareAllowed)
            return Err<bool>("Compare not allowed");

        var rs = MoveUpToTop();
        if (rs.Faulted)
            return rs;

        var levelStack = _stack.CurrentLevel;
        if (levelStack.Count is 0)
            return false;

        var node = levelStack[0];
        levelStack.Clear();

        _comparer ??= new MathComparer();

        _comparer.AddNode(node);
        if (op.HasValue)
            _comparer.AddOp(op.Value);
        return true;
    }

    private IMathNode? GetLastNonValueNode()
    {
        var stackLevel = CollectionsMarshal.AsSpan(_stack.CurrentLevel);

        for (var i = stackLevel.Length - 1; i >= 0; i--)
        {
            var node = stackLevel[i];
            if (node.Priority is ValueNodePriority)
                continue;

            return node;
        }

        return null;
    }

    private void ReadLettersToBuffer(ref ValueStringBuilder buffer, ReadOnlySpan<char> math, ref int index)
    {
        for (; index < math.Length; index++)
        {
            var chr = math[index];
            if (!chr.IsLowerLetter())
                break;

            buffer.Append(chr);
        }
    }

    public CalcResult Calc()
    {
        if (_comparer is not null)
            return _comparer.Calc(Scope).MapToCalcResult(Scope.Format);

        return _root!.Calc(Scope).MapToCalcResult(Scope.Format);
    }

    public IMathNode GetTopNode()
    {
        return _comparer ?? _root!;
    }
}