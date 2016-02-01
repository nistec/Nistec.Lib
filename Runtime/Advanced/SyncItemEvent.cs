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

namespace Nistec.Runtime.Advanced
{
    
    /// <summary>
    /// SyncItemCompletedEventHandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SyncItemEventHandler<T>(object sender, SyncItemEventArgs<T> e);

    /// <summary>
    /// CacheEventArgs
    /// </summary>
    public class SyncItemEventArgs<T> : EventArgs
    {
        T item;

        /// <summary>
        /// SyncItemCompletedEventArgs
        /// </summary>
        /// <param name="item"></param>
        public SyncItemEventArgs(T item)
        {
            this.item = item;
        }

        #region Properties Implementation
        /// <summary>
        /// Items
        /// </summary>
        public T Item
        {
            get { return this.item; }
        }

        #endregion

    }

 }
