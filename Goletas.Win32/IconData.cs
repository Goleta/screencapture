//
// Copyright © 2005 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System;
    using System.Security;
    using System.Runtime.ConstrainedExecution;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

	/// <summary>
	/// Contains information about an icon or a cursor.
	/// </summary>
    internal sealed class IconData : CriticalFinalizerObject, IDisposable
	{
		/// <summary>
		/// Holds the icon information.
		/// </summary>
        private ICONINFO _Data;

        /// <summary>
        /// Tracks whether <see cref="Dispose"/> method has been called for
        /// this <see cref="IconInfo"/> object.
        /// </summary>
        private bool _Disposed;

		/// <summary>
		/// Gets the X-coordinate of hotspot of the icon represented by this <see cref="IconInfo"/>.
		/// </summary>
		/// <value>
		/// The hotspot of the icon.
		/// </value>
        public int HotSpotX
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._Data.xHotspot;
            }
        }

        /// <summary>
        /// Gets the Y-coordinate of hotspot of the icon represented by this <see cref="IconInfo"/>.
        /// </summary>
        /// <value>
        /// The hotspot of the icon.
        /// </value>
        public int HotSpotY
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._Data.yHotspot;
            }
        }

        /// <summary>
        /// Releases all resources used by this <see cref="IconInfo"/> object.
		/// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void Release()
        {
            if (!this._Disposed)
            {
                if (this._Data.hbmMask != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(new HandleRef(this, this._Data.hbmMask));
                }

                if (this._Data.hbmColor != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(new HandleRef(this, this._Data.hbmColor));
                }

                this._Disposed = true;
            }
        }

		/// <summary>
        /// Releases all resources used by this <see cref="IconInfo"/> object.
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
        ~IconData()
		{
			this.Release();
		}

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public IconData(IntPtr icon)
        {
            if (NativeMethods.GetIconInfo(new HandleRef(this, icon), out this._Data) == 0)
            {
                throw new Win32Exception();
            }
        }

	}
}