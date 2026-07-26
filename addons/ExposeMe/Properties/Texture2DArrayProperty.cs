#if TOOLS
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Reflection;

[Tool]
public partial class Texture2DArrayProperty : BaseProperty
{
    private const float HeaderMinHeight = 26.0f;

    private VBoxContainer _container;
    private Label _headerLabel;
    private Button _foldButton;
    private VBoxContainer _body;
    private SpinBox _sizeSpin;
    private VBoxContainer _rowsContainer;
    private Button _addButton;
    private readonly List<Texture2D> _items = new();
    private System.Type _memberType;
    private bool _expanded = true;
    private bool _commitQueued;

    public Texture2DArrayProperty()
    {
    }

    public override void Initialize(Node targetNode, MemberInfo member)
    {
        base.Initialize(targetNode, member);
        _memberType = member is PropertyInfo pi ? pi.PropertyType : ((FieldInfo)member).FieldType;

        // Use full-width custom UI instead of the default left label column.
        DrawLabel = false;

        if (_container != null) return;

        _container = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        var headerRow = new HBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, HeaderMinHeight)
        };

        _headerLabel = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };

        _foldButton = new Button
        {
            ToggleMode = true,
            ButtonPressed = _expanded,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            SizeFlagsStretchRatio = 1.0f,
            Alignment = HorizontalAlignment.Left,
            CustomMinimumSize = new Vector2(0, HeaderMinHeight)
        };
        ApplyHeaderButtonStyle(_foldButton);
        _foldButton.Toggled += OnFoldToggled;

        headerRow.AddChild(_headerLabel);
        headerRow.AddChild(_foldButton);

        _body = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            Visible = _expanded
        };

        var sizeRow = new HBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        var sizeLabel = new Label { Text = "Size:" };
        _sizeSpin = new SpinBox
        {
            MinValue = 0,
            MaxValue = 999,
            Step = 1,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _sizeSpin.ValueChanged += OnSizeChanged;
        sizeRow.AddChild(sizeLabel);
        sizeRow.AddChild(_sizeSpin);

        _rowsContainer = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        _addButton = new Button
        {
            Text = "+ Add Texture",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _addButton.Pressed += OnAddPressed;

        _body.AddChild(sizeRow);
        _body.AddChild(_rowsContainer);
        _body.AddChild(_addButton);

        _container.AddChild(headerRow);
        _container.AddChild(_body);
        AddChild(_container);
    }

    public override void _UpdateProperty()
    {
        if (!IsConfigured || _container == null) return;

        Updating = true;
        _items.Clear();
        _items.AddRange(ToTextureList(GetMemberValue()));
        RebuildRows();
        Updating = false;
    }

    private void OnAddPressed()
    {
        if (!IsConfigured || Updating) return;

        _items.Add(null);
        ScheduleCommit();
    }

    private void OnFoldToggled(bool pressed)
    {
        _expanded = pressed;
        if (_body != null)
            _body.Visible = _expanded;
    }

    private void OnSizeChanged(double value)
    {
        if (!IsConfigured || Updating) return;

        var targetSize = (int)value;
        if (targetSize < 0)
            targetSize = 0;

        while (_items.Count < targetSize)
            _items.Add(null);

        while (_items.Count > targetSize)
            _items.RemoveAt(_items.Count - 1);

        ScheduleCommit();
    }

    private void OnResourceChanged(int index, Resource resource)
    {
        if (!IsConfigured || Updating) return;
        if (index < 0 || index >= _items.Count) return;

        _items[index] = resource as Texture2D;
        ScheduleCommit();
    }

    private void ScheduleCommit()
    {
        if (_commitQueued) return;
        _commitQueued = true;
        CallDeferred(MethodName.CommitAndRefresh);
    }

    private void CommitAndRefresh()
    {
        _commitQueued = false;
        if (!IsConfigured || _rowsContainer == null) return;

        Updating = true;
        var value = BuildMemberValue();
        SetMemberValue(value);

        var editedProperty = GetEditedProperty();
        if (!string.IsNullOrEmpty(editedProperty))
            EmitChanged(editedProperty, ToVariant(value));

        RebuildRows();
        Updating = false;
    }

    private void RebuildRows()
    {
        if (_headerLabel != null)
            _headerLabel.Text = string.IsNullOrWhiteSpace(Label) ? "Textures" : Label;

        if (_foldButton != null)
            _foldButton.Text = $"Array[Texture2D] (size {_items.Count})";

        if (_sizeSpin != null)
            _sizeSpin.Value = _items.Count;

        while (_rowsContainer.GetChildCount() > 0)
            _rowsContainer.GetChild(0).QueueFree();

        for (int i = 0; i < _items.Count; i++)
        {
            var row = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            var picker = new EditorResourcePicker
            {
                BaseType = nameof(Texture2D),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                EditedResource = _items[i]
            };

            var index = i;
            picker.ResourceChanged += resource => OnResourceChanged(index, resource);

            row.AddChild(picker);
            _rowsContainer.AddChild(row);
        }
    }

    private object BuildMemberValue()
    {
        if (_memberType == typeof(Texture2D[]))
            return _items.ToArray();

        if (_memberType == typeof(Array<Texture2D>))
        {
            var result = new Array<Texture2D>();
            foreach (var item in _items)
                result.Add(item);
            return result;
        }

        return _items.ToArray();
    }

    private static List<Texture2D> ToTextureList(object value)
    {
        var result = new List<Texture2D>();

        switch (value)
        {
            case null:
                return result;

            case Texture2D[] textures:
                result.AddRange(textures);
                return result;

            case Array<Texture2D> textures:
                foreach (var texture in textures)
                    result.Add(texture);
                return result;

            case Array variants:
                foreach (var item in variants)
                    result.Add(item.As<Texture2D>());
                return result;

            default:
                return result;
        }
    }

    private static void ApplyHeaderButtonStyle(Button button)
    {
        var normal = new StyleBoxFlat
        {
            BgColor = Color.Color8(93, 93, 93),
            BorderColor = Color.Color8(120, 120, 120),
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            CornerRadiusTopLeft = 3,
            CornerRadiusTopRight = 3,
            CornerRadiusBottomLeft = 3,
            CornerRadiusBottomRight = 3
        };

        var hover = (StyleBoxFlat)normal.Duplicate();
        hover.BgColor = Color.Color8(101, 101, 101);

        var pressed = (StyleBoxFlat)normal.Duplicate();
        pressed.BgColor = Color.Color8(86, 86, 86);

        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeStyleboxOverride("focus", hover);
        button.AddThemeColorOverride("font_color", Color.Color8(220, 220, 220));
    }
}
#endif