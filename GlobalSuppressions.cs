// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Many methods do not start with capitals in order to align with components of the forms.", Scope = "namespaceanddescendants", Target = "JDP")]
[assembly: SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "Strings", Scope = "namespaceanddescendants", Target = "JDP")]
