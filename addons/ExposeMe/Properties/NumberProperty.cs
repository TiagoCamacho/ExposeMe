#if TOOLS
using Godot;
using Godot.Collections;
using System;
using System.Globalization;
using System.Reflection;

[Tool]
public partial class NumberProperty : BaseProperty
{
    private SpinBox _spinBox;
    private System.Type _memberType;
    private string _hintString = string.Empty;
    private bool _isInteger;

    public NumberProperty()
    {
    }

    public void Initialize(Node targetNode, MemberInfo member, Dictionary godotPropInfo, bool isInteger)
    {
        base.Initialize(targetNode, member);
        _memberType = member is PropertyInfo pi ? pi.PropertyType : ((FieldInfo)member).FieldType;
        _isInteger = isInteger;

        _hintString = godotPropInfo.TryGetValue("hint_string", out var hs)
            ? hs.AsString()
            : string.Empty;

        if (_spinBox != null)
        {
            ConfigureSpinBox();
            return;
        }

        _spinBox = new SpinBox();
        _spinBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        ConfigureSpinBox();
        _spinBox.ValueChanged += OnValueChanged;
        AddChild(_spinBox);
    }

    public override void _UpdateProperty()
    {
        if (!IsConfigured || _spinBox == null) return;
        Updating = true;
        _spinBox.Value = Convert.ToDouble(GetMemberValue());
        Updating = false;
    }

    private void OnValueChanged(double value)
    {
        if (Updating) return;
        SetMemberValue(ConvertToMemberType(value));
        EmitChanged(GetEditedProperty(), Variant.From(value));
    }

    private object ConvertToMemberType(double value)
    {
        if (_memberType == typeof(int)) return (int)value;
        if (_memberType == typeof(long)) return (long)value;
        if (_memberType == typeof(float)) return (float)value;
        return value;
    }

    private void ConfigureSpinBox()
    {
        _spinBox.MinValue = ParseHintPart(_hintString, 0) ?? -9999999;
        _spinBox.MaxValue = ParseHintPart(_hintString, 1) ?? 9999999;
        _spinBox.Step = ParseHintPart(_hintString, 2) ?? (_isInteger ? 1.0 : 0.01);
    }

    private static double? ParseHintPart(string hintString, int index)
    {
        if (string.IsNullOrEmpty(hintString)) return null;
        var parts = hintString.Split(',');
        if (index >= parts.Length) return null;
        return double.TryParse(parts[index].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var val)
            ? val
            : null;
    }
}
#endif
