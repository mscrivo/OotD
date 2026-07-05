// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace OotD.Utility;

/// <summary>
///     Why a usable Outlook installation could not be found, or <see cref="None" /> on success.
/// </summary>
public enum OutlookDetectionError
{
    None,
    OfficeNotInstalled,
    UnsupportedVersion,
    OutlookExecutableNotFound,
    OutlookLocationNotFound
}

/// <summary>
///     Result of probing the machine for a usable Microsoft Outlook installation.
/// </summary>
/// <param name="Error">The failure reason, or <see cref="OutlookDetectionError.None" /> on success.</param>
/// <param name="Bitness">"x86" or "x64" when <paramref name="Error" /> is None; otherwise an empty string.</param>
public sealed record OutlookInstallation(OutlookDetectionError Error, string Bitness)
{
    public bool IsUsable => Error == OutlookDetectionError.None;
}

/// <summary>
///     Reads the registry / file system to locate Outlook. Abstracted so the detection policy in
///     <see cref="OutlookInstallationDetector" /> can be unit tested without touching the machine.
/// </summary>
public interface IOutlookEnvironment
{
    /// <summary>Installed Office major versions (e.g. the "16.0" key -> 16), parsed from the Office registry key.</summary>
    IReadOnlyList<double> GetInstalledOfficeVersions();

    /// <summary>The folder Outlook.exe lives in, from the App Paths registry key, or null/empty when unknown.</summary>
    string? GetOutlookInstallPath();

    /// <summary>True if Outlook.exe exists inside <paramref name="installPath" />.</summary>
    bool OutlookExecutableExists(string installPath);

    /// <summary>The "Bitness" value ("x86"/"x64") recorded for the given Office version, or null/empty if absent.</summary>
    string? GetBitness(double officeVersion);
}

/// <summary>
///     Decides whether a usable Outlook installation is present and, if so, which process bitness OotD
///     must launch to match it (Outlook COM interop requires the host process to match Outlook's
///     bitness). This is pure policy -- every machine read goes through <see cref="IOutlookEnvironment" />.
/// </summary>
public static class OutlookInstallationDetector
{
    /// <summary>Office 2010 (14.0) is the minimum supported version.</summary>
    public const double MinimumSupportedVersion = 14;

    public static OutlookInstallation Detect(IOutlookEnvironment environment)
    {
        var version = environment.GetInstalledOfficeVersions().DefaultIfEmpty(0).Max();

        if (version <= 0)
        {
            return new OutlookInstallation(OutlookDetectionError.OfficeNotInstalled, string.Empty);
        }

        if (version < MinimumSupportedVersion)
        {
            return new OutlookInstallation(OutlookDetectionError.UnsupportedVersion, string.Empty);
        }

        var installPath = environment.GetOutlookInstallPath();

        if (!string.IsNullOrWhiteSpace(installPath) && !environment.OutlookExecutableExists(installPath))
        {
            return new OutlookInstallation(OutlookDetectionError.OutlookExecutableNotFound, string.Empty);
        }

        if (string.IsNullOrEmpty(installPath))
        {
            return new OutlookInstallation(OutlookDetectionError.OutlookLocationNotFound, string.Empty);
        }

        return new OutlookInstallation(OutlookDetectionError.None, DetectBitness(environment, version));
    }

    private static string DetectBitness(IOutlookEnvironment environment, double version)
    {
        string? bitness = null;

        // Prefer the newest version's recorded bitness, falling back to older version keys if the
        // newest one doesn't record it. Defaults to x86 when nothing is recorded.
        while (string.IsNullOrWhiteSpace(bitness) && version - 1 >= MinimumSupportedVersion)
        {
            bitness = environment.GetBitness(version);
            if (string.IsNullOrWhiteSpace(bitness))
            {
                version--;
            }
            else
            {
                break;
            }
        }

        return string.IsNullOrWhiteSpace(bitness) ? "x86" : bitness;
    }
}
