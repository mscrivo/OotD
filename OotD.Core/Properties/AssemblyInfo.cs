using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

// Make internal classes visible to the test assembly
[assembly: InternalsVisibleTo("OotD.Core.Tests")]
