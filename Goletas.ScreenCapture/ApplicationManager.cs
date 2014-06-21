//
// Copyright © 2006 - 2008 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture
{
    using System;
    using System.IO;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Resources;
    using System.Reflection;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using Goletas.ScreenCapture.Drawing.Imaging;
    using Goletas.Win32;

    /// <summary>
    /// Manages instances of the application.
    /// </summary>
    public sealed class ApplicationManager : ApplicationContext, IMessageFilter
    {
        private static readonly ResourceManager _ResourceManager = new ResourceManager("Goletas.ScreenCapture", Assembly.GetExecutingAssembly());

        internal static int GetInt32(string name)
        {
            return int.Parse(_ResourceManager.GetString(name, CultureInfo.CurrentUICulture), CultureInfo.CurrentUICulture);
        }

        internal static string GetString(string name)
        {
            return _ResourceManager.GetString(name, CultureInfo.CurrentUICulture);
        }

        public static string ProductName
        {
            get
            {
                return _ResourceManager.GetString("ProductName", CultureInfo.CurrentUICulture);
            }
        }

        public static string ProductFullName
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, _ResourceManager.GetString("ProductFullName", CultureInfo.CurrentUICulture), Application.ProductVersion);
            }
        }

        public static string LegalInfo
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, _ResourceManager.GetString("LegalInfo", CultureInfo.CurrentUICulture), 2008);
            }
        }

        private const int APP_EXIT_CODE_FAILED = 1;
        private const int WM_APP_SHOW = NativeMethods.WM_APP;
        internal const int WM_APP_BRING_TO_TOP = NativeMethods.WM_APP + 1;

        /// <summary>
        /// The settings form and the application are being disposed.
        /// </summary>
        private const int FORM_STATE_DISPOSED = -1;

        /// <summary>
        /// The settings form is closed.
        /// </summary>
        private const int FORM_STATE_CLOSED = 0;

        /// <summary>
        /// The settings form is being created.
        /// </summary>
        private const int FORM_STATE_STARTING = 1;

        /// <summary>
        /// The settings form has been created and shown to the user.
        /// </summary>
        private const int FORM_STATE_READY = 2;


        private int _SettingsFormState;
        private IntPtr _SettingsForm;

        private int _Disposed;

        private Hook _KeyboardHook;
        private Hook _MouseHook;

        private FileMapping _InstanceId;

        /// <summary>
        /// The handle to the marked window.
        /// </summary>
        private IntPtr _SelectedWindow;

        /// <summary>
        /// True if the PrintScreen key is down.
        /// </summary>
        private bool _IsPrintScreenKeyDown;

        /// <summary>
        /// True if searching for a window; otherwise false.
        /// </summary>
        private bool _IsSearchMode;


        /// <summary>
        /// Marks the window with the specified window handle.
        /// </summary>
        /// <param name="window">
        /// The handle to the window which will be marked.
        /// </param>
        private static void DrawRectangle(IntPtr window)
        {
            RECT WindowBounds;

            if (NativeMethods.GetWindowRect(new HandleRef(null, window), out WindowBounds))
            {
                using (Pen Marker = new Pen(NativeMethods.PS_SOLID, 5, 0x000000FF))
                {
                    using (SharedDeviceContext WindowDC = new SharedDeviceContext(window))
                    {
                        HandleRef DC = new HandleRef(WindowDC, WindowDC.Handle);

                        IntPtr OriginalBrush = NativeMethods.SelectObject(DC, new HandleRef(null, NativeMethods.GetStockObject(NativeMethods.HOLLOW_BRUSH)));
                        IntPtr OriginalPen = NativeMethods.SelectObject(DC, new HandleRef(Marker, Marker.Handle));

                        if (NativeMethods.IsZoomed(window))
                        {
                            NativeMethods.Rectangle(DC, 5, 5, WindowBounds.Right - WindowBounds.Left - 5, WindowBounds.Bottom - WindowBounds.Top - 5);
                        }
                        else
                        {
                            NativeMethods.Rectangle(DC, 1, 1, WindowBounds.Right - WindowBounds.Left - 1, WindowBounds.Bottom - WindowBounds.Top - 1);
                        }

                        NativeMethods.SelectObject(DC, new HandleRef(Marker, OriginalPen));
                        NativeMethods.SelectObject(DC, new HandleRef(null, OriginalBrush));
                    }
                }
            }
        }

        // this operation is very slow...
        private static void RedrawWindow(IntPtr window)
        {
            NativeMethods.InvalidateRect(window, IntPtr.Zero, true);
            // Window.Update(WindowHandle);
            NativeMethods.RedrawWindow(window, IntPtr.Zero, IntPtr.Zero, NativeMethods.RDW_ALLCHILDREN | NativeMethods.RDW_FRAME | NativeMethods.RDW_INVALIDATE | NativeMethods.RDW_UPDATENOW);
        }

        private IntPtr OnKeyboardEvent(int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if ((code < 0) || (lParam.vkCode != NativeMethods.VK_SNAPSHOT))
            {
                return NativeMethods.CallNextHookEx(new HandleRef(this._KeyboardHook, this._KeyboardHook.Handle), code, wParam, ref lParam);
            }

            if (!(this._IsPrintScreenKeyDown = ((lParam.flags & NativeMethods.LLKHF_UP) == 0)))
            {
                if (this._IsSearchMode)
                {
                    this._IsSearchMode = false;
                    IntPtr Window = this._SelectedWindow;

                    if (Window != IntPtr.Zero)
                    {
                        this._SelectedWindow = IntPtr.Zero;

                        RedrawWindow(Window);
                        JobManager.AddJob(Window);
                    }
                }
                else if ((lParam.flags & NativeMethods.LLKHF_ALTDOWN) != 0)
                {
                    this.ShowSettingsForm();
                }
                else if (NativeMethods.IsKeyDown(NativeMethods.VK_SHIFT))
                {
                    JobManager.AddJob(ImageArea.ForegroundWindow);
                }
                else if (NativeMethods.IsKeyDown(NativeMethods.VK_CONTROL))
                {
                    JobManager.AddJob(ImageArea.VirtualScreen);
                }
                else
                {
                    JobManager.AddJob(ImageArea.CurrentScreen);
                }
            }

            return new IntPtr(1);
        }

#warning Keeps searching if user Win+L while searching

        private IntPtr OnMouseEvent(int code, IntPtr wParam, ref MSLLHOOKSTRUCT lParam)
        {
            if ((code >= 0) && (wParam == new IntPtr(NativeMethods.WM_MOUSEMOVE)) && this._IsPrintScreenKeyDown)
            {
                this._IsSearchMode = true;

                IntPtr Window = NativeMethods.WindowFromPoint(lParam.pt);

                if (Window != IntPtr.Zero)
                {
                    IntPtr SelectedWindow = this._SelectedWindow;
                    this._SelectedWindow = Window;

                    if (Window != SelectedWindow)
                    {
                        RedrawWindow(SelectedWindow);
                    }

                    DrawRectangle(Window);
                }
            }

            return NativeMethods.CallNextHookEx(new HandleRef(this._MouseHook, this._MouseHook.Handle), code, wParam, ref lParam);
        }



        #region experimental
/*
#warning experimental

        const int R2_NOTXORPEN = 10;

        [DllImport("gdi32.dll", ExactSpelling = true)]
        private static extern int SetROP2(HandleRef context, int drawMode);


        private SharedDeviceContext _Context = null;
        private Pen _Marker = null;
        private IntPtr _OriginalBrush;
        private IntPtr _OriginalPen;
        private int _OldMode;


        private void PrepareContext()
        {
            this._Marker = new Pen(NativeMethods.PS_SOLID, 4, 0x000000FF);
            this._Context = new SharedDeviceContext(IntPtr.Zero);

            HandleRef DC = new HandleRef(this._Context, this._Context.Handle);

            this._OriginalBrush = NativeMethods.SelectObject(DC, new HandleRef(null, NativeMethods.GetStockObject(NativeMethods.HOLLOW_BRUSH)));
            this._OriginalPen = NativeMethods.SelectObject(DC, new HandleRef(this._Marker, this._Marker.Handle));

            this._OldMode = SetROP2(DC, R2_NOTXORPEN);
        }

        private void ReleaseContext()
        {
            HandleRef DC = new HandleRef(this._Context, this._Context.Handle);

            SetROP2(DC, this._OldMode);

            NativeMethods.SelectObject(DC, new HandleRef(this, this._OriginalPen));
            NativeMethods.SelectObject(DC, new HandleRef(this, this._OriginalBrush));

            this._Context.Dispose();
            this._Marker.Dispose();
        }


        private IntPtr OnKeyboardEvent(int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if ((code < 0) || (lParam.vkCode != NativeMethods.VK_SNAPSHOT))
            {
                return NativeMethods.CallNextHookEx(new HandleRef(this._KeyboardHook, this._KeyboardHook.Handle), code, wParam, ref lParam);
            }

            if (!(this._IsPrintScreenKeyDown = ((lParam.flags & NativeMethods.LLKHF_UP) == 0)))
            {
                if (this._IsSearchMode)
                {
                    this._IsSearchMode = false;
                    IntPtr Window = this._SelectedWindow;

                    if (Window != IntPtr.Zero)
                    {
                        this._SelectedWindow = IntPtr.Zero;

                        RECT windowBounds;

                        if (NativeMethods.GetWindowRect(new HandleRef(null, Window), out windowBounds))
                        {
                            NativeMethods.Rectangle(new HandleRef(this._Context, this._Context.Handle), windowBounds.Left, windowBounds.Top, windowBounds.Right, windowBounds.Bottom);

                            this.ReleaseContext();
                        }


                        JobManager.AddJob(Window);
                    }
                }
                else if ((lParam.flags & NativeMethods.LLKHF_ALTDOWN) != 0)
                {
                    this.ShowSettingsForm();
                }
                else if (NativeMethods.IsKeyDown(NativeMethods.VK_SHIFT))
                {
                    JobManager.AddJob(ImageBounds.ForegroundWindow);
                }
                else if (NativeMethods.IsKeyDown(NativeMethods.VK_CONTROL))
                {
                    JobManager.AddJob(ImageBounds.VirtualScreen);
                }
                else
                {
                    JobManager.AddJob(ImageBounds.CurrentScreen);
                }
            }

            return new IntPtr(1);
        }

#warning Keeps searching if user Win+L while searching

        private IntPtr OnMouseEvent(int code, IntPtr wParam, ref MSLLHOOKSTRUCT lParam)
        {
            if ((code >= 0) && (wParam == new IntPtr(NativeMethods.WM_MOUSEMOVE)) && this._IsPrintScreenKeyDown)
            {
                if (!this._IsSearchMode)
                {
                    this.PrepareContext();
                }

                this._IsSearchMode = true;

                IntPtr Window = NativeMethods.WindowFromPoint(lParam.pt);

                if (Window != IntPtr.Zero)
                {
                    IntPtr SelectedWindow = this._SelectedWindow;
                    this._SelectedWindow = Window;

                    if (Window != SelectedWindow)
                    {
                        RECT windowBounds;

                        NativeMethods.GetWindowRect(new HandleRef(null, SelectedWindow), out windowBounds);
                        NativeMethods.Rectangle(new HandleRef(this._Context, this._Context.Handle), windowBounds.Left, windowBounds.Top, windowBounds.Right, windowBounds.Bottom);

                        NativeMethods.GetWindowRect(new HandleRef(null, Window), out windowBounds);
                        NativeMethods.Rectangle(new HandleRef(this._Context, this._Context.Handle), windowBounds.Left, windowBounds.Top, windowBounds.Right, windowBounds.Bottom);

                    }
                }
            }

            return NativeMethods.CallNextHookEx(new HandleRef(this._MouseHook, this._MouseHook.Handle), code, wParam, ref lParam);
        }
        */
        #endregion


        private void CreateSettingsForm(object e)
        {
            SettingsForm form = new SettingsForm();

            form.HandleCreated += new EventHandler(this.SettingsFormHandleCreated);
            form.HandleDestroyed += new EventHandler(this.SettingsFormHandleDestroyed);

            Application.Run(form);
        }

        internal static Thread StartStaThread(ParameterizedThreadStart method, object e)
        {
            Thread thread = new Thread(method);
#if DEBUG
                thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
#endif
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(e);

            return thread;
        }

        private void ShowSettingsForm()
        {
            int State = Interlocked.CompareExchange(ref this._SettingsFormState, FORM_STATE_STARTING, FORM_STATE_CLOSED);

            if (State == FORM_STATE_CLOSED)
            {
                StartStaThread(this.CreateSettingsForm, null);
            }
            else if (State == FORM_STATE_READY)
            {
                NativeMethods.PostMessage(new HandleRef(this, this._SettingsForm), WM_APP_BRING_TO_TOP);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_APP_SHOW)
            {
                this.ShowSettingsForm();

                return true;
            }

            return false;
        }

        private void SettingsFormHandleCreated(object sender, EventArgs e)
        {
            IntPtr Window = ((SettingsForm)sender).Handle;

            if (Interlocked.CompareExchange(ref this._SettingsFormState, FORM_STATE_READY, FORM_STATE_STARTING) == FORM_STATE_DISPOSED)
            {
                NativeMethods.PostMessage(new HandleRef(sender, Window), NativeMethods.WM_CLOSE);
            }
            else
            {
                this._SettingsForm = Window;
            }
        }

        private void SettingsFormHandleDestroyed(object sender, EventArgs e) // how about handle recreation?
        {
            if (Interlocked.CompareExchange(ref this._SettingsFormState, FORM_STATE_CLOSED, FORM_STATE_READY) != FORM_STATE_DISPOSED)
            {
                this._SettingsForm = IntPtr.Zero;

                if (((SettingsForm)sender).DialogResult != DialogResult.OK)
                {
                    this.Dispose();
                    Application.Exit();
                }
            }
        }

        private static bool IsBackgroundMode(string[] e)
        {
            if (e != null)
            {
                for (int i = 0; i < e.Length; i++)
                {
                    if ("background".Equals(e[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private ApplicationManager(string[] e)
        {

#warning move to StartApplication?
#if !DEBUG
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException, false);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.OnUnhandledException);
#endif
            
            this._InstanceId = new FileMapping(@"Local\Goletas.ScreenCapture");

            if (this._InstanceId.IsNew)
            {
                Application.SetCompatibleTextRenderingDefault(false);

                if ((Configuration.Current.Settings == null) || (Configuration.Current.FileTypes == null))
                {
                    throw new InvalidOperationException();
                }

                if (!IsBackgroundMode(e))
                {
                    this.ShowSettingsForm();
                }

                Application.AddMessageFilter(this);
                this._InstanceId.Write(NativeMethods.GetCurrentThreadId());

                this._KeyboardHook = new Hook(new LowLevelKeyboardProc(this.OnKeyboardEvent));
                this._MouseHook = new Hook(new LowLevelMouseProc(this.OnMouseEvent));
            }
        }

        protected override void ExitThreadCore()
        {
            this.Dispose();
            base.ExitThreadCore();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (Interlocked.Exchange(ref this._Disposed, 1) == 0))
            {
                if (this._MouseHook != null)
                {
                    this._MouseHook.Dispose();
                    this._MouseHook = null;
                }

                if (this._KeyboardHook != null)
                {
                    this._KeyboardHook.Dispose();
                    this._KeyboardHook = null;
                }

                if (this._InstanceId != null)
                {
                    bool IsFirstInstance = this._InstanceId.IsNew;

                    this._InstanceId.Dispose();
                    this._InstanceId = null;

                    if (IsFirstInstance)
                    {
                        Application.RemoveMessageFilter(this);

                        if (this._SelectedWindow != IntPtr.Zero)
                        {
                            RedrawWindow(this._SelectedWindow);
                            this._SelectedWindow = IntPtr.Zero;
                        }
                    }
                }

                if ((Interlocked.Exchange(ref this._SettingsFormState, FORM_STATE_DISPOSED) == FORM_STATE_READY) && (this._SettingsForm != IntPtr.Zero))
                {
                    NativeMethods.PostMessageW(new HandleRef(this, this._SettingsForm), NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    this._SettingsForm = IntPtr.Zero;
                }
            }

            base.Dispose(disposing);
        }

        private static void ShowErrorMessageBox(object message)
        {
            try
            {
                Goletas.ScreenCapture.MessageBox.Show(null, (string)message, MessageBoxIcon.Error);
            }
            catch
            {

            }

            Environment.Exit(APP_EXIT_CODE_FAILED);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                this.Dispose();
            }
            catch
            {

            }

            string message = null;

            try
            {
                string LogPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + "Goletas.ScreenCapture.log";

                using (StreamWriter Writer = new StreamWriter(LogPath, true))
                {
                    Writer.WriteLine(DateTime.Now.ToUniversalTime().ToString(CultureInfo.CurrentCulture));
                    Writer.WriteLine(ApplicationManager.ProductFullName);
                    Writer.WriteLine(Environment.OSVersion.VersionString);
                    Writer.WriteLine(Environment.Version.ToString());

                    Writer.WriteLine(((Exception)e.ExceptionObject).ToString());
                }

                message = string.Format(CultureInfo.CurrentCulture, ApplicationManager.GetString("UnhandledExceptionEx"), LogPath);
            }
            catch
            {
                try
                {
                    message = ApplicationManager.GetString("UnhandledException");
                }
                catch
                {

                }
            }

            if (message != null)
            {
                try
                {
                    StartStaThread(ShowErrorMessageBox, message).Join();
                }
                catch
                {

                }
            }

            Environment.Exit(APP_EXIT_CODE_FAILED);
        }

        private static bool IsFullTrust()
        {
            try
            {
                new PermissionSet(PermissionState.Unrestricted).Demand();
            }
            catch (SecurityException)
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void StartApplication(string[] e)
        {
            ApplicationManager manager = new ApplicationManager(e);

            if (manager._InstanceId.IsNew)
            {
                Application.Run(manager);
            }
            else
            {
                NativeMethods.PostThreadMessage(manager._InstanceId.Read(), WM_APP_SHOW);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] e)
        {
#if DEBUG
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-CN");
#endif
            Application.EnableVisualStyles();

            if (IsFullTrust())
            {
                if ((Environment.OSVersion.Platform != PlatformID.Win32NT) || (Environment.OSVersion.Version <= (new Version(5, 1, 2600))))
                {
                    Goletas.ScreenCapture.MessageBox.Show(null, ApplicationManager.GetString("OsNotSupported"), MessageBoxIcon.Exclamation);
                }
                else
                {
                    StartApplication(e);
                }
            }
            else
            {
                Goletas.ScreenCapture.MessageBox.Show(null, ApplicationManager.GetString("NoFullTrust"), MessageBoxIcon.Exclamation);
            }
        }

    }
}