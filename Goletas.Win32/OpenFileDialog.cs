//
// Copyright © 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System;

    internal sealed class OpenFileDialog : FileDialog
    {
        protected override bool ShowDialog()
        {
            return NativeMethods.GetOpenFileNameW(ref this.dialog);
        }

        public OpenFileDialog()
        {
            this.dialog.Flags |= NativeMethods.OFN_FILEMUSTEXIST | NativeMethods.OFN_PATHMUSTEXIST;
        }

    }
}