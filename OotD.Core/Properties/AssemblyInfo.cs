﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
