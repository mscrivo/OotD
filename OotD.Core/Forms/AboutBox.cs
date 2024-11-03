﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OotD.Properties;

namespace OotD.Forms;

public sealed partial class AboutBox : Form
{
    public AboutBox()
    {
        InitializeComponent();

        //  Initialize the AboutBox to display the product information from the assembly information.
        //  Change assembly information settings for your application through either:
        //  - Project->Properties->Application->Assembly Information
        //  - AssemblyInfo.cs
        Text = string.Format(Resources.AboutOotD, Title);
        labelProductName.Text = Product;
        labelVersion.Text = string.Format(Resources.AboutVersion, Version);
        labelCopyright.Text = CopyRight;
    }

    private void OKButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    private void LinkWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            OpenUrl("https://outlookonthedesktop.com");
        }
        catch
        {
            MessageBox.Show(this, Resources.ErrorLaunchingWebsite, Resources.ErrorCaption,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PicDonate_Click(object sender, EventArgs e)
    {
        try
        {
            OpenUrl(
                "https://www.paypal.com/cgi-bin/webscr?cmd=_xclick&business=mscrivo%40tfnet%2eca&item_name=Outlook%20on%20the%20Desktop%20Donation&amount=5%2e00&no_shipping=0&no_note=1&tax=0&currency_code=USD&lc=CA&bn=PP%2dDonationsBF&charset=UTF%2d8");
        }
        catch
        {
            MessageBox.Show(this, Resources.ErrorLaunchingWebsite, Resources.ErrorCaption,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #region Assembly Attribute Accessors

    private static string Title
    {
        get
        {
            // Get all Title attributes on this assembly
            var attributes =
                Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);

            // If there is at least one Title attribute
            if (attributes.Length <= 0)
            {
                return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            }

            // Select the first one
            var titleAttribute = (AssemblyTitleAttribute)attributes[0];
            // If it is not an empty string, return it
            return !string.IsNullOrEmpty(titleAttribute.Title)
                ? titleAttribute.Title
                : Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
        }
    }

    private static string Version
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return
                $"{fileVersionInfo.ProductMajorPart}.{fileVersionInfo.ProductMinorPart}.{fileVersionInfo.ProductBuildPart}";
        }
    }

    private static string Product
    {
        get
        {
            // Get all Product attributes on this assembly
            var attributes =
                Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            // If there aren't any Product attributes, return an empty string
            return attributes.Length == 0 ? "" : ((AssemblyProductAttribute)attributes[0]).Product;

            // If there is a Product attribute, return its value
        }
    }

    private static string CopyRight
    {
        get
        {
            // Get all Copyright attributes on this assembly
            var attributes =
                Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            // If there aren't any Copyright attributes, return an empty string
            return attributes.Length == 0 ? "" : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

            // If there is a Copyright attribute, return its value
        }
    }

    #endregion
}
