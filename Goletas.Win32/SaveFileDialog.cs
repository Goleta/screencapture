//
// Copyright © 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System;

    internal sealed class SaveFileDialog : FileDialog
    {
        protected override bool ShowDialog()
        {
            return NativeMethods.GetSaveFileNameW(ref this.dialog);
        }

        public SaveFileDialog()
        {
            this.dialog.Flags |= NativeMethods.OFN_NOREADONLYRETURN | NativeMethods.OFN_PATHMUSTEXIST | NativeMethods.OFN_OVERWRITEPROMPT;
        }

    }
}