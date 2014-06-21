//
// Copyright © 2007 Maksim Goleta. All rights reserved.
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

    internal sealed class FileView : CriticalFinalizerObject, IDisposable
	{
        private IntPtr _Handle;

        public IntPtr Handle
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._Handle;
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void Release()
        {
            IntPtr handle = Interlocked.Exchange(ref this._Handle, IntPtr.Zero);

            if (handle != IntPtr.Zero)
            {
                NativeMethods.UnmapViewOfFile(new HandleRef(this, handle));
            }
        }

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
        ~FileView()
        {
            this.Release();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public FileView(FileMapping fileMapping, uint desiredAccess, int numberOfBytesToMap)
        {
            this._Handle = NativeMethods.MapViewOfFile(new HandleRef(fileMapping, fileMapping.Handle), desiredAccess, 0, 0, new IntPtr(numberOfBytesToMap));

            if (this._Handle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

	}
}