//
// Copyright © 2005 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurityAttribute]
    internal delegate IntPtr LowLevelMouseProc(int code, IntPtr wParam, ref MSLLHOOKSTRUCT lParam);
}