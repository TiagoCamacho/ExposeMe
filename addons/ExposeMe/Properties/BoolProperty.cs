#if TOOLS
using Godot;
using System.Reflection;

[Tool]
public partial class BoolProperty : BaseProperty
{
    private CheckBox _checkBox;

    public BoolProperty()
    {
    }

    public override void Initialize(Node targetNode, MemberInfo member)
    {
        base.Initialize(targetNode, member);

        if (_checkBox != null) return;

        _checkBox = new CheckBox();
        _checkBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _checkBox.Toggled += OnToggled;
        AddChild(_checkBox);
    }

    public override void _UpdateProperty()
    {
        if (!IsConfigured || _checkBox == null) return;
        Updating = true;
        _checkBox.ButtonPressed = (bool)GetMemberValue();
        Updating = false;
    }

    private void OnToggled(bool pressed)
    {
        if (Updating) return;
        SetMemberValue(pressed);
        EmitChanged(GetEditedProperty(), Variant.From(pressed));
    }
}
#endif
