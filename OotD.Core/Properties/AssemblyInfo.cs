using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

// Make internal classes visible to the test assembly
[assembly: InternalsVisibleTo("OotD.Core.Tests")]
