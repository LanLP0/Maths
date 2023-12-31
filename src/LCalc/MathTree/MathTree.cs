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
    // Higher is harder to step in
    public const int SpecialNodePriority = -1;
    public const int PlusMinusNodePriority = 0;
    public const int MulDivModNodePriority = 1;
    public const int ExpFacNodePriority = 2;
    public const int BitwiseNodePriority = 3;
    public const int ValueNodePriority = 4;

    public readonly Scope Scope;
    private MathComparer? _comparer;
    private IMathNode? _root;

    public MathTree() : this(new Scope())
    {
    }

    public MathTree(Scope scope)
    {
        Scope = scope;
    }

    public Result Parse(ReadOnlySpan<char> math)
    {
        Span<SpanSegment<char>> chunks = stackalloc SpanSegment<char>[math.Length];
        SplitIntoChunks(math, ref chunks, out var count);

        var levelRoot = new LinkedNode<IMathNode?>(null);
        var spaceBeforeToken = false;
        var isInsideCustomFunction = false;
        var isInsideComputedVariable = false;

        for (var i = 0; i < count; i++)
        {
            var chunk = chunks[i];
            var c = chunk.GetFirst(math);

            Result result;
            switch (c)
            {
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z': // Variable / Fn call
                {
                    if (i + 1 < count)
                    {
                        // Test if this is a fn call

                        var next = chunks[i + 1];
                        if (next.GetFirst(math) is '(')
                        {
                            // This is a fn call
                            MoveDownFnCall(ref levelRoot, chunk.GetSpan(math).ToString());
                            i++; // Skip over the '('
                            break;
                        }
                    }

                    result = AddNode(levelRoot, new VariableNode(chunk.GetSpan(math).ToString()), spaceBeforeToken);
                    if (result.Faulted)
                        return result;

                    break;
                }
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '.': // Special number / Number
                {
                    Result<ValueNode> parseResult;

                    if (chunk.Length is 1 && c is '0' && i + 1 < count)
                    {
                        // Test for special number
                        var nextChunk = chunks[i + 1];
                        if (nextChunk.GetFirst(math) is 'b' or 'x' or 'o')
                        {
                            if (i + 2 < count)
                            {
                                var nextNextChunk = chunks[i + 2];
                                if (GetCharacterType(nextNextChunk.GetFirst(math)) is
                                    CharacterType.Number or CharacterType.Letter)
                                {
                                    // This is a special number
                                    var skip = 2;
                                    var length = 1 + nextChunk.Length + nextNextChunk.Length;

                                    for (var j = i + 3; j < count; j++)
                                    {
                                        var chunk1 = chunks[j];
                                        if (GetCharacterType(chunk1.GetFirst(math)) is not
                                            (CharacterType.Number or CharacterType.Letter))
                                            break;

                                        skip++;
                                        length += chunk1.Length;
                                    }

                                    // Merged all the chunks
                                    chunk = new SpanSegment<char>(chunk.Start, length);

                                    parseResult = ValueNode.Parse(chunk.GetSpan(math));
                                    if (parseResult.Faulted)
                                        return parseResult;

                                    result = AddNode(levelRoot, parseResult.Value!, spaceBeforeToken);
                                    if (result.Faulted)
                                        return result;

                                    i += skip; // Skip over the chunks that have ben merged
                                    break;
                                }
                            }
                            else if (nextChunk.Length > 1)
                            {
                                // This is also a special number
                                chunk = new SpanSegment<char>(chunk.Start, 1 + nextChunk.Length);

                                parseResult = ValueNode.Parse(chunk.GetSpan(math));
                                if (parseResult.Faulted)
                                    return parseResult;

                                result = AddNode(levelRoot, parseResult.Value!, spaceBeforeToken);
                                if (result.Faulted)
                                    return result;

                                i++; // Skip over the two chunks that have ben merged
                                break;
                            }
                        }
                    }

                    parseResult = ValueNode.Parse(chunk.GetSpan(math));
                    if (parseResult.Faulted)
                        return parseResult;

                    result = AddNode(levelRoot, parseResult.Value!, spaceBeforeToken);
                    if (result.Faulted)
                        return result;

                    break;
                }
                case ' ':
                {
                    spaceBeforeToken = true;
                    continue;
                }
                case '<':
                case '=':
                case '>': // Compare (except '!=') / Bitwise shift
                {
                    if (chunk.Length > 2)
                        return Err($"Invalid operator '{chunk.GetSpan(math)}'");

                    var op = chunk.GetSpan(math);
                    Result<bool> result1;
                    switch (op)
                    {
                        case "<":
                        {
                            result1 = AddToCompare(ref levelRoot, CompareOp.LessThan,
                                isInsideCustomFunction);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err();
                            break;
                        }
                        case ">":
                        {
                            result1 = AddToCompare(ref levelRoot, CompareOp.GreaterThan,
                                isInsideCustomFunction);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err();
                            break;
                        }
                        case ">=":
                        {
                            result1 = AddToCompare(ref levelRoot, CompareOp.GreaterThanOrEqual,
                                isInsideCustomFunction);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err();
                            break;
                        }
                        case "<=":
                        {
                            result1 = AddToCompare(ref levelRoot, CompareOp.LessThanOrEqual,
                                isInsideCustomFunction);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err();
                            break;
                        }
                        case "==":
                        {
                            result1 = AddToCompare(ref levelRoot, CompareOp.Equal,
                                isInsideCustomFunction);
                            if (result1.Faulted)
                                return result1;
                            if (!result1.Value)
                                return Err();
                            break;
                        }
                        case ">>":
                        {
                            result = AddNode(levelRoot, new RightShiftNode(), spaceBeforeToken);
                            if (result.Faulted)
                                return result;
                            break;
                        }
                        case "<<":
                        {
                            result = AddNode(levelRoot, new LeftShiftNode(), spaceBeforeToken);
                            if (result.Faulted)
                                return result;
                            break;
                        }
                        default:
                        {
                            return Err($"Invalid operator '{chunk.GetSpan(math)}'");
                        }
                    }

                    break;
                }
                case '+': // Positive value / Addition
                {
                    result = AddNode(levelRoot, new PlusNode(), spaceBeforeToken);
                    if (result.Faulted)
                    {
                        // Allow for positive value
                        if (i + 1 >= count)
                            return result;

                        var nextChar = chunks[i + 1].GetFirst(math);
                        if (IsValue(nextChar))
                            break;

                        return result;
                    }

                    break;
                }
                case '-': // Negative value / subtract
                {
                    var node = new MinusNode();

                    result = AddNode(levelRoot, node, spaceBeforeToken);
                    if (result.Success)
                        break;

                    node.AddNode(EmptyNode.Shared);
                    node.Priority = ValueNodePriority;

                    result = AddNode(levelRoot, node, spaceBeforeToken);
                    if (result.Faulted)
                        return result;

                    break;
                }
                case '*': // Multiply
                {
                    result = AddNode(levelRoot, new MultiplyNode(), spaceBeforeToken);
                    if (result.Faulted)
                        return result;
                    break;
                }
                case '/': // Divide
                {
                    result = AddNode(levelRoot, new DivideNode(), spaceBeforeToken);
                    if (result.Faulted)
                        return result;
                    break;
                }
                case '^': // Exponent / bw xor
                {
                    if (!math.TryGetValueAt(chunk.Start + 1, out c))
                        return Err("Invalid operator ^");

                    if (c is '^')
                    {
                        result = AddNode(levelRoot, new BitwiseXorNode(), spaceBeforeToken);
                        if (result.Faulted)
                            return result;

                        i++;
                        break;
                    }

                    result = AddNode(levelRoot, new ExponentNode(), spaceBeforeToken);
                    if (result.Faulted)
                        return result;

                    break;
                }
                case '%': // Percentage / modulo
                {
                    if (math.TryGetValueAt(chunk.Start + 1, out var nextChar1) && IsValue(nextChar1))
                    {
                        // Modulo

                        result = AddNode(levelRoot, new ModuloNode(), spaceBeforeToken);
                        if (result.Faulted)
                            return result;

                        break;
                    }

                    if (levelRoot.Value is null)
                        return Err("Invalid operator '%'");

                    IMathNode? secondToLastNode = null;
                    var lastNode = levelRoot.Value;

                    for (;;)
                    {
                        var nodeTmp = lastNode.GetLastNode();
                        if (nodeTmp is null)
                            break;

                        secondToLastNode = lastNode;
                        lastNode = nodeTmp;
                    }

                    if (lastNode is ValueNode valueNode)
                    {
                        valueNode.Value /= 100;
                        break;
                    }

                    if (lastNode is not VariableNode variableNode)
                        return Err("Invalid operator '%'");

                    var node = new DivideNode();
                    node.AddNode(variableNode);
                    node.AddNode(new ValueNode(100));
                    node.Priority = ValueNodePriority;

                    if (secondToLastNode is null)
                    {
                        levelRoot.Value = node;
                        break;
                    }

                    secondToLastNode.ChangeLastNodeTo(node);
                    break;
                }
                case '|': // Bw or / abs
                {
                    if (math.TryGetValueAt(chunk.Start + 1, out var nextChar) && nextChar is '|')
                    {
                        result = AddNode(levelRoot, new BitwiseOrNode(), spaceBeforeToken);
                        if (result.Faulted)
                            return result;

                        // Skip over the other '|'
                        i++;
                        break;
                    }

                    // Abs
                    if (levelRoot.Value is null)
                    {
                        MoveDownFnCall(ref levelRoot, "$abs");
                        break;
                    }

                    var firstNode = levelRoot.Value;

                    if (firstNode is not FunctionCallNode fnNode)
                    {
                        MoveDownFnCall(ref levelRoot, "$abs");
                        break;
                    }

                    if (fnNode.Name is not "$abs")
                    {
                        MoveDownFnCall(ref levelRoot, "$abs");
                        break;
                    }

                    fnNode.Name = fnNode.Name.Substring(1);
                    MoveUp(ref levelRoot);
                    break;
                }
                case '&': // Variable / bw and
                {
                    if (math.TryGetValueAt(chunk.Start + 1, out var nextChar) && nextChar.IsLowerLetter())
                    {
                        // Variable

                        if (isInsideCustomFunction)
                            return Err("Cannot set variable/option inside custom function");

                        if (isInsideComputedVariable)
                            return Err("Cannot set variable/option inside computed variable");

                        var nameChunk = chunks[i + 1];
                        var length = 1 + nameChunk.Length;
                        var skip = 1;
                        if (i + 2 < count)
                        {
                            var next2 = chunks[i + 2];
                            if (next2.GetFirst(math) is '=')
                            {
                                if (i + 3 >= count)
                                    return Err("Missing variable/option value");

                                var next3 = chunks[i + 3];
                                var next3FirstChar = next3.GetFirst(math);
                                if (GetCharacterType(next3FirstChar) is CharacterType.Letter
                                    or CharacterType.Number)
                                {
                                    length += next2.Length + next3.Length;
                                    skip += 2;
                                }
                                else if (next3FirstChar is '-' &&
                                    math.TryGetValueAt(next3.Start + 1, out c) && c.IsDigit())
                                {
                                    // Negative value
                                    length += next2.Length + 1 + chunks[i + 4].Length;
                                    skip += 3;
                                }
                                else if (next3FirstChar is '(')
                                {
                                    MoveDownAndClear(ref levelRoot);
                                    levelRoot.Value = new ComputedVariableNode(nameChunk.GetSpan(math).ToString());
                                    i += 3;
                                    isInsideComputedVariable = true;
                                    break;
                                }
                                else
                                {
                                    return Err("Invalid variable/option value");
                                }
                            }
                        }

                        var segment = new SpanSegment<char>(chunk.Start, length);
                        result = SetVariable(segment, math);
                        if (result.Faulted)
                            return result;

                        i += skip;
                        break;
                    }

                    result = AddNode(levelRoot, new BitwiseAndNode(), spaceBeforeToken);
                    if (result.Faulted)
                        return result;
                    break;
                }
                case '~': // Bw not
                {
                    var node = new BitwiseNotNode();
                    if (levelRoot.Value is null)
                    {
                        levelRoot.Value = node;
                        break;
                    }

                    if (!AddValueNode(levelRoot.Value, node))
                        return Err("Invalid operator ~");

                    break;
                }
                case '!': // Factorial / compare unequal
                {
                    if (!math.TryGetValueAt(chunk.Start + 1, out c))
                    {
                        result = AddNode(levelRoot, new FactorialNode(), spaceBeforeToken);
                        if (result.Faulted)
                            return result;
                        break;
                    }

                    if (c is '=')
                    {
                        var result1 = AddToCompare(ref levelRoot, CompareOp.Difference,
                            isInsideCustomFunction);
                        if (result1.Faulted)
                            return result1;
                        if (!result1.Value)
                            return Err("Invalid operator !=");
                        i++;
                        break;
                    }

                    result = AddNode(levelRoot, new FactorialNode(), spaceBeforeToken);
                    if (result.Faulted)
                        return result;
                    break;
                }
                case '(': // Brace left
                {
                    MoveDownAndClear(ref levelRoot);
                    break;
                }
                case ')': // Brace right
                {
                    switch (levelRoot.Value)
                    {
                        case ComputedVariableNode:
                            isInsideComputedVariable = false;
                            break;
                    }

                    result = MoveUp(ref levelRoot);
                    if (result.Faulted)
                        return result;

                    break;
                }
                case '[': // Custom function begin
                {
                    if (!Scope.IsCustomFunctionAllowed)
                        return Err("Custom function is not allowed");

                    if (isInsideCustomFunction || isInsideComputedVariable)
                        return Err("Invalid char '['");

                    i++;
                    if (i >= count)
                        return Err("Invalid char '['");

                    var name = chunks[i++];
                    if (GetCharacterType(name.GetFirst(math)) is not CharacterType.Letter)
                        return Err("Invalid custom function name");

                    if (count <= i)
                        return Err("Invalid custom function syntax");

                    if (chunks[i].GetFirst(math) is not '(')
                        return Err("Invalid custom function syntax");

                    // Arg
                    i++;
                    var isComaBefore = false;
                    var args = new VariableCollection();

                    for (; i < count; i++)
                    {
                        var chunk1 = chunks[i];
                        var firstChar1 = chunk1.GetFirst(math);

                        switch (firstChar1)
                        {
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                            {
                                if (!args.TryAdd(chunk1.GetSpan(math).ToString(), 0))
                                    return Err("Duplicated variables in custom function");

                                isComaBefore = false;
                                break;
                            }
                            case ',':
                                if (isComaBefore)
                                    return Err("Invalid custom function syntax");
                                break;
                            case ' ':
                                break;
                            case ')':
                                if (isComaBefore)
                                    return Err("Invalid custom function syntax");

                                goto BreakCustomFunctionParseArgLoop;
                            case ']':
                                return Err("Invalid custom function syntax");
                            default:
                                return Err("Invalid custom function syntax");
                        }

                        continue;
                        BreakCustomFunctionParseArgLoop:
                        i++;
                        break;
                    }

                    if (chunks[i].GetFirst(math) is not ('=' or ' '))
                        return Err("Invalid custom function syntax");

                    MoveDown(ref levelRoot);
                    levelRoot.Value = new CustomFunctionNode(name.GetSpan(math).ToString(), args);

                    isInsideCustomFunction = true;
                    break;
                }
                case ']': // Custom function end
                {
                    if (levelRoot.Value is not CustomFunctionNode cfnNode)
                        return Err("Invalid custom function");

                    if (!cfnNode.IsFull())
                        return Err("Missing custom function body");

                    var fn = cfnNode.MakeCustomFunction(Scope.CustomFunctions!);
                    result = Scope.CustomFunctions!.Add(fn);
                    if (result.Faulted)
                        return result;

                    levelRoot = levelRoot.Previous!;
                    isInsideCustomFunction = false;
                    break;
                }
                case ',':
                    if (levelRoot.Value is not FunctionCallNode fnCallNode)
                        return Err("',' can only be used in function calls");

                    if (fnCallNode.ArgCount is 0 || fnCallNode.SkipOverSlot)
                        return Err("No value before ','");

                    fnCallNode.SkipOverSlot = true;
                    break;
                default:
                {
                    return Err($"Invalid character '{c}'");
                }
            }

            spaceBeforeToken = false;
        }

        if (isInsideCustomFunction)
            return Err("Invalid custom function syntax");

        var result2 = MoveUpToTop(ref levelRoot);
        if (result2.Faulted)
            return result2;

        if (levelRoot.Value is null)
            return Err("No expression found");

        _root = levelRoot.Value;
        _comparer?.AddNode(_root);
        Scope.EndInit();

        return Ok();
    }

    private Result<bool> AddToCompare(scoped ref LinkedNode<IMathNode?> levelRoot, CompareOp op,
        bool isInsideCustomFunction)
    {
        if (isInsideCustomFunction)
            return Err("Comparison is not allowed inside custom function");

        if (!Scope.IsCompareAllowed)
            return Err<bool>("Compare not allowed");

        _comparer ??= new MathComparer();

        var result = MoveUpToTop(ref levelRoot);
        if (result.Faulted)
            return result;

        if (levelRoot.Value is null)
            return false;

        _comparer.AddNode(levelRoot.Value);
        _comparer.AddOp(op);

        levelRoot.Value = null;
        return true;
    }

    private Result MoveUpToTop(scoped ref LinkedNode<IMathNode?> levelRoot)
    {
        for (;;)
        {
            if (levelRoot.Previous is null)
                return Ok();

            var result = MoveUpForgiving(ref levelRoot);
            if (result.Faulted)
                return result;
        }
    }

    /// <summary>
    ///     Same as move up but with implicit braces insertion
    /// </summary>
    private Result MoveUpForgiving(scoped ref LinkedNode<IMathNode?> levelRoot)
    {
        var prevLevel = levelRoot;
        if (!SimpleMoveUp(ref levelRoot))
            return Err("Invalid expression");

        if (prevLevel.Value is null)
            return Err("Invalid expression");

        var node = prevLevel.Value;
        if (node.Priority is SpecialNodePriority)
            switch (node)
            {
                case CustomFunctionNode:
                    return Err("Invalid amount of braces in custom function");
                case ComputedVariableNode variable:
                    return SetVariable(variable);
            }

        if (node is FunctionCallNode fnNode)
        {
            if (fnNode.SkipOverSlot)
                return Err("No value after ','");

            if (fnNode.Name.StartsWith('$'))
            {
                if (fnNode.Name is "$abs")
                    fnNode.Name = "abs";
                else
                    return Err($"Invalid {fnNode.Name.Substring(1)} syntax");
            }
        }

        node.Priority = ValueNodePriority;

        return AddNode(levelRoot, node, true);
    }

    // When an ')' occurred
    private Result MoveUp(scoped ref LinkedNode<IMathNode?> levelRoot)
    {
        var prevLevel = levelRoot;
        if (!SimpleMoveUp(ref levelRoot))
            return Err("Invalid expression");

        if (prevLevel.Value is null)
            return Err("Invalid expression");

        var node = prevLevel.Value;
        if (node.Priority is SpecialNodePriority)
            switch (node)
            {
                case CustomFunctionNode:
                    return Err("Invalid amount of braces in custom function");
                case ComputedVariableNode variable:
                    return SetVariable(variable);
            }

        if (node is FunctionCallNode fnNode)
        {
            if (fnNode.SkipOverSlot)
                return Err("No value after ','");

            if (fnNode.Name.StartsWith('$'))
                return Err($"Invalid {fnNode.Name.Substring(1)} syntax");
        }

        node.Priority = ValueNodePriority;

        return AddNode(levelRoot, node, true);
    }

    private bool SimpleMoveUp(ref LinkedNode<IMathNode?> levelRoot)
    {
        if (levelRoot.Previous is null)
            return false;

        levelRoot = levelRoot.Previous;
        return true;
    }

    private void MoveDownFnCall(ref LinkedNode<IMathNode?> levelRoot, string fnName)
    {
        MoveDown(ref levelRoot);
        levelRoot.Value = new FunctionCallNode(fnName);
    }

    private void MoveDownAndClear(ref LinkedNode<IMathNode?> levelRoot)
    {
        MoveDown(ref levelRoot);
        levelRoot.Value = null;
    }

    private void MoveDown(ref LinkedNode<IMathNode?> rootNode)
    {
        if (rootNode.Next is null)
            rootNode.ReplaceNext(new LinkedNode<IMathNode?>(null));

        rootNode = rootNode.Next!;
    }

    private Result AddNode(LinkedNode<IMathNode?> levelRoot, IMathNode node, bool spaceBeforeToken)
    {
        if (levelRoot.Value is null)
        {
            if (node.Priority != ValueNodePriority)
                return node.GenerateMissingValueError();

            levelRoot.Value = node;
            return Ok();
        }

        if (node.Priority != ValueNodePriority)
        {
            var result = AddFnNode(levelRoot, node);
            if (result.Faulted)
                return result;

            if (node is FactorialNode)
                node.Priority = ValueNodePriority;

            return result;
        }

        if (AddValueNode(levelRoot.Value, node))
            return Ok();

        if (spaceBeforeToken)
            return Err("Missing operator");

        AddFnNode(levelRoot, node is ValueNode ? new ExponentNode() : new MultiplyNode());

        AddValueNode(levelRoot.Value, node);
        return Ok();
    }

    private Result AddFnNode(LinkedNode<IMathNode?> levelRoot, IMathNode node)
    {
        IMathNode? secondToLastNode = null;
        var lastNode = levelRoot.Value!;

        if (node is ExponentNode)
            for (;;)
            {
                if (lastNode is not ExponentNode)
                    break;

                var nodeTmp = lastNode.GetLastNode();
                if (nodeTmp is null)
                    goto SkipToInteract;
                secondToLastNode = lastNode;
                lastNode = nodeTmp;
            }

        for (;;)
        {
            if (lastNode.Priority >= node.Priority)
                break;

            var nodeTmp = lastNode.GetLastNode();
            if (nodeTmp is null)
                break;
            secondToLastNode = lastNode;
            lastNode = nodeTmp;
        }

        SkipToInteract:
        // Interact
        // It's like a node is consuming and take
        // over the node that was there before

        if (lastNode.Priority is SpecialNodePriority)
            return node.GenerateMissingValueError();

        if (lastNode.Priority is not ValueNodePriority && !lastNode.IsFull())
            return lastNode.GenerateMissingValueError();

        node.AddNode(lastNode);
        if (secondToLastNode is null)
            levelRoot.Value = node;
        else
            secondToLastNode.ChangeLastNodeTo(node);

        return Ok();
    }

    private bool AddValueNode(IMathNode levelRoot, IMathNode node)
    {
        var node1 = levelRoot;
        var lastNotFull = node1.IsFull() ? null : node1;

        for (;;)
        {
            node1 = node1.GetLastNode();
            if (node1 is null)
                break;

            if (!node1.IsFull())
                lastNotFull = node1;
        }

        if (lastNotFull is null)
            return false;

        lastNotFull.AddNode(node);
        return true;
    }

    private Result SetVariable(ComputedVariableNode variable)
    {
        var result = variable.Calc(Scope);
        if (result.Faulted)
            return result;

        return Scope.SetVariable(variable.Name, result.Value);
    }

    private Result SetVariable(SpanSegment<char> segment, ReadOnlySpan<char> math)
    {
        var span = math.Slice(segment.Start + 1, segment.Length - 1);
        var assignSymbolIndex = span.IndexOf('=');
        if (assignSymbolIndex is -1)
        {
            var op = span.ToString();

            switch (op)
            {
                case "raw":
                    Scope.Format = Format.Raw;
                    break;
                case "step":
                    Scope.SetOpt(CalculatorOption.Step);
                    break;
                case "tree":
                    Scope.SetOpt(CalculatorOption.Step | CalculatorOption.Tree);
                    break;
                case "solve":
                    Scope.SetOpt(CalculatorOption.Solve);
                    break;
                case "latex":
                    Scope.SetOpt(CalculatorOption.LaTeX);
                    break;
                case "latexdoc":
                    Scope.SetOpt(CalculatorOption.Step | CalculatorOption.LaTeX | CalculatorOption.LaTeXDoc);
                    break;
                case "render":
                    Scope.SetOpt(CalculatorOption.Render);
                    break;
            }

            return Ok();
        }

        var name = span.Slice(0, assignSymbolIndex);
        var value = span.Slice(assignSymbolIndex + 1);

        var c = value[0];
        if (c.IsDigit() || c is '-')
        {
            var parseResult = CalculatorHelpers.ParseNumber(value);
            if (parseResult.Faulted)
                return parseResult.Exception!;

            return Scope.SetVariable(name.ToString(), parseResult.Value);
        }

        switch (name)
        {
            case "format":
            case "fmt":
                switch (value)
                {
                    case "human":
                        Scope.Format = Format.Human;
                        break;
                    case "none":
                    case "raw":
                        Scope.Format = Format.Raw;
                        break;
                    case "hex":
                        Scope.Format = Format.Hex;
                        break;
                    case "octal":
                    case "oct":
                        Scope.Format = Format.Octal;
                        break;
                    case "binary":
                    case "bin":
                        Scope.Format = Format.Binary;
                        break;
                    default:
                        return Err($"Unknown format: {value}");
                }

                break;
        }

        return Ok();
    }

    private static void SplitIntoChunks(ReadOnlySpan<char> math,
        ref Span<SpanSegment<char>> output, out int count)
    {
        count = 0;

        var start = 0;
        var prevType = GetCharacterType(math[0]);

        for (var i = 1; i < math.Length; i++)
        {
            var c = math[i];

            var type = GetCharacterType(c);
            if (type == prevType && type is not CharacterType.Other)
                continue;

            // From start -> prev char
            output[count] = new SpanSegment<char>(start, i - start);

            start = i;
            prevType = type;
            count++;
        }

        output[count] = new SpanSegment<char>(start, math.Length - start);
        count++;
    }

    private static CharacterType GetCharacterType(char c)
    {
        switch ((int)c)
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
                return CharacterType.Letter;
            case '<':
            case '=':
            case '>':
                return CharacterType.CompareSymbol;
            case ' ':
                return CharacterType.Space;
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
            case '.':
                return CharacterType.Number;
            default:
                return CharacterType.Other;
        }
    }

    private static bool IsValue(char c)
    {
        if (GetCharacterType(c) is CharacterType.Number or CharacterType.Letter)
            return true;

        return c is '(';
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

    private enum CharacterType
    {
        Letter,        // a-z              joined
        Number,        // 0-9              joined
        Space,         // ' '              joined
        CompareSymbol, // <, =, >          joined
        Other          // everything else  seperated
    }
}