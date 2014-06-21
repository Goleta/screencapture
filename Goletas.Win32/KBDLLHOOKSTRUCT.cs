//
// Copyright © 2005 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System;
    using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
    internal struct KBDLLHOOKSTRUCT
	{
        public int vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
	}
}