//
// Copyright © 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture
{
    using System.Windows.Forms;
    using System.Globalization;

    public static class MessageBox
    {
        private static bool IsRightToLeft(IWin32Window owner)
        {
            for (Control e = (owner as Control); e != null; e = e.Parent)
            {
                if (e.RightToLeft != RightToLeft.Inherit)
                {
                    return (e.RightToLeft == RightToLeft.Yes);
                }
            }

            return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
        }

        public static DialogResult Show(IWin32Window owner, string text, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return System.Windows.Forms.MessageBox.Show(owner, text, ApplicationManager.ProductName, buttons, icon, defaultButton, IsRightToLeft(owner) ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : (MessageBoxOptions)0);
        }

        public static DialogResult Show(IWin32Window owner, string text, MessageBoxIcon icon)
        {
            return Show(owner, text, MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1);
        }

    }
}