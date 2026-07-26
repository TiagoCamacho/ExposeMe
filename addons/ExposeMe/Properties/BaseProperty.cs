#if TOOLS
using Godot;
using Godot.Collections;
using System.Reflection;

/// <summary>
/// Shared base class for all ExposeMe property editors.
/// Stores the target child node and member info, and provides
/// helpers for reflection-based get/set.
/// </summary>
[Tool]
public abstract partial class BaseProperty : EditorProperty
{
    protected Node TargetNode;
    protected MemberInfo Member;
    protected bool Updating;
    protected bool IsConfigured;
    private string _memberName;
    private System.Type _memberType;

    protected BaseProperty()
    {
    }

    public virtual void Initialize(Node targetNode, MemberInfo member)
    {
        TargetNode = targetNode;
        Member = member;
        _memberName = member.Name;
        _memberType = member is PropertyInfo pi ? pi.PropertyType : ((FieldInfo)member).FieldType;
        IsConfigured = true;
    }

    protected object GetMemberValue()
    {
        if (!IsConfigured)
            throw new System.InvalidOperationException("ExposeMe property editor has not been initialized.");

        if (HasGodotProperty())
            return ConvertVariantToMemberType(TargetNode.Get(_memberName));

        if (CanUseReflection())
        {
            return Member is PropertyInfo pi
                ? pi.GetValue(TargetNode)
                : ((FieldInfo)Member).GetValue(TargetNode);
        }

        throw new System.InvalidOperationException(
            $"ExposeMe could not read '{_memberName}' from '{TargetNode.Name}'. " +
            "The member is not visible through Godot's property system and the editor only has a base Node wrapper.");
    }

    protected void SetMemberValue(object value)
    {
        if (!IsConfigured)
            throw new System.InvalidOperationException("ExposeMe property editor has not been initialized.");

        var root = EditorInterface.Singleton.GetEditedSceneRoot();
        var owner = TargetNode.Owner;
        if (owner != null && owner != root)
            root?.SetEditableInstance(owner, true);

        if (HasGodotProperty())
        {
            TargetNode.Set(_memberName, ToVariant(value));
            return;
        }

        if (CanUseReflection())
        {
            if (Member is PropertyInfo pi)
                pi.SetValue(TargetNode, value);
            else
                ((FieldInfo)Member).SetValue(TargetNode, value);
            return;
        }

        throw new System.InvalidOperationException(
            $"ExposeMe could not write '{_memberName}' on '{TargetNode.Name}'. " +
            "The member is not visible through Godot's property system and the editor only has a base Node wrapper.");
    }

    private bool CanUseReflection()
    {
        return Member.DeclaringType?.IsInstanceOfType(TargetNode) == true;
    }

    private bool HasGodotProperty()
    {
        foreach (var property in TargetNode.GetPropertyList())
        {
            if (property.TryGetValue("name", out var name) && name.AsString() == _memberName)
                return true;
        }

        return false;
    }

    private object ConvertVariantToMemberType(Variant value)
    {
        if (_memberType == typeof(bool)) return (bool)value;
        if (_memberType == typeof(int)) return (int)value;
        if (_memberType == typeof(long)) return (long)value;
        if (_memberType == typeof(float)) return (float)value;
        if (_memberType == typeof(double)) return (double)value;
        if (_memberType == typeof(string)) return (string)value;
        if (_memberType == typeof(Vector2)) return (Vector2)value;
        if (_memberType == typeof(Vector3)) return (Vector3)value;
        if (_memberType == typeof(Texture2D)) return (Texture2D)value;
        if (_memberType == typeof(Texture2D[])) return ToTextureArray(value);
        if (_memberType == typeof(Array<Texture2D>)) return ToTextureGodotArray(value);
        return value;
    }

    private static Texture2D[] ToTextureArray(Variant value)
    {
        if (value.VariantType == Variant.Type.Nil)
            return System.Array.Empty<Texture2D>();

        if (value.VariantType != Variant.Type.Array)
            return System.Array.Empty<Texture2D>();

        var source = (Array)value;
        var result = new Texture2D[source.Count];
        for (int i = 0; i < source.Count; i++)
            result[i] = source[i].As<Texture2D>();

        return result;
    }

    private static Array<Texture2D> ToTextureGodotArray(Variant value)
    {
        var result = new Array<Texture2D>();

        if (value.VariantType == Variant.Type.Nil)
            return result;

        if (value.VariantType != Variant.Type.Array)
            return result;

        var source = (Array)value;
        foreach (var item in source)
            result.Add(item.As<Texture2D>());

        return result;
    }

    protected static Variant ToVariant(object value)
    {
        if (value == null)
            return new Variant();

        if (value is bool b) return Variant.From(b);
        if (value is int i) return Variant.From(i);
        if (value is long l) return Variant.From(l);
        if (value is float f) return Variant.From(f);
        if (value is double d) return Variant.From(d);
        if (value is string s) return Variant.From(s);
        if (value is Vector2 v2) return Variant.From(v2);
        if (value is Vector3 v3) return Variant.From(v3);
        if (value is Texture2D texture) return Variant.From(texture);
        if (value is Texture2D[] textures) return Variant.From(ToVariantArray(textures));
        if (value is Array<Texture2D> textureArray) return Variant.From((Array)textureArray);
        if (value is Array array) return Variant.From(array);

        throw new System.InvalidOperationException(
            $"The type is not supported for conversion to/from Variant: '{value.GetType()}'");
    }

    private static Array ToVariantArray(Texture2D[] textures)
    {
        var result = new Array();
        foreach (var texture in textures)
            result.Add(texture);

        return result;
    }
}
#endif
