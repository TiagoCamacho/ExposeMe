#if TOOLS
using Godot;
using Godot.Collections;
using System.Reflection;
using System.Text;

[Tool]
public partial class ExposeInspector : EditorInspectorPlugin
{
    private static readonly Color DefaultFontColor = Color.Color8(203, 204, 205);
    private static readonly Color DefaultBackgroundColor = Color.Color8(57, 57, 57);
    private const int DefaultFontSize = 13;
    private const int DefaultCornerRadius = 2;

    public override bool _CanHandle(GodotObject obj) => obj is Node;

    public override void _ParseBegin(GodotObject obj)
    {
        if (obj is not Node node) return;
        if (GetScriptType(node).GetCustomAttribute<ExposerNodeAttribute>() == null) return;
        ScanChildren(node);
    }

    private void ScanChildren(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            var childType = GetScriptType(child);
            var members = GetExposeMembers(childType);

            if (members.Length > 0)
            {
                var style = childType.GetCustomAttribute<ExposeStyleAttribute>();
                AddCustomControl(CreateGroupLabel(child.Name, style));

                foreach (var member in members)
                {
                    var editor = CreatePropertyEditor(child, member);
                    if (editor != null)
                    {
                        editor.Label = Humanize(member.Name);
                        AddCustomControl(editor);
                    }
                }
            }

            ScanChildren(child);
        }
    }

    private static readonly System.Collections.Generic.Dictionary<string, System.Type> _typeCache = new();

    private static System.Type GetScriptType(Node node)
    {
        var type = node.GetType();

        var scriptVariant = node.GetScript();
        if (scriptVariant.VariantType == Variant.Type.Nil) return type;

        var script = scriptVariant.As<Script>();
        if (script == null) return type;

        var path = script.ResourcePath;
        if (string.IsNullOrEmpty(path)) return type;

        if (_typeCache.TryGetValue(path, out var cached)) return cached;

        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var directMatch = assembly.GetType(type.FullName ?? string.Empty);
            if (directMatch != null)
            {
                foreach (var scriptPath in directMatch.GetCustomAttributes<ScriptPathAttribute>())
                {
                    if (scriptPath.Path == path)
                    {
                        _typeCache[path] = directMatch;
                        return directMatch;
                    }
                }
            }
        }

        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var t in assembly.GetTypes())
            {
                foreach (var scriptPath in t.GetCustomAttributes<ScriptPathAttribute>())
                {
                    if (scriptPath.Path == path)
                    {
                        _typeCache[path] = t;
                        return t;
                    }
                }
            }
        }

        _typeCache[path] = type;
        return type;
    }

    private static MemberInfo[] GetExposeMembers(System.Type type)
    {
        var members = new System.Collections.Generic.List<MemberInfo>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetCustomAttribute<ExposeAttribute>() != null && prop.CanRead && prop.CanWrite)
                members.Add(prop);
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.GetCustomAttribute<ExposeAttribute>() != null)
                members.Add(field);
        }

        return members.ToArray();
    }

    private static EditorProperty CreatePropertyEditor(Node child, MemberInfo member)
    {
        var memberType = member is PropertyInfo pi ? pi.PropertyType : ((FieldInfo)member).FieldType;
        var godotProp = GetGodotPropertyInfo(child, member.Name);

        if (memberType == typeof(bool))
        {
            var editor = new BoolProperty();
            editor.Initialize(child, member);
            return editor;
        }

        if (memberType == typeof(int) || memberType == typeof(long))
        {
            var editor = new NumberProperty();
            editor.Initialize(child, member, godotProp, isInteger: true);
            return editor;
        }

        if (memberType == typeof(float) || memberType == typeof(double))
        {
            var editor = new NumberProperty();
            editor.Initialize(child, member, godotProp, isInteger: false);
            return editor;
        }

        if (memberType == typeof(string))
        {
            var editor = new StringProperty();
            editor.Initialize(child, member);
            return editor;
        }

        if (memberType == typeof(Vector2))
        {
            var editor = new Vector2Property();
            editor.Initialize(child, member);
            return editor;
        }

        if (memberType == typeof(Vector3))
        {
            var editor = new Vector3Property();
            editor.Initialize(child, member);
            return editor;
        }

        if (memberType == typeof(Texture2D))
        {
            var editor = new Texture2DProperty();
            editor.Initialize(child, member);
            return editor;
        }

        // if (memberType == typeof(Texture2D[]) || memberType == typeof(Array<Texture2D>))
        // {
        //     var editor = new Texture2DArrayProperty();
        //     editor.Initialize(child, member);
        //     return editor;
        // }

        return null;
    }

    private static Dictionary GetGodotPropertyInfo(Node node, string propertyName)
    {
        foreach (var prop in node.GetPropertyList())
        {
            if (prop.TryGetValue("name", out var name) && name.AsString() == propertyName)
                return prop;
        }
        return new Dictionary();
    }

    private static Label CreateGroupLabel(string name, ExposeStyleAttribute style)
    {
        var fontColor = ParseColorOrDefault(style?.FontColorHex, DefaultFontColor);
        var backgroundColor = ParseColorOrDefault(style?.BackgroundColorHex, DefaultBackgroundColor);
        var fontSize = style?.FontSize > 0 ? style.FontSize : DefaultFontSize;

        var label = new Label
        {
            Text = string.IsNullOrWhiteSpace(style?.Label) ? name : style.Label,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        label.AddThemeColorOverride("font_color", fontColor);
        label.AddThemeFontSizeOverride("font_size", fontSize);

        var stylebox = new StyleBoxFlat
        {
            BgColor = backgroundColor,
            CornerRadiusTopLeft = DefaultCornerRadius,
            CornerRadiusTopRight = DefaultCornerRadius,
            CornerRadiusBottomLeft = DefaultCornerRadius,
            CornerRadiusBottomRight = DefaultCornerRadius
        };
        label.AddThemeStyleboxOverride("normal", stylebox);

        return label;
    }

    private static Color ParseColorOrDefault(string colorHex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(colorHex)) return fallback;

        var value = colorHex.Trim();
        if (!value.StartsWith("#"))
            value = $"#{value}";

        if (!value.IsValidHtmlColor())
            return fallback;

        return Color.FromString(value, fallback);
    }

    private static string Humanize(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
                sb.Append(' ');
            sb.Append(name[i]);
        }
        return sb.ToString();
    }
}
#endif
