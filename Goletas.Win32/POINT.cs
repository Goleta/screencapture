//
// Copyright © 2005 - 2006 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	internal struct POINT
	{
        public int X;
        public int Y;
	}
}