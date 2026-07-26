using System;

/// <summary>
/// Marks a public property or field on a child node to be exposed
/// in the parent node's inspector. The parent node must be marked
/// with <see cref="ExposerNodeAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ExposeAttribute : Attribute { }
