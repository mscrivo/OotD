using OotD.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OotD.Forms
{
    public partial class AboutBox : Form
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

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        #region Assembly Attribute Accessors

        private static string? Title
        {
            get
            {
                // Get all Title attributes on this assembly
                var attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                // If there is at least one Title attribute
                if (attributes.Length > 0)
                {
                    // Select the first one
                    var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    // If it is not an empty string, return it
                    if (!string.IsNullOrEmpty(titleAttribute.Title))
                    {
                        return titleAttribute.Title;
                    }
                }
                // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
                return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        private static string Version
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fileVersionInfo.ProductVersion;
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
                if (attributes.Length == 0)
                {
                    return "";
                }

                // If there is a Product attribute, return its value
                return ((AssemblyProductAttribute)attributes[0]).Product;
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
                if (attributes.Length == 0)
                {
                    return "";
                }

                // If there is a Copyright attribute, return its value
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        #endregion

        private void OKButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenUrl(string url)
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
    }
}
