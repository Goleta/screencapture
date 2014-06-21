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

    internal sealed class FileMapping : CriticalFinalizerObject, IDisposable
    {
        private IntPtr _Handle;
        private bool _IsNew;

        public IntPtr Handle
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._Handle;
            }
        }

        public bool IsNew
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._IsNew;
            }
        }

        public void Write(int value)
        {
            using (FileView view = new FileView(this, NativeMethods.FILE_MAP_WRITE, sizeof(int)))
            {
                Marshal.WriteInt32(view.Handle, 0, value);
                GC.KeepAlive(view);
            }
        }

        public int Read()
        {
            using (FileView view = new FileView(this, NativeMethods.FILE_MAP_READ, sizeof(int)))
            {
                int value = Marshal.ReadInt32(view.Handle, 0);
                GC.KeepAlive(view);

                return value;
            }
        }

        /// <summary>
        /// Releases all resources used by this <see cref="FileMapping"/> object.
		/// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void Release()
        {
            IntPtr handle = Interlocked.Exchange(ref this._Handle, IntPtr.Zero);

            if (handle != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(new HandleRef(this, handle));
            }
        }

		/// <summary>
        /// Releases all resources used by this <see cref="FileMapping"/> object.
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
        ~FileMapping()
		{
			this.Release();
		}

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public FileMapping(string fileName)
        {
            this._Handle = NativeMethods.CreateFileMappingW(new IntPtr(NativeMethods.INVALID_HANDLE_VALUE), IntPtr.Zero, NativeMethods.PAGE_READWRITE | NativeMethods.SEC_COMMIT, 0, sizeof(int), fileName);

            int error = Marshal.GetLastWin32Error();

            if (this._Handle == IntPtr.Zero)
            {
                throw new Win32Exception(error);
            }

            this._IsNew = (error != NativeMethods.ERROR_ALREADY_EXISTS);
        }

    }
}