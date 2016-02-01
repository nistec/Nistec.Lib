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
using System.IO;
using System.Security;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Nistec.IO
{
   
    internal static class IoErrors
    {
         internal static void EndOfFile()
        {
            throw new EndOfStreamException("IO.EOF Read Beyond EOF");
        }

        internal static void EndReadCalledTwice()
        {
            throw new ArgumentException("Invalid Operation End Read Called Multiple");
        }

        internal static void EndWriteCalledTwice()
        {
            throw new ArgumentException("Invalid Operation End Write Called Multiple");
        }

        internal static void FileNotOpen()
        {
            throw new ObjectDisposedException(null, "Object Disposed File Closed");
        }

        internal static void MemoryStreamNotExpandable()
        {
            throw new NotSupportedException("Not Supported Memory Stream Not Expandable");
        }

        internal static void ReaderClosed()
        {
            throw new ObjectDisposedException(null, "Object Disposed Reader Closed");
        }

        internal static void ReadNotSupported()
        {
            throw new NotSupportedException("Not Supported Unreadable Stream");
        }

        internal static void SeekNotSupported()
        {
            throw new NotSupportedException("Not Supported Unseekable Stream");
        }

        internal static void StreamIsClosed()
        {
            throw new ObjectDisposedException(null, "Object Disposed Stream Closed");
        }

        internal static void WriteNotSupported()
        {
            throw new NotSupportedException("Not Supported Unwritable Stream");
        }

        internal static void WriterClosed()
        {
            throw new ObjectDisposedException(null, "Object Disposed Writer Closed");
        }

        internal static void WrongAsyncResult()
        {
            throw new ArgumentException("Arg Wrong Async Result");
        }

        internal static void UnauthorizedAccess(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new UnauthorizedAccessException("Unauthorized, Access is Denied!");
            }
            throw new UnauthorizedAccessException("Unauthorized, Access is Denied path: " + path);
        }

        internal static void SharingViolation(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new IOException("Sharing Violation error!");
            }
            throw new IOException("Sharing Violation error, path: " + path);
        }

        internal static void FileExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new IOException("File allready exists");
            }
            throw new IOException("File allready exists path: " + path);
        }

        internal static void FileOrPathNotFound(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new IOException("File or path not found");
            }
            throw new IOException("File or path not found: " + path);
        }

        [SecurityCritical]
        internal static void WinIOError(int errorCode, string path)
        {
            bool isInvalidPath = (errorCode == 0x7b) || (errorCode == 0xa1);
            switch (errorCode)
            {
                case 0x20:
                    SharingViolation(path);
                     break;
                case 80:
                    FileExists(path);
                    break;
                case 2:
                   FileOrPathNotFound(path);
                     break;
                case 3:
                    FileOrPathNotFound(path);
                     break;
                case 5:
                     UnauthorizedAccess(path);
                     break;
                case 15:
                    throw new DriveNotFoundException("Drivenot found " + path);
                case 0x57:
                    throw new IOException(Win32Io.GetMessage(errorCode));
                case 0xb7:
                    FileExists(path);
                    break;
                case 0xce:
                    throw new PathTooLongException("Path is too long");

                case 0x3e3:
                    throw new OperationCanceledException();
            }
            throw new IOException(Win32Io.GetMessage(errorCode));//, UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));
        }
    }
}
