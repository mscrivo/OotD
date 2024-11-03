// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage("Interoperability", "CA1416:Validate platform compatibility",
        Justification = "OotD is Windows only", Scope = "module")]
[assembly:
    SuppressMessage("Style", "IDE0073:The file header does not match the required text",
        Justification = "No header required in suppression file")]
