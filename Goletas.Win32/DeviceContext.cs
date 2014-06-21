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
	/// Represents a device context.
	/// </summary>
    internal sealed class DeviceContext : CriticalFinalizerObject, IDisposable
	{
		/// <summary>
		/// The handle for this object.
		/// </summary>
		private IntPtr _Handle;

		/// <summary>
		/// Gets the handle for this <see cref="DeviceContext"/> object.
		/// This is not a copy of the handle; do not delete it.
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
		/// Releases all resources used by this <see cref="DeviceContext"/> object.
		/// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void Release()
		{
			if (this._Handle != IntPtr.Zero)
			{
				NativeMethods.DeleteDC(new HandleRef(this, this._Handle));

				this._Handle = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Releases all resources used by this <see cref="DeviceContext"/> object.
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
		~DeviceContext()
		{
			this.Release();
		}

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public DeviceContext()
        {
			if ((this._Handle = NativeMethods.CreateDCW("DISPLAY", null, IntPtr.Zero, IntPtr.Zero)) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceContext"/> class
        /// for a device using the specified device name. 
        /// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public DeviceContext(string deviceName)
        {
            if ((this._Handle = NativeMethods.CreateDCW(null, deviceName, IntPtr.Zero, IntPtr.Zero)) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

	}
}