using System;
using System.Windows.Forms;
using Microsoft.Win32;
using OutlookDesktop.Properties;
using System.Globalization;

namespace OutlookDesktop
{
	/// <summary>
	/// Summary description for Prefernces.
	/// </summary>
	public class Preferences
	{		
        public const double DefaultOpacity  = 0.5;
        public const int DefaultLeftPosition = 100;
        public const int DefaultTopPosition = 100;
        public const int DefaultHeight = 500;
        public const int DefaultWidth = 700;

		private RegistryKey appReg;

		public Preferences()
		{
			try 
			{
				appReg = Registry.CurrentUser.CreateSubKey("Software\\"+Application.CompanyName+"\\"+Application.ProductName);
			} 
			catch (Exception ex) 
			{
                MessageBox.Show(ex.Message, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
		}

		~Preferences()
		{
			appReg.Close();
		}

		/// <summary>
		/// Returns true if there is a registry entry that makes Outlook on the Desktop start
		/// when Windows starts. On set, we save or delete that registry value
		/// accordingly.
		/// </summary>
		public static bool StartWithWindows
		{
			get
			{
				using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")) 
				{
					if (key != null) 
					{
						string val = (string) key.GetValue("OutlookOnDesktop");
						return (val != null && val.Length > 0);
					}
				}
				return false;
			}
			set
			{
				using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true)) 
				{
					if (key != null) 
					{
						try 
						{
							if (value) 
							{
								key.SetValue("OutlookOnDesktop", Application.ExecutablePath);
							} 
							else 
							{
								key.DeleteValue("OutlookOnDesktop");
							}
						} 
						catch (Exception) {}
					}
				}
			}
		}

		/// <summary>
		/// Main Window Opacity.
		/// </summary>
		public double Opacity
		{
			get
			{
                double opacity = DefaultOpacity;

                double.TryParse((string)appReg.GetValue("Opacity", opacity.ToString("G", CultureInfo.CurrentCulture)), out opacity);
                return opacity;
			}
			set
			{
				appReg.SetValue("Opacity", value);
			}
		}
		
		/// <summary>
		/// Main Window Left.
		/// </summary>
		public int Left
		{
			get
			{
                return (int)appReg.GetValue("Left", DefaultLeftPosition);
			}
			set
			{
				appReg.SetValue("Left", value);
			}
		}

		/// <summary>
		/// Main Window Top.
		/// </summary>
		public int Top
		{
			get
			{
                return (int)appReg.GetValue("Top", DefaultTopPosition);
			}
			set
			{
				appReg.SetValue("Top", value);
			}
		}

		/// <summary>
		/// Main Window Width.
		/// </summary>
		public int Width
		{
			get
			{
                return (int)appReg.GetValue("Width", DefaultWidth);
			}
			set
			{
				appReg.SetValue("Width", value);
			}
		}

		/// <summary>
		/// Main Window Height.
		/// </summary>
		public int Height
		{
			get
			{
                return (int)appReg.GetValue("Height", DefaultHeight);
			}
			set
			{
				appReg.SetValue("Height", value);
			}
		}

        public string FolderViewType
        {
            get
            {
                return (string)appReg.GetValue("CurrentViewType", "Calendar");
            }
            set
            {
                appReg.SetValue("CurrentViewType", value);
            }
        }
	}
}
