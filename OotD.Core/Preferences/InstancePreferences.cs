// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OotD.Preferences;

public class InstancePreferences
{
    public const int DefaultHeight = 500;
    public const int DefaultLeftPosition = 100;
    public const double DefaultOpacity = 0.5;
    public const int DefaultTopPosition = 100;
    public const int DefaultWidth = 700;

    private readonly RegistryKey _appReg;

    public InstancePreferences(string instanceName)
    {
        _appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + instanceName) ?? throw new InvalidOperationException();
    }

    /// <summary>
    /// Main Window Opacity.
    /// </summary>
    public double Opacity
    {
        get
        {
            var opacity = DefaultOpacity;

            return double.TryParse(
                _appReg.GetValue("Opacity", opacity.ToString("G", CultureInfo.CurrentCulture)).ToString(),
                out opacity)
                ? opacity
                : DefaultOpacity;
        }
        set => _appReg.SetValue("Opacity", value);
    }

    /// <summary>
    /// Main Window Left.
    /// </summary>
    public int Left
    {
#pragma warning disable CS8605 // Unboxing a possibly null value.
        get => (int)_appReg.GetValue("Left", DefaultLeftPosition);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        set => _appReg.SetValue("Left", value);
    }

    /// <summary>
    /// Main Window Top.
    /// </summary>
    public int Top
    {
#pragma warning disable CS8605 // Unboxing a possibly null value.
        get => (int)_appReg.GetValue("Top", DefaultTopPosition);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        set => _appReg.SetValue("Top", value);
    }

    /// <summary>
    /// Main Window Width.
    /// </summary>
    public int Width
    {
#pragma warning disable CS8605 // Unboxing a possibly null value.
        get => (int)_appReg.GetValue("Width", DefaultWidth);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        set => _appReg.SetValue("Width", value);
    }

    /// <summary>
    /// Main Window Height.
    /// </summary>
    public int Height
    {
#pragma warning disable CS8605 // Unboxing a possibly null value.
        get => (int)_appReg.GetValue("Height", DefaultHeight);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        set => _appReg.SetValue("Height", value);
    }

    public string? OutlookFolderName
    {
        get => _appReg.GetValue("CurrentViewType", "Calendar").ToString();
        set
        {
            if (value != null)
            {
                _appReg.SetValue("CurrentViewType", value);
            }

            OutlookFolderView = string.Empty;
        }
    }

    public string? OutlookFolderView
    {
        get => _appReg.GetValue("OutlookView", "Day/Week/Month").ToString();
        set
        {
            if (value != null)
            {
                _appReg.SetValue("OutlookView", value);
            }
        }
    }


    public string? OutlookFolderEntryId
    {
        get => _appReg.GetValue("FolderEntryId", "").ToString();
        set
        {
            if (value != null)
            {
                _appReg.SetValue("FolderEntryId", value);
            }
        }
    }


    public string? OutlookFolderStoreId
    {
        get => _appReg.GetValue("FolderStoreId", "").ToString();
        set
        {
            if (value != null)
            {
                _appReg.SetValue("FolderStoreId", value);
            }
        }
    }

    public bool DisableEditing
    {
        get
        {
            _ = bool.TryParse(_appReg.GetValue("DisableEditing", "False").ToString(), out var retVal);
            return retVal;
        }
        set => _appReg.SetValue("DisableEditing", value);
    }

    public string? ViewXml
    {
        get => _appReg.GetValue("ViewXML", "").ToString();
        set => _appReg.SetValue("ViewXML", value!);
    }

    ~InstancePreferences()
    {
        _appReg.Close();
    }
}
