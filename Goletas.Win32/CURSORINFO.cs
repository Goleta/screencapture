//
// Copyright © 2005 - 2006 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System;
    using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	internal struct CURSORINFO
	{
        public int cbSize;
        public uint flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
	}
}