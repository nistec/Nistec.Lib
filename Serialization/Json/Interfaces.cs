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

namespace Nistec.Serialization
{
    public interface IJsonSerializer
    {
        string Write(object obj);

        string Write(object obj, Type baseType);

        object Read(string json, Type type);

        T Read<T>(string json);
    }

    public interface ISerialJson
    {
        /// <summary>
        /// Write entity to stream.
        /// </summary>
        /// <param name="serializer"></param>
        string EntityWrite(IJsonSerializer serializer);

        /// <summary>
        /// Read entity from json.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="serializer"></param>
        object EntityRead(string json, IJsonSerializer serializer);
    }
}
