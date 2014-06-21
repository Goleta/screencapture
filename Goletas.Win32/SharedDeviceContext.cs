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

	/// <summary>
	/// Represents a shared device context.
	/// </summary>
    internal sealed class SharedDeviceContext : CriticalFinalizerObject, IDisposable
	{
		/// <summary>
		/// The handle for this object.
		/// </summary>
		private IntPtr _Handle;

        private IntPtr _Window;

		/// <summary>
		/// Gets the handle for this <see cref="SharedDeviceContext"/> object.
		/// This is not a copy of the handle; do not release it, call
		/// <see cref="Dispose()"/> method instead.
		/// </summary>
        public IntPtr Handle
		{
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return this._Handle;
			}
		}

		/// <summary>
		/// Releases all resources used by this <see cref="SharedDeviceContext"/> object.
		/// </summary>
		/// <param name="Disposing">
		/// <c>true</c> to dispose managed and unmanaged resources. <c>false</c> to dispose
		/// only unmanaged resources.
		/// </param>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void Release()
        {
            if (this._Handle != IntPtr.Zero)
            {
                NativeMethods.ReleaseDC(new HandleRef(this, this._Window), new HandleRef(this, this._Handle));

                this._Handle = IntPtr.Zero;
            }
        }

		/// <summary>
		/// Releases all resources used by this <see cref="SharedDeviceContext"/> object.
		/// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Dispose()
		{
			this.Release();
            GC.SuppressFinalize(this);
		}

		/// <summary>
		/// This destructor will run only if the <see cref="Dispose()"/> method does
		/// not get called.
		/// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		~SharedDeviceContext()
		{
			this.Release();
		}

     //   const uint DCX_LOCKWINDOWUPDATE = 0x00000400;
     //   const uint DCX_CACHE = 0x00000002;

     //   [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("user32.dll", ExactSpelling = true)]
     //   public static extern IntPtr GetDCEx(HandleRef window, IntPtr region, uint flags);

		/// <summary>
		/// Initializes a new instance of the <see cref="SharedDeviceContext"/> from the
		/// specified window.
		/// </summary>
		/// <param name="Window">
		/// The window from which to initialize the <see cref="SharedDeviceContext"/>.
		/// </param>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public SharedDeviceContext(IntPtr window)
		{
            if ((this._Handle = NativeMethods.GetWindowDC(new HandleRef(this, window))) == IntPtr.Zero)
           // if ((this._Handle = GetDCEx(new HandleRef(this, window), IntPtr.Zero, DCX_LOCKWINDOWUPDATE | DCX_CACHE)) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            this._Window = window;
		}

	}
}