using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace LToolBox.Ui.Browser;

[SupportedOSPlatform("browser")]
public sealed partial class JavascriptStateManager
{
    [JSImport("save", "stateManager")]
    public static partial void Save(string state);

    [JSImport("load", "stateManager")]
    public static partial string? Load();

    [JSImport("invalidate", "stateManager")]
    public static partial void Invalidate();

    [JSExport]
    public static void OnBeforeUnload()
    {
        App.SuspendHelper.SaveState();
    }
}