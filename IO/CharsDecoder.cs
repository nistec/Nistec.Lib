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
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System;
using System.Reflection;

namespace Nistec.IO
{

    internal class CharsDecoder
    {
        char[] buffer;
        int startIndex;
        int curIndex;
        int endIndex;

        internal CharsDecoder()
        {
        }

        internal int DecodedCount
        {
            get
            {
                return curIndex - startIndex;
            }
        }

        internal bool IsFull
        {
            get
            {
                return curIndex == endIndex;
            }
        }

        internal int Decode(char[] chars, int startPos, int len)
        {

            int copyCount = endIndex - curIndex;
            if (copyCount > len)
            {
                copyCount = len;
            }
            Buffer.BlockCopy(chars, startPos * 2, buffer, curIndex * 2, copyCount * 2);
            curIndex += copyCount;

            return copyCount;
        }

        internal int Decode(string str, int startPos, int len)
        {

            int copyCount = endIndex - curIndex;
            if (copyCount > len)
            {
                copyCount = len;
            }
            str.CopyTo(startPos, buffer, curIndex, copyCount);
            curIndex += copyCount;

            return copyCount;
        }

        internal void Reset()
        {
        }

        internal void SetNextOutputBuffer(Array buffer, int index, int count)
        {
            this.buffer = (char[])buffer;
            this.startIndex = index;
            this.curIndex = index;
            this.endIndex = index + count;
        }
    }
 

}
