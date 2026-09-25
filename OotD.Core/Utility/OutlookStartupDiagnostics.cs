// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using OotD.Properties;

namespace OotD.Utility;

/// <summary>
///     The known reasons Outlook's COM automation or the Outlook View Control can fail at startup.
/// </summary>
public enum OutlookStartupProblem
{
    Unknown,

    /// <summary>Outlook couldn't be started over COM (CO_E_SERVER_EXEC_FAILURE).</summary>
    ServerExecFailure,

    /// <summary>Outlook's COM interfaces or type library aren't registered correctly.</summary>
    RegistrationBroken,

    /// <summary>The Outlook COM classes (e.g. the View Control) aren't registered at all.</summary>
    ClassNotRegistered
}

/// <summary>
///     Turns the COM errors seen when starting Outlook or creating the Outlook View Control into
///     an explanation the user can act on, instead of a raw exception.
/// </summary>
public static class OutlookStartupDiagnostics
{
    private const int CO_E_SERVER_EXEC_FAILURE = unchecked((int)0x80080005);
    private const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154);
    private const int REGDB_E_IIDNOTREG = unchecked((int)0x80040155);
    private const int TYPE_E_LIBNOTREGISTERED = unchecked((int)0x8002801D);
    private const int TYPE_E_ELEMENTNOTFOUND = unchecked((int)0x8002802B);
    private const int TYPE_E_CANTLOADLIBRARY = unchecked((int)0x80029C4A);

    public static OutlookStartupProblem Classify(Exception exception)
    {
        for (var ex = exception; ex != null; ex = ex.InnerException)
        {
            switch (ex.HResult)
            {
                case CO_E_SERVER_EXEC_FAILURE:
                    return OutlookStartupProblem.ServerExecFailure;
                case REGDB_E_CLASSNOTREG:
                    return OutlookStartupProblem.ClassNotRegistered;
                case REGDB_E_IIDNOTREG:
                case TYPE_E_LIBNOTREGISTERED:
                case TYPE_E_ELEMENTNOTFOUND:
                case TYPE_E_CANTLOADLIBRARY:
                    return OutlookStartupProblem.RegistrationBroken;
            }

            // A failed QueryInterface on the Outlook Application object surfaces as an
            // InvalidCastException whose own HResult is E_NOINTERFACE; the underlying cause is a
            // broken interface registration.
            if (ex is InvalidCastException)
            {
                return OutlookStartupProblem.RegistrationBroken;
            }
        }

        return OutlookStartupProblem.Unknown;
    }

    /// <summary>
    ///     Builds the message to show the user for a startup failure.
    /// </summary>
    /// <param name="exception">The exception thrown while starting Outlook or the view control.</param>
    /// <param name="newOutlookEnabled">Whether Outlook is switched to the new Outlook for Windows.</param>
    public static string GetMessage(Exception exception, bool newOutlookEnabled)
    {
        var message = Classify(exception) switch
        {
            OutlookStartupProblem.ServerExecFailure => Resources.OutlookServerExecFailure,
            OutlookStartupProblem.RegistrationBroken => Resources.OutlookRegistrationBroken,
            OutlookStartupProblem.ClassNotRegistered => Resources.OutlookClassNotRegistered,
            _ => Resources.ErrorInitializingApp + Environment.NewLine + exception.Message
        };

        return WithNewOutlookHint(message, newOutlookEnabled);
    }

    /// <summary>
    ///     Appends advice to switch back to classic Outlook when the "New Outlook" toggle is on.
    /// </summary>
    public static string WithNewOutlookHint(string message, bool newOutlookEnabled)
    {
        return newOutlookEnabled
            ? message + Environment.NewLine + Environment.NewLine + Resources.NewOutlookEnabledHint
            : message;
    }
}
