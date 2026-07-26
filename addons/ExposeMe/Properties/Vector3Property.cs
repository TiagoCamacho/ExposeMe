#if TOOLS
using Godot;
using System.Reflection;

[Tool]
public partial class Vector3Property : BaseProperty
{
    private SpinBox _spinX;
    private SpinBox _spinY;
    private SpinBox _spinZ;

    public Vector3Property()
    {
    }

    public override void Initialize(Node targetNode, MemberInfo member)
    {
        base.Initialize(targetNode, member);

        if (_spinX != null || _spinY != null || _spinZ != null) return;

        var hbox = new HBoxContainer();
        hbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        var labelX = new Label { Text = "x" };
        labelX.AddThemeColorOverride("font_color", Color.Color8(205, 92, 92)); // IndianRed

        _spinX = CreateSpinBox();

        var labelY = new Label { Text = "y" };
        labelY.AddThemeColorOverride("font_color", Color.Color8(46, 139, 87)); // SeaGreen

        _spinY = CreateSpinBox();

        var labelZ = new Label { Text = "z" };
        labelZ.AddThemeColorOverride("font_color", Color.Color8(147, 112, 219)); // MediumPurple

        _spinZ = CreateSpinBox();

        hbox.AddChild(labelX);
        hbox.AddChild(_spinX);
        hbox.AddChild(labelY);
        hbox.AddChild(_spinY);
        hbox.AddChild(labelZ);
        hbox.AddChild(_spinZ);

        _spinX.ValueChanged += _ => OnSpinChanged();
        _spinY.ValueChanged += _ => OnSpinChanged();
        _spinZ.ValueChanged += _ => OnSpinChanged();

        AddChild(hbox);
    }

    public override void _UpdateProperty()
    {
        if (!IsConfigured || _spinX == null || _spinY == null || _spinZ == null) return;
        Updating = true;
        var v = (Vector3)GetMemberValue();
        _spinX.Value = v.X;
        _spinY.Value = v.Y;
        _spinZ.Value = v.Z;
        Updating = false;
    }

    private void OnSpinChanged()
    {
        if (Updating) return;
        var v = new Vector3((float)_spinX.Value, (float)_spinY.Value, (float)_spinZ.Value);
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
