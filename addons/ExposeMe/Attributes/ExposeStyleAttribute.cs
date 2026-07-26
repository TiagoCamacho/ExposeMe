using System;

/// <summary>
/// Customizes the group header shown for a child node's exposed properties.
/// All values are optional and fall back to the plugin defaults when omitted.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class ExposeStyleAttribute : Attribute
{
    public string BackgroundColorHex { get; set; }

    public string FontColorHex { get; set; }

    public int FontSize { get; set; }

    public string Label { get; set; }
}