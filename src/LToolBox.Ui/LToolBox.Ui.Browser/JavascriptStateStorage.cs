using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace LToolBox.Ui.Browser;

[SupportedOSPlatform("browser")]
public sealed partial class JavascriptStateStorage
{
    [JSImport("save", "StateStorage")]
    public static partial void Save(string state);

    [JSImport("load", "StateStorage")]
    public static partial string? Load();

    [JSImport("invalidate", "StateStorage")]
    public static partial void Invalidate();
}