//
// Copyright © 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System;
    using System.Windows;
    using System.Text;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using Goletas.ScreenCapture.Drawing.Imaging;

    internal abstract class FileDialog
    {
        protected OPENFILENAME dialog;
        private string _FileName;

        public string Title
        {
            get
            {
                return this.dialog.lpstrTitle;
            }
            set
            {
                this.dialog.lpstrTitle = value;
            }
        }

        public string InitialDirectory
        {
            get
            {
                return this.dialog.lpstrInitialDir;
            }
            set
            {
                this.dialog.lpstrInitialDir = value;
            }
        }

        public string Filter
        {
            set
            {
                this.dialog.lpstrFilter = value.Replace('|', '\0') + '\0';
            }
        }

        public int FilterIndex
        {
            get
            {
                return this.dialog.nFilterIndex;
            }
            set
            {
                this.dialog.nFilterIndex = value;
            }
        }

        public string DefaultExtension
        {
            get
            {
                return this.dialog.lpstrDefExt;
            }
            set
            {
                this.dialog.lpstrDefExt = value;
            }
        }

        public string FileName
        {
            get
            {
                return this._FileName;
            }
            set
            {
                if (value == null)
                {
                    this._FileName = string.Empty;
                }
                else
                {

                    if (value.Length >= NativeMethods.MAX_PATH)
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    this._FileName = value;
                }
            }
        }

        protected virtual IntPtr WndProc(IntPtr window, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == NativeMethods.WM_INITDIALOG)
            {
                NativeMethods.SetFocus(wParam);

                IntPtr dialog = NativeMethods.GetParent(window);

                NativeMethods.MoveToCenter(dialog);
               
                if (NativeMethods.GetParent(dialog) == IntPtr.Zero)
                {
                    NativeMethods.SendMessageW(dialog, NativeMethods.WM_SETICON, new IntPtr(NativeMethods.ICON_BIG), NativeMethods.GetApplicationIcon());
                    NativeMethods.BringToTop(new HandleRef(this, dialog), false);
                }
            }

            return IntPtr.Zero;
        }

        public bool ShowDialog(IntPtr owner)
        {
            int byteLength = this._FileName.Length << 1; // FileName setter validates the file length

            this.dialog.hwndOwner = owner;
            this.dialog.lpstrFile = Marshal.AllocCoTaskMem(NativeMethods.MAX_PATH * 2);

            try
            {
                if (byteLength != 0)
                {
                    NativeMethods.RtlMoveMemory(this.dialog.lpstrFile, this._FileName, new IntPtr(byteLength));
                }

                if (byteLength < NativeMethods.MAX_PATH * 2)
                {
                    Marshal.WriteInt16(this.dialog.lpstrFile, byteLength, 0);
                }

                if (this.ShowDialog())
                {
                    this._FileName = Marshal.PtrToStringUni(this.dialog.lpstrFile);

                    return true;
                }

                switch (NativeMethods.CommDlgExtendedError())
                {
                    case 0:
                        break;

                    case NativeMethods.CDERR_INITIALIZATION:
                    case NativeMethods.CDERR_MEMALLOCFAILURE:
                    case NativeMethods.FNERR_SUBCLASSFAILURE:
                        throw new OutOfMemoryException();

                    case NativeMethods.FNERR_INVALIDFILENAME:
                        throw new ArgumentException();

                    case NativeMethods.FNERR_BUFFERTOOSMALL:
                        throw new OverflowException();

                    default:
                        throw new InvalidOperationException();
                }
            }
            finally
            {
                if (this.dialog.lpstrFile != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(this.dialog.lpstrFile);
                    this.dialog.lpstrFile = IntPtr.Zero;
                }
            }

            return false;
        }

        protected abstract bool ShowDialog();

        protected FileDialog()
        {
            this._FileName = string.Empty;
            this.dialog.lStructSize = Marshal.SizeOf(typeof(OPENFILENAME));
            this.dialog.nMaxFile = NativeMethods.MAX_PATH;
            this.dialog.Flags = NativeMethods.OFN_HIDEREADONLY | NativeMethods.OFN_ENABLESIZING | NativeMethods.OFN_EXPLORER | NativeMethods.OFN_ENABLEHOOK;
            this.dialog.lpfnHook = new WndProc(this.WndProc);
        }

    }
}