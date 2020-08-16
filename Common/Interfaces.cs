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
using Nistec.IO;
using System.Collections;
using Nistec.Generic;
using Nistec.Serialization;


namespace Nistec.Data
{
    public interface IDataTableAdaptor 
    {
        void Prepare(System.Data.DataTable dt);
    }
    public interface IDataRowAdaptor
    {
        void Prepare(System.Data.DataRow dr);
    }
   
}

namespace Nistec
{
    public enum ListenerState
    {
        Down=0,
        Initilaized=1,
        Started=2,
        Stoped=3,
        Paused=4
    }
    public enum OnOffState
    {
        On = 0,
        Off = 1,
        Toggle = 2,
    }

    public interface IListener
    {
        void Start();
        void Stop();
        bool Pause(OnOffState onOff);
        void Shutdown(bool waitForWorkers);
        //bool Initialized { get; }
        ListenerState State { get; }
    }
}