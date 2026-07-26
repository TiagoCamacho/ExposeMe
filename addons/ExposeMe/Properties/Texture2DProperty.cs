#if TOOLS
using Godot;
using System.Reflection;

[Tool]
public partial class Texture2DProperty : BaseProperty
{
    private EditorResourcePicker _picker;

    public Texture2DProperty()
    {
    }

    public override void Initialize(Node targetNode, MemberInfo member)
    {
        base.Initialize(targetNode, member);

        if (_picker != null) return;

        _picker = new EditorResourcePicker
        {
            BaseType = nameof(Texture2D),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        _picker.ResourceChanged += OnResourceChanged;
        AddChild(_picker);
    }

    public override void _UpdateProperty()
    {
        if (!IsConfigured || _picker == null) return;

        Updating = true;
        _picker.EditedResource = GetMemberValue() as Texture2D;
        Updating = false;
    }

    private void OnResourceChanged(Resource resource)
    {
        if (Updating) return;

        var texture = resource as Texture2D;
        SetMemberValue(texture);
        EmitChanged(GetEditedProperty(), Variant.From(texture));
    }
}
#endif