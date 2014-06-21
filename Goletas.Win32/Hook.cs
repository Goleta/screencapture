//
// Copyright © 2005 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System;
    using System.Security;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using System.Threading;

    internal sealed class Hook : CriticalFinalizerObject, IDisposable
	{
        /// <summary>
        /// The handle for the system hook.
        /// </summary>
        private IntPtr _Handle;

        private GCHandle _Callback;

        private int _Disposed;

        /// <summary>
        /// Gets the handle for the system hook.
        /// </summary>
        /// <remarks>
        /// This is not a copy of the handle; do not uninstall it.
        /// </remarks>
        public IntPtr Handle
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._Handle;
            }
        }

        /// <summary>
        /// Uninstalls the hook and releases all resources used by the system hook.
        /// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void Release()
        {
            if (Interlocked.Exchange(ref this._Disposed, 1) == 0)
            {
                if (this._Handle != IntPtr.Zero)
                {
                    NativeMethods.UnhookWindowsHookEx(new HandleRef(this, this._Handle));
                    this._Handle = IntPtr.Zero;
                }

                if (this._Callback.IsAllocated)
                {
                    this._Callback.Free();
                }
            }
        }

        /// <summary>
        /// Uninstalls the hook and releases all resources used by the system hook.
        /// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Dispose()
        {
            this.Release();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This destructor will run only if the <see cref="Dispose()"/>
        /// method does not get called.
        /// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        ~Hook()
        {
            this.Release();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public Hook(LowLevelKeyboardProc callback)
        {
            this._Callback = GCHandle.Alloc(callback);

            if ((this._Handle = NativeMethods.SetWindowsHookExW(NativeMethods.WH_KEYBOARD_LL, Marshal.GetFunctionPointerForDelegate(callback), NativeMethods.GetModuleHandle(), 0)) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public Hook(LowLevelMouseProc callback)
        {
            this._Callback = GCHandle.Alloc(callback);

            if ((this._Handle = NativeMethods.SetWindowsHookExW(NativeMethods.WH_MOUSE_LL, Marshal.GetFunctionPointerForDelegate(callback), NativeMethods.GetModuleHandle(), 0)) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

	}
}