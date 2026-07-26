#if TOOLS
using Godot;
using System.Reflection;

[Tool]
public partial class StringProperty : BaseProperty
{
    private LineEdit _lineEdit;

    public StringProperty()
    {
    }

    public override void Initialize(Node targetNode, MemberInfo member)
    {
        base.Initialize(targetNode, member);

        if (_lineEdit != null) return;

        _lineEdit = new LineEdit();
        _lineEdit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _lineEdit.TextChanged += OnTextChanged;
        AddChild(_lineEdit);
    }

    public override void _UpdateProperty()
    {
        if (!IsConfigured || _lineEdit == null) return;
        Updating = true;
        _lineEdit.Text = (string)GetMemberValue() ?? string.Empty;
        Updating = false;
    }

    private void OnTextChanged(string text)
    {
        if (Updating) return;
        SetMemberValue(text);
        EmitChanged(GetEditedProperty(), Variant.From(text));
    }
}
#endif
