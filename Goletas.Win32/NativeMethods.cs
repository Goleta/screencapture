//
// Copyright © 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System;
    using System.Security;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
    using System.Windows.Forms;
    using System.ComponentModel;

    [SuppressUnmanagedCodeSecurityAttribute]
    internal static class NativeMethods
    {
        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern int GetSystemMetrics(int index);

        public static RECT GetVirtualScreenBounds()
        {
            RECT bounds;

            bounds.Left = GetSystemMetrics(SM_XVIRTUALSCREEN);
            bounds.Right = bounds.Left + GetSystemMetrics(SM_CXVIRTUALSCREEN);

            bounds.Top = GetSystemMetrics(SM_YVIRTUALSCREEN);
            bounds.Bottom = bounds.Top + GetSystemMetrics(SM_CYVIRTUALSCREEN);

            return bounds;
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr LoadIconW(IntPtr instance, IntPtr iconId);

        public static IntPtr GetApplicationIcon()
        {
            IntPtr Icon = LoadIconW(GetModuleHandle(), new IntPtr(IDI_APPLICATION));

            if (Icon == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return Icon;
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int PostMessageW(HandleRef window, int msg, IntPtr wParam, IntPtr lParam);

        public static void PostMessage(HandleRef window, int msg)
        {
            if ((window.Handle != IntPtr.Zero) && (PostMessageW(window, msg, IntPtr.Zero, IntPtr.Zero) == 0))
            {
                throw new Win32Exception();
            }
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern int PostThreadMessageW(int threadId, int msg, IntPtr wParam, IntPtr lParam);

        public static void PostThreadMessage(int threadId, int msg)
        {
            if ((threadId != 0) && (PostThreadMessageW(threadId, msg, IntPtr.Zero, IntPtr.Zero) == 0))
            {
                throw new Win32Exception();
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFileMappingW(IntPtr file, IntPtr attributes, uint protect, int maxSizeHigh, int maxSizeLow, string name);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern int CloseHandle(HandleRef handle);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr MapViewOfFile(HandleRef fileMappingObject, uint desiredAccess, int fileOffsetHigh, int fileOffsetLow, IntPtr numberOfBytesToMap);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern int UnmapViewOfFile(HandleRef baseAddress);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern void RtlMoveMemory(IntPtr destination, string source, IntPtr length);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr SetFocus(IntPtr window);

        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BringWindowToTop(IntPtr window);

        public static void MoveToCenter(IntPtr window)
        {
            RECT WindowBounds;
            POINT CursorPosition;

            if (GetCursorPos(out CursorPosition) && GetWindowRect(new HandleRef(null, window), out WindowBounds))
            {
                MONITORINFO Info;
                Info.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

                if (GetMonitorInfoW(MonitorFromPoint(CursorPosition, MONITOR_DEFAULTTONEAREST), out Info))
                {
                    SetWindowPos(window, IntPtr.Zero, (((Info.rcWork.Right - Info.rcWork.Left) - (WindowBounds.Right - WindowBounds.Left)) >> 1), (((Info.rcWork.Bottom - Info.rcWork.Top) - (WindowBounds.Bottom - WindowBounds.Top)) >> 1), 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
                }
            }
        }

        [DllImport("comdlg32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetOpenFileNameW(ref OPENFILENAME info);

        [DllImport("comdlg32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetSaveFileNameW(ref OPENFILENAME info);

        [DllImport("comdlg32.dll", ExactSpelling = true)]
        public static extern uint CommDlgExtendedError();

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetParent(IntPtr window);

        public static void ShowHelp(Control control, string message)
        {
            HH_POPUP popup = new HH_POPUP(Marshal.StringToCoTaskMemUni(message));

            try
            {
                if (GetCursorPos(out popup.pt))
                {
                    RECT WindowBounds;
                    HandleRef Parent = new HandleRef(control, control.Handle);

                    if (GetWindowRect(Parent, out WindowBounds))
                    {
                        if (!WindowBounds.Contains(popup.pt))
                        {
#warning TODO: proper alignment if partially off the screen

                            popup.pt.X = WindowBounds.Left + ((WindowBounds.Right - WindowBounds.Left) >> 1);
                            popup.pt.Y = WindowBounds.Top + ((WindowBounds.Bottom - WindowBounds.Top) >> 1);
                        }

                        HtmlHelpW(Parent, IntPtr.Zero, HH_DISPLAY_TEXT_POPUP, ref popup); // file parameter must be null
                    }
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(popup.pszText);
            }
        }

        [DllImport("hhctrl.ocx", ExactSpelling = true)]
        private static extern IntPtr HtmlHelpW(HandleRef control, IntPtr file, uint command, [In] ref HH_POPUP popup);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("user32.dll", ExactSpelling = true)]
        public static extern int ReleaseDC(HandleRef window, HandleRef sharedDeviceContext);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetWindowDC(HandleRef window);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr CreatePen(int style, int width, uint colorRef);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int GetIconInfo(HandleRef icon, out ICONINFO info);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr SetWindowsHookExW(int hookType, IntPtr callback, IntPtr module, uint threadId);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr CallNextHookEx(HandleRef hook, int code, IntPtr wParam, [In] ref KBDLLHOOKSTRUCT lParam);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr CallNextHookEx(HandleRef hook, int code, IntPtr wParam, [In] ref MSLLHOOKSTRUCT lParam);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("user32.dll", ExactSpelling = true)]
        public static extern int UnhookWindowsHookEx(HandleRef hook);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern int DeleteDC(HandleRef deviceContext);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("gdi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateDCW(string driverName, string deviceName, IntPtr output, IntPtr data);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern int BitBlt(HandleRef destination, int destinationLeft, int destinationTop, int width, int height, HandleRef source, int sourceLeft, int sourceTop, uint rasterOperation);

        public static void BitBlt(HandleRef destination, int width, int height, HandleRef source, int sourceLeft, int sourceTop)
        {
            if (BitBlt(destination, 0, 0, width, height, source, sourceLeft, sourceTop, NativeMethods.SRCCOPY | NativeMethods.CAPTUREBLT) == 0)
            {
                throw new Win32Exception();
            }
        }

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr SelectObject(HandleRef deviceContext, HandleRef graphicsObject);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern int DrawIcon(HandleRef deviceContext, int x, int y, HandleRef icon);

        public static void DrawItem(HandleRef deviceContext, int x, int y, HandleRef icon)
        {
            if (DrawIcon(deviceContext, x, y, icon) == 0)
            {
                throw new Win32Exception();
            }
        }

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern int Rectangle(HandleRef deviceContext, int left, int top, int right, int bottom);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetModuleHandleW(IntPtr moduleName);

        public static IntPtr GetModuleHandle()
        {
            IntPtr Module = GetModuleHandleW(IntPtr.Zero);

            if (Module == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return Module;
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern int GetCurrentThreadId();

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern int MessageBeep(uint beepType);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern int DeleteObject(HandleRef graphic);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr GetStockObject(int stockObjectType);

        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT position);

        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorInfo(out CURSORINFO info);

        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfoW(IntPtr monitor, out MONITORINFO info);

        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfoW(IntPtr monitor, [In, Out] MONITORINFOEX info);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr MonitorFromPoint(POINT point, int monitorType);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern short GetAsyncKeyState(int virtualKey);

        /// <summary>
        /// Determines whether a key is up or down at the time the function is called.
        /// </summary>
        /// <param name="virtualKey">
        /// Specifies a virtual-key to test.
        /// </param>
        /// <returns>
        /// <c>true</c> if the key is down; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsKeyDown(int virtualKey)
        {
            return ((GetAsyncKeyState(virtualKey) & 0x8000) != 0); // top bit is 1 if key is down
        }

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern int SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr SendMessageW(IntPtr window, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessageW(HandleRef window, int msg, IntPtr wParam, string lParam);

        public static void SetTextualCue(TextBox textBox, string cue)
        {
            SendMessageW(new HandleRef(textBox, textBox.Handle), EM_SETCUEBANNER, IntPtr.Zero, cue);
        }

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern int GetWindowThreadProcessId(IntPtr window, IntPtr processId);

        public static bool BringToTop(HandleRef window, bool setForeground)
        {
            IntPtr ForegroundWindow = GetForegroundWindow(); // The window can be null!

            if (ForegroundWindow != IntPtr.Zero)
            {
                if (ForegroundWindow == window.Handle)
                {
                    return true;
                }

                int ForegroundAppThreadId = GetWindowThreadProcessId(ForegroundWindow, IntPtr.Zero);
                int CurrentAppThreadId = GetCurrentThreadId();
                bool DetachThreadInput = false;

                if (ForegroundAppThreadId != CurrentAppThreadId)
                {
                    DetachThreadInput = AttachThreadInput(ForegroundAppThreadId, CurrentAppThreadId, true);
                }

                bool BroughtToTop;

                if (setForeground)
                {
                    BroughtToTop = SetForegroundWindow(window.Handle);
                }
                else
                {
                    BringWindowToTop(ForegroundWindow);
                    BroughtToTop = BringWindowToTop(window.Handle);
                }

                if (DetachThreadInput)
                {
                    AttachThreadInput(ForegroundAppThreadId, CurrentAppThreadId, false);
                }

                return BroughtToTop;
            }

            GC.KeepAlive(window.Wrapper);

            return false;
        }

        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AttachThreadInput(int idAttach, int idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool attach);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr window);

        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsZoomed(IntPtr window);

        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(HandleRef window, out RECT bounds);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern int InvalidateRect(IntPtr window, IntPtr rectangle, [MarshalAs(UnmanagedType.Bool)] bool erase);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern int RedrawWindow(IntPtr window, IntPtr rectangle, IntPtr region, uint mode);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr WindowFromPoint(POINT position);


        public const int WM_CLOSE = 0x0010;

        public const int WM_INITDIALOG = 0x0110;
        // public const int WM_NOTIFY = 0x004E;

        private const uint HH_DISPLAY_TEXT_POPUP = 14;

        private const int EM_SETCUEBANNER = 0x1501;

        public const int MAX_PATH = 260;


        private const int SWP_NOSIZE = 0x0001;
        // private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        // public const int SWP_NOREDRAW = 0x0008
        private const int SWP_NOACTIVATE = 0x0010;
        // public const int SWP_FRAMECHANGED = 0x0020;
        // public const int SWP_SHOWWINDOW = 0x0040;
        // public const int SWP_HIDEWINDOW = 0x0080;
        // public const int SWP_NOCOPYBITS = 0x0100;
        // public const int SWP_NOOWNERZORDER = 0x0200;
        // public const int SWP_NOSENDCHANGING = 0x0400;


        // public const uint OFN_READONLY = 0x00000001;
        public const uint OFN_OVERWRITEPROMPT = 0x00000002;
        public const uint OFN_HIDEREADONLY = 0x00000004;
        // public const uint OFN_NOCHANGEDIR = 0x00000008;
        // public const uint OFN_SHOWHELP = 0x00000010;
        public const uint OFN_ENABLEHOOK = 0x00000020;
        // public const uint OFN_ENABLETEMPLATE = 0x00000040;
        // public const uint OFN_ENABLETEMPLATEHANDLE = 0x00000080;
        // public const uint OFN_NOVALIDATE = 0x00000100;
        // public const uint OFN_ALLOWMULTISELECT = 0x00000200;
        // public const uint OFN_EXTENSIONDIFFERENT = 0x00000400;
        public const uint OFN_PATHMUSTEXIST = 0x00000800;
        public const uint OFN_FILEMUSTEXIST = 0x00001000;
        // public const uint OFN_CREATEPROMPT = 0x00002000;
        // public const uint OFN_SHAREAWARE = 0x00004000;
        public const uint OFN_NOREADONLYRETURN = 0x00008000;
        // public const uint OFN_NOTESTFILECREATE = 0x00010000;
        // public const uint OFN_NONETWORKBUTTON = 0x00020000;
        // public const uint OFN_NOLONGNAMES = 0x00040000;
        public const uint OFN_EXPLORER = 0x00080000;
        // public const uint OFN_NODEREFERENCELINKS = 0x00100000;
        // public const uint OFN_LONGNAMES = 0x00200000;
        // public const uint OFN_ENABLEINCLUDENOTIFY = 0x00400000;
        public const uint OFN_ENABLESIZING = 0x00800000;
        // public const uint OFN_DONTADDTORECENT = 0x02000000;
        // public const uint OFN_FORCESHOWHIDDEN = 0x10000000;


        public const int HOLLOW_BRUSH = 5;

        public const int PS_SOLID = 0;

        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE_LL = 14;

        public const int MONITOR_DEFAULTTONULL = 0x00000000;
        public const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        public const uint MB_DEFAULT = 0xFFFFFFFF;
        // public const uint MB_ICONHAND = 0x00000010;

        public const uint CURSOR_SHOWING = 0x00000001;

        private const uint CAPTUREBLT = 0x40000000;
        private const uint SRCCOPY = 0x00CC0020;

        public const uint RDW_INVALIDATE = 0x0001;
        public const uint RDW_ALLCHILDREN = 0x0080;
        public const uint RDW_UPDATENOW = 0x0100;
        public const uint RDW_FRAME = 0x0400;

        // public const int WM_KEYDOWN = 0x0100;
        // public const int WM_KEYUP = 0x0101;
        // public const int WM_SYSKEYDOWN = 0x0104;
        // public const int WM_SYSKEYUP = 0x0105;

        public const uint LLKHF_ALTDOWN = 0x2000 >> 8;
        public const uint LLKHF_UP = 0x8000 >> 8;

        public const int WM_MOUSEMOVE = 0x0200;
        // public const int WM_LBUTTONDOWN = 0x0201;
        // public const int WM_LBUTTONUP = 0x0202;
        // public const int WM_RBUTTONDOWN = 0x0204;
        // public const int WM_RBUTTONUP = 0x0205;
        // public const int WM_MOUSEWHEEL = 0x020A;


        public const int VK_SHIFT = 0x10;
        public const int VK_CONTROL = 0x11;
        // public const int VK_MENU = 0x12;
        public const int VK_SNAPSHOT = 0x2C;

        public const int WM_APP = 0x8000;

        private const int IDI_APPLICATION = 32512;

        public const int INVALID_HANDLE_VALUE = -1;

        public const uint PAGE_READWRITE = 0x04;
        public const uint SEC_COMMIT = 0x8000000;

        public const int ERROR_ALREADY_EXISTS = 183;

        public const uint FILE_MAP_WRITE = 0x0002;
        public const uint FILE_MAP_READ = 0x0004;

        // public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;

        // public const int WM_GETICON = 0x007F;
        public const int WM_SETICON = 0x0080;


        public const uint CDERR_INITIALIZATION = 0x0002;
        public const uint CDERR_MEMALLOCFAILURE = 0x0009;

        public const uint FNERR_SUBCLASSFAILURE = 0x3001;
        public const uint FNERR_INVALIDFILENAME = 0x3002;
        public const uint FNERR_BUFFERTOOSMALL = 0x3003;

        public const int HWND_TOPMOST = -1;

        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;


    }
}