#if TOOLS
using Godot;

[Tool]
public partial class ExposeMe : EditorPlugin
{
    private ExposeInspector _inspector;

    public override void _EnterTree()
    {
        _inspector = new ExposeInspector();
        AddInspectorPlugin(_inspector);
    }

    public override void _ExitTree()
    {
        RemoveInspectorPlugin(_inspector);
    }
}
#endif
