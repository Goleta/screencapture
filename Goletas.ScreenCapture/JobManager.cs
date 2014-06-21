//
// Copyright © 2007 - 2008 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture
{
    using System;
    using System.IO;
    using System.Text;
    using System.Globalization;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Drawing;
    using System.Diagnostics;
    using System.Threading;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    using Goletas.Win32;
    using Goletas.ScreenCapture.Drawing.Imaging;

    public static class JobManager
    {
        // add JobsInProgressCount

        private sealed class JobParameters
        {
            public readonly ImageArea ImageArea;
            public readonly IntPtr Window;

            public JobParameters(ImageArea imageArea)
            {
                this.ImageArea = imageArea;
            }

            public JobParameters(IntPtr window)
            {
                this.Window = window;
            }
        }

        internal static void AddJob(ImageArea imageArea)
        {
            ApplicationManager.StartStaThread(ProcessJob, new JobParameters(imageArea));
        }

        internal static void AddJob(IntPtr window)
        {
            ApplicationManager.StartStaThread(ProcessJob, new JobParameters(window));
        }

        private static void ProcessJob(object e)
        {
            JobParameters jobParams = (JobParameters)e;
            JobInfo jobInfo = Configuration.Current.Settings.GetJobInfo();
            System.Drawing.Bitmap output = null;

            switch (jobParams.ImageArea)
            {
                case ImageArea.CurrentScreen:
                    output = GetCurrentScreenImage(jobInfo.IncludeCursor);
                    break;

                case ImageArea.VirtualScreen:
                    output = GetVirtualScreenImage(jobInfo.IncludeCursor);
                    break;

                case ImageArea.ForegroundWindow:
                    output = GetWindowImage(NativeMethods.GetForegroundWindow(), jobInfo.IncludeCursor);
                    break;

                case ImageArea.SelectedWindow:
                    output = GetWindowImage(jobParams.Window, jobInfo.IncludeCursor);
                    break;
            }

            if (output != null)
            {
                SaveImage(jobInfo, output);
            }
        }

    /*    private static System.Drawing.Bitmap GetWindowImage(IntPtr window, bool includeCursor)
        {
            RECT windowBounds;

            if ((window != IntPtr.Zero) && NativeMethods.GetWindowRect(new HandleRef(null, window), out windowBounds))
            {
                RECT imageBounds;
                RECT virtualScreenBounds = NativeMethods.GetVirtualScreenBounds();

                if (windowBounds.Intersect(ref virtualScreenBounds, out imageBounds))
                {
                    int width = imageBounds.Right - imageBounds.Left;
                    int height = imageBounds.Bottom - imageBounds.Top;

                    Bitmap outputBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                    using (Graphics outputGraphics = Graphics.FromImage(outputBitmap))
                    {
                        outputGraphics.Clear(SystemColors.Desktop);

                        using (SharedDeviceContext sourceDC = new SharedDeviceContext(window))
                        {
                            IntPtr outputDC = outputGraphics.GetHdc();

                            try
                            {
                                int offsetX = (virtualScreenBounds.Left > windowBounds.Left) ? virtualScreenBounds.Left - windowBounds.Left : 0;
                                int offsetY = (virtualScreenBounds.Top > windowBounds.Top) ? virtualScreenBounds.Top - windowBounds.Top : 0;

                                NativeMethods.BitBlt(new HandleRef(outputGraphics, outputDC), width, height, new HandleRef(sourceDC, sourceDC.Handle), offsetX, offsetY);

                                if (includeCursor)
                                {
                                    CURSORINFO cursorInfo;
                                    cursorInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                                    if (NativeMethods.GetCursorInfo(out cursorInfo) && windowBounds.Contains(cursorInfo.ptScreenPos) && (cursorInfo.flags == NativeMethods.CURSOR_SHOWING))
                                    {
                                        using (IconData icon = new IconData(cursorInfo.hCursor))
                                        {
                                            NativeMethods.DrawIcon(new HandleRef(outputGraphics, outputDC), cursorInfo.ptScreenPos.X - icon.HotSpotX - imageBounds.Left, cursorInfo.ptScreenPos.Y - icon.HotSpotY - imageBounds.Top, new HandleRef(icon, cursorInfo.hCursor));
                                        }
                                    }
                                }

                                return outputBitmap;
                            }
                            finally
                            {
                                outputGraphics.ReleaseHdc();
                            }
                        }
                    }
                }
            }

            return null;
        }
     */

#warning experimental
        private static Bitmap GetWindowImage(IntPtr window, bool includeCursor)
        {
            RECT windowBounds;
            NativeMethods.GetWindowRect(new HandleRef(null, window), out windowBounds);

            RECT virtualScreenBounds = NativeMethods.GetVirtualScreenBounds();

            RECT imageBounds;
            windowBounds.Intersect(ref virtualScreenBounds, out imageBounds);

            int width = imageBounds.Right - imageBounds.Left;
            int height = imageBounds.Bottom - imageBounds.Top;

            Bitmap outputBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (Graphics outputGraphics = Graphics.FromImage(outputBitmap))
            {
                outputGraphics.Clear(SystemColors.Desktop);

                using (DeviceContext sourceDC = new DeviceContext())
                {
                    IntPtr outputDC = outputGraphics.GetHdc();

                    try
                    {
                        NativeMethods.BitBlt(new HandleRef(outputGraphics, outputDC), width, height, new HandleRef(sourceDC, sourceDC.Handle), imageBounds.Left, imageBounds.Top);

                        if (includeCursor)
                        {
                            CURSORINFO cursorInfo;
                            cursorInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                            if (NativeMethods.GetCursorInfo(out cursorInfo) && (cursorInfo.flags == NativeMethods.CURSOR_SHOWING))
                            {
                                using (IconData icon = new IconData(cursorInfo.hCursor))
                                {
                                    NativeMethods.DrawItem(new HandleRef(outputGraphics, outputDC), cursorInfo.ptScreenPos.X - imageBounds.Left - icon.HotSpotX, cursorInfo.ptScreenPos.Y - imageBounds.Top - icon.HotSpotY, new HandleRef(icon, cursorInfo.hCursor));
                                }
                            }
                        }
                    }
                    finally
                    {
                        outputGraphics.ReleaseHdc();
                    }
                }
            }

            return outputBitmap;
        }

        private static Bitmap GetVirtualScreenImage(bool includeCursor)
        {
            RECT virtualScreenBounds = NativeMethods.GetVirtualScreenBounds();

            int width = virtualScreenBounds.Right - virtualScreenBounds.Left;
            int height = virtualScreenBounds.Bottom - virtualScreenBounds.Top;
            
            Bitmap outputBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (Graphics outputGraphics = Graphics.FromImage(outputBitmap))
            {
                outputGraphics.Clear(SystemColors.Desktop);

                using (DeviceContext sourceDC = new DeviceContext())
                {
                    IntPtr outputDC = outputGraphics.GetHdc();

                    try
                    {
                        NativeMethods.BitBlt(new HandleRef(outputGraphics, outputDC), width, height, new HandleRef(sourceDC, sourceDC.Handle), virtualScreenBounds.Left, virtualScreenBounds.Top);

                        if (includeCursor)
                        {
                            CURSORINFO cursorInfo;
                            cursorInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                            if (NativeMethods.GetCursorInfo(out cursorInfo) && (cursorInfo.flags == NativeMethods.CURSOR_SHOWING))
                            {
                                using (IconData icon = new IconData(cursorInfo.hCursor))
                                {
                                    NativeMethods.DrawItem(new HandleRef(outputGraphics, outputDC), cursorInfo.ptScreenPos.X - virtualScreenBounds.Left - icon.HotSpotX, cursorInfo.ptScreenPos.Y - virtualScreenBounds.Top - icon.HotSpotY, new HandleRef(icon, cursorInfo.hCursor));
                                }
                            }
                        }
                    }
                    finally
                    {
                        outputGraphics.ReleaseHdc();
                    }
                }
            }

            return outputBitmap;
        }

        /// <summary>
        /// Captures the screen containing mouse pointer.
        /// </summary>
        /// <param name="includeCursor">
        /// <c>true</c> to capture the mouse pointer; otherwise, <c>false</c>.
        /// </param>
        /// <returns>
        /// The snapshot of the current screen.
        /// </returns>
        private static Bitmap GetCurrentScreenImage(bool includeCursor)
        {
            POINT screenId;
            MONITORINFOEX monitorInfo = new MONITORINFOEX();

            if (NativeMethods.GetCursorPos(out screenId) && NativeMethods.GetMonitorInfoW(NativeMethods.MonitorFromPoint(screenId, NativeMethods.MONITOR_DEFAULTTONEAREST), monitorInfo))
            {
                int width = monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left;
                int height = monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top;

                Bitmap outputBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                using (Graphics outputGraphics = Graphics.FromImage(outputBitmap))
                {
                    using (DeviceContext sourceDC = new DeviceContext(new string(monitorInfo.szDevice)))
                    {
                        IntPtr outputDC = outputGraphics.GetHdc();

                        try
                        {
                            NativeMethods.BitBlt(new HandleRef(outputGraphics, outputDC), width, height, new HandleRef(sourceDC, sourceDC.Handle), 0, 0);

                            if (includeCursor)
                            {
                                CURSORINFO cursorInfo;
                                cursorInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                                if (NativeMethods.GetCursorInfo(out cursorInfo) && (cursorInfo.flags == NativeMethods.CURSOR_SHOWING))
                                {
                                    using (IconData icon = new IconData(cursorInfo.hCursor))
                                    {
                                        NativeMethods.DrawItem(new HandleRef(outputGraphics, outputDC), cursorInfo.ptScreenPos.X - monitorInfo.rcMonitor.Left - icon.HotSpotX, cursorInfo.ptScreenPos.Y - monitorInfo.rcMonitor.Top - icon.HotSpotY, new HandleRef(icon, cursorInfo.hCursor));
                                    }
                                }
                            }

                            return outputBitmap;
                        }
                        finally
                        {
                            outputGraphics.ReleaseHdc();
                        }
                    }
                }
            }

            return null;
        }

        private static bool StartExternalApp(string fileName, string arguments)
        {
            Process ExternalApp = new Process();
            ExternalApp.StartInfo = new ProcessStartInfo(fileName, arguments);

            try
            {
                ExternalApp.Start();
            }
            catch (Win32Exception)
            {
                return false;
            }
            finally
            {
                if (ExternalApp != null)
                {
                    ExternalApp.Dispose();
                }
            }

            return true;
        }

        private static bool CopyToClipboard(Bitmap bitmap)
        {
            IDataObject Data = new DataObject();
            Data.SetData(DataFormats.Bitmap, true, bitmap);

            try
            {
                Clipboard.SetDataObject(Data, true, 2, 150);
            }
            catch (ExternalException)
            {
                return false;
            }

            return true;
        }

        private static string GetFilePath(JobInfo info)
        {
            bool addSeparator = (info.WorkingDirectory[info.WorkingDirectory.Length - 1] != '\\');

            if (info.FileNameLabel.Length != 0 && ((info.WorkingDirectory.Length + ((addSeparator ? 1 : 0) + info.FileNameLabel.Length) + 1 + info.FileType.Extensions[0].Length) < NativeMethods.MAX_PATH) && Directory.Exists(info.WorkingDirectory))
            {
                if (addSeparator)
                {
                    return info.WorkingDirectory + @"\" + info.FileNameLabel + "." + info.FileType.Extensions[0];
                }
                else
                {
                    return info.WorkingDirectory + info.FileNameLabel + "." + info.FileType.Extensions[0];
                }
            }
            
            return string.Empty;
        }

        private static void SaveImage(JobInfo info, Bitmap bitmap)
        {
            if (info == null || bitmap == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                if ((info.ImageDestinations & ImageDestinations.Clipboard) == ImageDestinations.Clipboard)
                {
                    while (!CopyToClipboard(bitmap))
                    {
                        if (Goletas.ScreenCapture.MessageBox.Show(null, ApplicationManager.GetString("ImageToClipboardFailed"), MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Cancel)
                        {
                            break;
                        }
                    }
                }

                if ((info.ImageDestinations & ImageDestinations.File) == ImageDestinations.File)
                {
                    string filePath = GetFilePath(info);
                    FileType renderer = info.FileType;

                    

                    bool forceSelectFile = false;
                    Goletas.Win32.SaveFileDialog dialog = null;

                    do
                    {
                        bool overwriteFile = info.UseFileOverwrite;

                        if (forceSelectFile || filePath.Length == 0 || (File.Exists(filePath) && (((File.GetAttributes(filePath) & (FileAttributes.ReadOnly | FileAttributes.System)) != (FileAttributes)0) || !overwriteFile)))
                        {
                            if (dialog == null)
                            {
                                dialog = new Goletas.Win32.SaveFileDialog();
                                dialog.Title = ApplicationManager.GetString("SaveImageAs");
                                dialog.InitialDirectory = (filePath.Length != 0) ? info.WorkingDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                                dialog.DefaultExtension = renderer.Extensions[0];
                                dialog.FileName = "*." + dialog.DefaultExtension;
                                dialog.FilterIndex = Configuration.Current.FileTypes.IndexOf(renderer.FormatId) + 1;
                            }

                            dialog.Filter = Configuration.Current.FileTypes.GetTypeFilter();

                            if (!dialog.ShowDialog(IntPtr.Zero))
                            {
                                return;
                            }

                            renderer = Configuration.Current.FileTypes[dialog.FilterIndex - 1];
                            filePath = dialog.FileName;
                            
                            if (File.Exists(filePath))
                            {
                                overwriteFile = true;
                            }
                        }

                        try
                        {
                            using (FileStream output = new FileStream(filePath, overwriteFile ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, 8 * 1024, FileOptions.None))
                            {
                                renderer.Save(bitmap, output);
                                bitmap.Dispose();

                                break;
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {

                        }
                        catch (IOException)
                        {

                        }

                        forceSelectFile = true;
                    }
                    while (true);

                    if (info.ExternalApp.Length != 0)
                    {
                        if (!(File.Exists(info.ExternalApp) && StartExternalApp(info.ExternalApp, "\"" + filePath + "\"")))
                        {
                            Goletas.ScreenCapture.MessageBox.Show(null, ApplicationManager.GetString("ExternalAppNotExecuted"), MessageBoxIcon.Exclamation);
                        }
                    }

                }
            }
            finally
            {
                bitmap.Dispose();
            }
        }

    }
}