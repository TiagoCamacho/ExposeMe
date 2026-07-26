using System;

/// <summary>
/// Marks a node class as an importer that aggregates <see cref="ExposeAttribute"/>
/// properties from its descendants into its own inspector panel.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class ExposerNodeAttribute : Attribute { }
