//licHeader
//===============================================================================================================
// System  : Nistec.Lib - Nistec.Lib Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of nistec library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;

namespace Nistec.IO
{
    [SuppressUnmanagedCodeSecurity]
    internal static class Win32Io
    {

        [StructLayout(LayoutKind.Sequential)]
        internal class SecurityAttributes// SECURITY_ATTRIBUTES
        {
            internal int nLength;
            internal unsafe byte* pSecurityDescriptor;
            internal int bInheritHandle;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, IntPtr numBytesRead_mustBeZero, NativeOverlapped* overlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, out int numBytesRead, IntPtr mustBeZero);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, IntPtr numBytesWritten_mustBeZero, NativeOverlapped* lpOverlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, IntPtr mustBeZero);
 

        [SecurityCritical]
        internal static SafeFileHandle SafeCreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SecurityAttributes securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile)
        {
            SafeFileHandle handle = CreateFile(lpFileName, dwDesiredAccess, dwShareMode, securityAttrs, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
            if (!handle.IsInvalid && (GetFileType(handle) != 1))
            {
                handle.Dispose();
                throw new NotSupportedException("Not Supported, IO Non File Devices");
            }
            return handle;
        }
        [SecurityCritical]
        internal static unsafe long SetFilePointer(SafeFileHandle handle, long offset, SeekOrigin origin, out int hr)
        {
            hr = 0;
            int lo = (int)offset;
            int hi = (int)(offset >> 0x20);
            lo = SetFilePointerWin32(handle, lo, &hi, (int)origin);
            if ((lo == -1) && ((hr = Marshal.GetLastWin32Error()) != 0))
            {
                return -1L;
            }
            return (long)((((ulong)hi) << 0x20) | ((uint)lo));
        }
        [DllImport("kernel32.dll", EntryPoint = "SetFilePointer", SetLastError = true)]
        private static extern unsafe int SetFilePointerWin32(SafeFileHandle handle, int lo, int* hi, int origin);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SecurityAttributes securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
        
        [DllImport("kernel32.dll")]
        internal static extern int GetFileType(SafeFileHandle handle);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, NativeOverlapped* lpOverlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int WriteFile(SafePipeHandle handle, byte* bytes, int numBytesToWrite, IntPtr numBytesWritten_mustBeZero, NativeOverlapped* lpOverlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int WriteFile(SafePipeHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, IntPtr mustBeZero);
        [DllImport("kernel32.dll")]
        internal static extern int SetErrorMode(int newMode);

        internal static readonly IntPtr NULL = IntPtr.Zero;

        [SecurityCritical]
        internal static string GetMessage(int errorCode)
        {
            StringBuilder lpBuffer = new StringBuilder(0x200);
            if (FormatMessage(0x3200, NULL, errorCode, 0, lpBuffer, lpBuffer.Capacity, NULL) != 0)
            {
                return lpBuffer.ToString();
            }
            return ("UnknownError_Num " + errorCode);
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr va_list_arguments);

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern uint AlignedSizeOfType(Type type);
       
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static uint AlignedSizeOf<T>() where T : struct
        {
            uint num = Win32Io.SizeOf<T>();
            switch (num)
            {
                case 1:
                case 2:
                    return num;
            }
            if ((IntPtr.Size == 8) && (num == 4))
            {
                return num;
            }
            return AlignedSizeOfType(typeof(T));
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static uint SizeOf<T>() where T : struct
        {
            return Win32Io.SizeOfType(typeof(T));
        }
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern int SizeOfHelper(Type t, bool throwIfNotMarshalable);
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern uint SizeOfType(Type type);

        #region files dll imports
        //
        // DllImport statements identify specific functions and declare their C# function signature
        // 
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(
            IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileInformationByHandle(
            IntPtr hFile,
            out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFile(
            string fileName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadFile(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(
            IntPtr hFile,
            IntPtr bytes,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            int overlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            int overlapped);

        [DllImport("kernel32.dll")]
        public static extern void ZeroMemory(IntPtr ptr, int size);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadFile(SafeFileHandle handle, IntPtr buffer, uint numBytesToRead, out uint numBytesRead, IntPtr overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod);

        //[DllImport("kernel32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool ReadFile(IntPtr handle, IntPtr buffer, uint numBytesToRead, out uint numBytesRead, IntPtr overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetFilePointerEx(IntPtr hFile, long liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod);

        #endregion

        #region Lock/Unlock

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool LockFile(SafeFileHandle handle, int offsetLow, int offsetHigh, int countLow, int countHigh);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool UnlockFile(SafeFileHandle handle, int offsetLow, int offsetHigh, int countLow, int countHigh);

        /// Return Type: BOOL->int 
        ///hFile: HANDLE->void* 
        ///dwFlags: DWORD->unsigned int 
        ///dwReserved: DWORD->unsigned int 
        ///nNumberOfBytesToLockLow: DWORD->unsigned int 
        ///nNumberOfBytesToLockHigh: DWORD->unsigned int 
        ///lpOverlapped: LPOVERLAPPED->_OVERLAPPED* 
        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "LockFileEx")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool LockFileEx([System.Runtime.InteropServices.InAttribute()] System.IntPtr hFile, uint dwFlags, uint dwReserved, uint nNumberOfBytesToLockLow, uint nNumberOfBytesToLockHigh, ref OVERLAPPED lpOverlapped);


        private const uint LOCKFILE_EXCLUSIVE_LOCK = 0x00000002;

        public static unsafe void LockFile(SafeFileHandle handle, bool exclusive, long offset, long length, Action action)
        {
            if (handle == null)
                throw new ArgumentNullException("handle");
            if (handle.IsInvalid)
                throw new ArgumentException("An invalid file handle was specified.", "handle");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("The offset cannot be negative.", "offset");
            if (length < 0)
                throw new ArgumentOutOfRangeException("The length cannot be negative.", "length");

            Overlapped overlapped = new Overlapped();
            overlapped.OffsetHigh = (int)(offset >> 32);
            overlapped.OffsetLow = (int)offset;
            IOCompletionCallback callback =
                (errorCode, numBytes, nativeOverlapped) =>
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        Overlapped.Free(nativeOverlapped);
                    }
                };
            NativeOverlapped* native = overlapped.Pack(callback, null);
            uint flags = exclusive ? LOCKFILE_EXCLUSIVE_LOCK : 0;
            if (!LockFileEx(handle, flags, 0, (int)length, (int)(length >> 32), native))
            {
                Overlapped.Free(native);
                IoErrors.FileNotOpen();
            }
        }

        public static unsafe void UnlockFile(SafeFileHandle handle, bool exclusive, long offset, long length, Action action)
        {
            if (handle == null)
                throw new ArgumentNullException("handle");
            if (handle.IsInvalid)
                throw new ArgumentException("An invalid file handle was specified.", "handle");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("The offset cannot be negative.", "offset");
            if (length < 0)
                throw new ArgumentOutOfRangeException("The length cannot be negative.", "length");

            Overlapped overlapped = new Overlapped();
            overlapped.OffsetHigh = (int)(offset >> 32);
            overlapped.OffsetLow = (int)offset;
            IOCompletionCallback callback =
                (errorCode, numBytes, nativeOverlapped) =>
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        Overlapped.Free(nativeOverlapped);
                    }
                };
            NativeOverlapped* native = overlapped.Pack(callback, null);
            uint flags = exclusive ? LOCKFILE_EXCLUSIVE_LOCK : 0;
            if (!UnlockFileEx(handle, flags, 0, (int)length, (int)(length >> 32), native))
            {
                Overlapped.Free(native);
                IoErrors.FileNotOpen();
            }
        }

        //FileHandle-                   Handle to the file to lock
        //fileOffsetLow-                Offset to start of locked section (low-order 32 bits)
        //fileOffsetHigh-               Offset to start of locked section (high-order 32 bits)
        //unlockLow-                    Number of bytes to unlock (low-order 32 bits)
        //unlockHigh-                   Number of bytes to unlock (high-order 32 bits)
        //Returns TRUE on success.

        [DllImport("kernel32.dll", SetLastError = true)]
        private static unsafe extern bool LockFileEx(SafeFileHandle handle, uint flags, uint mustBeZero, int countLow, int countHigh, NativeOverlapped* overlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static unsafe extern bool UnlockFileEx(SafeFileHandle handle, uint flags, uint mustBeZero, int countLow, int countHigh, NativeOverlapped* overlapped);

        #endregion

        #region constants
        //
        // these are constants used by the Win32 api functions.  They can be found in the documentation and header files.
        //
        public const UInt32 GENERIC_READ = 0x80000000;
        public const UInt32 GENERIC_WRITE = 0x40000000;
        public const UInt32 FILE_SHARE_READ = 0x00000001;
        public const UInt32 FILE_SHARE_WRITE = 0x00000002;
        public const UInt32 FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        public const UInt32 CREATE_NEW = 1;
        public const UInt32 CREATE_ALWAYS = 2;
        public const UInt32 OPEN_EXISTING = 3;
        public const UInt32 OPEN_ALWAYS = 4;
        public const UInt32 TRUNCATE_EXISTING = 5;
        #endregion

        #region structures
        //
        // This section declares the structures used by the Win32 functions so that the information can be accessed by C# code
        //
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public FILETIME CreationTime;
            public FILETIME LastAccessTime;
            public FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FILETIME
        {
            public uint DateTimeLow;
            public uint DateTimeHigh;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OVERLAPPED
        {
            public uint internalLow;
            public uint internalHigh;
            public uint offsetLow;
            public uint offsetHigh;
            public IntPtr hEvent;
        }
        #endregion


    }

}
