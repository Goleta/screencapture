//
// Copyright © 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal sealed class MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szDevice;

        public MONITORINFOEX()
        {
            this.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
            this.szDevice = new char[32];
        }

    }
}