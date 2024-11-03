﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;

namespace OotD.Utility;

/// <summary>
///     Extension method to allow accessing properties and fields in a worker thread without throwing:
///     "Cross-thread operation not valid: Control 'MainForm' accessed from a thread other than the thread it was created
///     on."
/// </summary>
public static class SynchronizeInvokeExtensions
{
    public static void InvokeEx<T>(this T @this, Action<T> action) where T : ISynchronizeInvoke
    {
        if (@this.InvokeRequired)
        {
            @this.Invoke(action, [@this]);
        }
        else
        {
            action(@this);
        }
    }
}
