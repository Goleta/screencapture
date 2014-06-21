//
// Copyright © 2007 - 2008 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture
{
    using System;
    using System.IO;
    using Microsoft.Win32;
    using System.Globalization;
    using System.Runtime.InteropServices;

    using Goletas.ScreenCapture.Drawing.Imaging;

    public static class Configuration
    {
        public sealed class Items
        {
            private Settings _Settings;
            private FileTypeCollection _FileTypes;

            public Settings Settings
            {
                get
                {
                    return this._Settings;
                }
            }

            public FileTypeCollection FileTypes
            {
                get
                {
                    return this._FileTypes;
                }
            }

            internal Items(Settings settings, FileTypeCollection fileTypes)
            {
                this._Settings = settings;
                this._FileTypes = fileTypes;
            }
        }

        private static Items _Current = Load();

        public static Items Current
        {
            get
            {
                return _Current;
            }
        }

        private static Items Load()
        {
            Settings settings = new Settings();
            FileType[] supportedTypes = new FileType[] { new PngFileType(), new BmpFileType(), new TiffFileType(), new JpegFileType(), new GifFileType() };
            FileTypeCollection fileTypes = new FileTypeCollection(supportedTypes);

            using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\Goletas\ScreenCapture", RegistryKeyPermissionCheck.ReadSubTree))
            {
                if (Key != null)
                {
                    try
                    {
                        for (int i = 0; i < supportedTypes.Length; i++)
                        {
                            if (supportedTypes[i].IsEditable)
                            {
                                supportedTypes[i].Deserialize(Key.GetValue(supportedTypes[i].FormatId.ToString("B")) as byte[]);
                            }
                        }

                        settings.IncludeCursor = Settings.Int32ToBoolTrueOnFail(Key.GetValue("IncludeCursor"));
                        settings.AlwaysOnTop = Settings.Int32ToBoolFalseOnFail(Key.GetValue("AlwaysOnTop"));
                        if (settings.UpdateFileNameFormat(Key.GetValue("FileName") as string))
                        {
                            settings.ImageDestinations = Settings.ToImageDestinations(Key.GetValue("ImageDestinations"));
                        }
                        settings.WorkingDirectory = Key.GetValue("WorkingDirectory") as string;
                        settings.UseExternalApp = Settings.Int32ToBoolFalseOnFail(Key.GetValue("UseExternalApp"));
                        settings.ExternalApp = Key.GetValue("ExternalApp") as string;
                        settings.UseFileOverwrite = Settings.Int32ToBoolFalseOnFail(Key.GetValue("UseFileOverwrite"));

                        string FormatId = Key.GetValue("FileType") as string;
                        int TypeIndex;

                        if (Settings.IsGuid(FormatId) && ((TypeIndex = fileTypes.IndexOf(new Guid(FormatId))) >= 0))
                        {
                            settings.FileType = fileTypes[TypeIndex];
                        }
                    }
                    catch (IOException)
                    {

                    }
                }
            }

            using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadSubTree))
            {
                if (Key != null)
                {
                    try
                    {
                        settings.AutoStartup = ((Key.GetValue("Goletas.ScreenCapture") as string) == ("\"" + System.Windows.Forms.Application.ExecutablePath + "\" background"));
                    }
                    catch (IOException)
                    {

                    }
                }
            }

            if (settings.FileType == null)
            {
                settings.FileType = fileTypes[0];
            }

            if (!Settings.IsValidPath(settings.WorkingDirectory))
            {
                settings.WorkingDirectory = null;
                settings.ImageDestinations = ImageDestinations.Clipboard;
            }

            if (!Settings.IsValidPathToExecutable(settings.ExternalApp))
            {
                settings.ExternalApp = null;
                settings.UseExternalApp = false;
            }

            return new Items(Settings.Synchronized(settings), fileTypes);
        }

        internal static void Save(FileType fileType)
        {
            if (fileType == null)
            {
                throw new ArgumentNullException();
            }

            if (!fileType.IsEditable)
            {
                throw new ArgumentException();
            }

            using (RegistryKey Key = Registry.CurrentUser.CreateSubKey(@"Software\Goletas\ScreenCapture", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (Key != null)
                {
                    Key.SetValue(fileType.FormatId.ToString("B"), fileType.Serialize(), RegistryValueKind.Binary);
                }
            }
        }

        internal static void Save(Settings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException();
            }

            if (settings.IsSynchronized)
            {
                throw new ArgumentException();
            }

            using (RegistryKey Key = Registry.CurrentUser.CreateSubKey(@"Software\Goletas\ScreenCapture", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (Key != null)
                {
                    Key.SetValue("IncludeCursor", settings.IncludeCursor, RegistryValueKind.DWord);
                    Key.SetValue("AlwaysOnTop", settings.AlwaysOnTop, RegistryValueKind.DWord);
                    Key.SetValue("ImageDestinations", settings.ImageDestinations, RegistryValueKind.DWord);
                    Key.SetValue("FileName", settings.FileNameFormat, RegistryValueKind.String);
                //    Key.SetValue("FileAutoNaming", settings.FileAutoNaming, RegistryValueKind.DWord);
                    Key.SetValue("WorkingDirectory", settings.WorkingDirectory, RegistryValueKind.String);
                    Key.SetValue("UseExternalApp", settings.UseExternalApp, RegistryValueKind.DWord);
                    Key.SetValue("ExternalApp", settings.ExternalApp, RegistryValueKind.String);
                    Key.SetValue("UseFileOverwrite", settings.UseFileOverwrite, RegistryValueKind.DWord);
                    Key.SetValue("FileType", settings.FileType.FormatId.ToString("B"), RegistryValueKind.String);
                }
            }

            using (RegistryKey Key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (Key != null)
                {
                    if (settings.AutoStartup)
                    {
                        Key.SetValue("Goletas.ScreenCapture", "\"" + System.Windows.Forms.Application.ExecutablePath + "\" background", RegistryValueKind.String);
                    }
                    else
                    {
                        Key.DeleteValue("Goletas.ScreenCapture", false);
                    }
                }
            }
        }

        internal static void Remove()
        {
            using (RegistryKey software = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (software != null)
                {
                    int entries = 1;

                    using (RegistryKey company = software.OpenSubKey("Goletas", RegistryKeyPermissionCheck.ReadWriteSubTree))
                    {
                        if (company != null)
                        {
                            string[] products = company.GetSubKeyNames();
                            entries = products.Length;

                            for (int i = 0; i < products.Length; i++)
                            {
                                if (products[i].Equals("ScreenCapture", StringComparison.OrdinalIgnoreCase))
                                {
                                    company.DeleteSubKeyTree("ScreenCapture");
                                    entries--;

                                    break;
                                }
                            }

                            if (entries == 0)
                            {
                                entries = company.ValueCount;
                            }
                        }
                    }

                    if (entries == 0)
                    {
                        software.DeleteSubKey("Goletas", false);
                    }
                }
            }

            using (RegistryKey Run = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (Run != null)
                {
                    Run.DeleteValue("Goletas.ScreenCapture", false);
                }
            }
        }

    }
}