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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections;
using Nistec.Logging;


namespace Nistec.Generic
{
    //The class helps looging exceptions and traces.
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// specific Mode
    /// <netlogSettings  LogFilename="C:\\Logs" LogLevel="Debug|Info|Warn|Error|Trace" LogMode="File|Console|Trace" IsAsync="false"/>
    /// All
    /// <netlogSettings  LogFilename="C:\\Logs" LogLevel="All" LogMode="File|Console"  IsAsync="false"/>
    /// </example>
    public static class Netlog
    {

        public static void Info(string message)
        {
            Logger.Instance.Log(LoggerLevel.Info, message, null);
        }
        public static void InfoFormat(string message, params object[] args)
        {
            Logger.Instance.Log(LoggerLevel.Info, message, args);
        }
        public static void Debug(string message)
        {
            Logger.Instance.Log(LoggerLevel.Debug, message, null);
        }
        public static void DebugFormat(string message, params object[] args)
        {
            Logger.Instance.Log(LoggerLevel.Debug, message, args);
        }
        public static void Warn(string message)
        {
            Logger.Instance.Log(LoggerLevel.Warn, message, null);
        }
        public static void WarnFormat(string message, params object[] args)
        {
            Logger.Instance.Log(LoggerLevel.Warn, message, args);
        }

        public static void Error(string message)
        {
            Logger.Instance.Log(LoggerLevel.Error, message, null);
        }

        public static void ErrorFormat(string message, params object[] args)
        {
            Logger.Instance.Log(LoggerLevel.Error, message, args);
        }

        public static void Exception(string message, Exception exception)
        {
            Logger.Instance.Exception(message, exception, false, false);
        }

        public static void Exception(string message, Exception exception, bool innerException, bool addStackTrace=false)
        {
            Logger.Instance.Exception(message, exception, innerException, addStackTrace);
        }

        public static void Trace(string method, bool begin)
        {
            Logger.Instance.Trace(method, begin);
        }
    }

}
