//
// Copyright © 2006 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct HH_POPUP
	{
		public int cbStruct;
		public IntPtr hinst;
		public uint idString;
		public IntPtr pszText;
		public POINT pt;
		public uint clrForeground;
		public uint clrBackground;
		public RECT rcMargins;
		public string pszFont;

		public HH_POPUP(IntPtr pszText)
		{
			this.cbStruct = Marshal.SizeOf(typeof(HH_POPUP));
			this.hinst = IntPtr.Zero;
			this.idString = 0;
			this.pszText = pszText;
			this.pt = new POINT();
            this.clrForeground = uint.MaxValue;
            this.clrBackground = uint.MaxValue;
            this.rcMargins = new RECT(10, 8, 10, 8); // default: RECT(-1, -1, -1, -1)
			this.pszFont = null;
		}

	}
}