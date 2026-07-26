#if TOOLS
using Godot;
using System.Reflection;

[Tool]
public partial class Vector2Property : BaseProperty
{
    private SpinBox _spinX;
    private SpinBox _spinY;

    public Vector2Property()
    {
    }

    public override void Initialize(Node targetNode, MemberInfo member)
    {
        base.Initialize(targetNode, member);

        if (_spinX != null || _spinY != null) return;

        var hbox = new HBoxContainer();
        hbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        var labelX = new Label { Text = "x" };
        labelX.AddThemeColorOverride("font_color", Color.Color8(205, 92, 92)); // IndianRed

        _spinX = CreateSpinBox();

        var labelY = new Label { Text = "y" };
        labelY.AddThemeColorOverride("font_color", Color.Color8(46, 139, 87)); // SeaGreen

        _spinY = CreateSpinBox();

        hbox.AddChild(labelX);
        hbox.AddChild(_spinX);
        hbox.AddChild(labelY);
        hbox.AddChild(_spinY);

        _spinX.ValueChanged += _ => OnSpinChanged();
        _spinY.ValueChanged += _ => OnSpinChanged();

        AddChild(hbox);
    }

    public override void _UpdateProperty()
    {
        if (!IsConfigured || _spinX == null || _spinY == null) return;
        Updating = true;
        var v = (Vector2)GetMemberValue();
        _spinX.Value = v.X;
        _spinY.Value = v.Y;
        Updating = false;
    }

    private void OnSpinChanged()
    {
        if (Updating) return;
        var v = new Vector2((float)_spinX.Value, (float)_spinY.Value);
        SetMemberValue(v);
        EmitChanged(GetEditedProperty(), Variant.From(v));
    }

    private static SpinBox CreateSpinBox()
    {
        return new SpinBox
        {
            MinValue = -999999,
            MaxValue = 999999,
            Step = 0.1,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
    }
}
#endif
