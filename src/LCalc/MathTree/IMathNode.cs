using System.Text;
using Common.Results;

namespace LCalc.MathTree;

internal interface IMathNode
{
    public int Priority { get; set; }

    public Result<double> Calc(Scope scope);

    /// <summary>
    ///     Add a node
    /// </summary>
    /// <param name="node"></param>
    /// <returns>true if the node was added, false otherwise</returns>
    public bool AddNode(IMathNode node);

    public bool IsFull();
    
    public void ChangeLastNodeTo(IMathNode node);
    
    public Result GenerateMissingValueError();

    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel = 1,
        bool showTree = false);

    public Result<int> GetDepth();
}