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
	/// Defines an object used to draw lines and curves.
	/// </summary>
    internal sealed class Pen : CriticalFinalizerObject, IDisposable
	{
		/// <summary>
		/// The handle for this object.
		/// </summary>
		private IntPtr _Handle;

		/// <summary>
		/// Gets the handle for this <see cref="Pen"/> object.
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
		/// Releases all resources used by this <see cref="Pen"/> object.
		/// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private void Release()
		{
			if(this._Handle != IntPtr.Zero)
			{
                NativeMethods.DeleteObject(new HandleRef(this, this._Handle));

				this._Handle = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Releases all resources used by this <see cref="Pen"/> object.
		/// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Dispose()
		{
			this.Release();
            GC.SuppressFinalize(this);
		}

		/// <summary>
		/// This destructor will run only if the <see cref="Dispose()"/> method
		/// does not get called.
		/// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		~Pen()
		{
			this.Release();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Pen"/> from the specified
		/// style, width, and color of the pen.
		/// </summary>
        /// <param name="style">
		/// The pen style.
		/// </param>
        /// <param name="width">
		/// The width of the pen.
		/// </param>
        /// <param name="colorRef">
		/// The color of the pen.
		/// </param>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public Pen(int style, int width, uint colorRef)
        {
            if ((this._Handle = NativeMethods.CreatePen(style, width, colorRef)) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

	}
}