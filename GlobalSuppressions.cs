// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Many methods do not start with capitals in order to align with components of the forms.", Scope = "namespaceanddescendants", Target = "~N:JDP")]
[assembly: SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "Strings", Scope = "namespaceanddescendants", Target = "~N:JDP")]
[assembly: SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "Certain sites utilize MD5 as a checksum for attachments to posts", Scope = "member", Target = "~M:JDP.General.Calculate64BitMD5(System.Byte[])~System.UInt64")]
[assembly: SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "<Pending>", Scope = "member", Target = "~M:JDP.HashGeneratorStream.#ctor(JDP.HashType)")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "namespaceanddescendants", Target = "~N:JDP")]
[assembly: SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "Small exclusion to reduce 'messages' in the error list. This syntax isn't compatible with .NET 2.0", Scope = "namespaceanddescendants", Target = "~N:JDP")]
