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
using Nistec.IO;
using Nistec.Serialization.Binary;
using Nistec.Serialization.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Serialization
{
    public interface ISerialEntity
    {
        /// <summary>
        /// Write entity to stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        void EntityWrite(Stream stream, IBinaryStreamer streamer);

        /// <summary>
        /// Read entity from stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        void EntityRead(Stream stream, IBinaryStreamer streamer);

        /*
        /// <summary>
        /// Get entity as stream.
        /// </summary>
        /// <param name="writeContextType"></param>
        /// <returns></returns>
        NetStream GetEntityStream(bool writeContextType);
        
        /// <summary>
        /// Set entity from stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="writeContextType"></param>
        /// <returns></returns>
        void SetEntityStream(NetStream stream, bool readContextType);
         */
    }

    public interface ISerialJson
    {
        /// <summary>
        /// Write entity to stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="serializer"></param>
        string EntityWrite(IJsonSerializer serializer);

        /// <summary>
        /// Read entity from json.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="serializer"></param>
        object EntityRead(string json, IJsonSerializer serializer);
    }

    public interface IEntityDictionary : ISerialEntity
    {
        IDictionary EntityDictionary();
        Type EntityType();
    }

    public interface ISerialContext
    {
        void ReadContext(ISerializerContext context);
        void WriteContext(ISerializerContext context);
    }

    public interface IBinaryStreamer
    {
        Stream BaseStream { get; }
        void WriteValue(object value, Type baseType = null);
        void WriteCount(int value);
        int ReadCount();
        object ReadValue();
        object StreamToValue(NetStream stream);
        T ReadValue<T>();
        T ReadValue<T>(SerialContextType contextType);
        void WriteString(string str);
        string ReadString();
        void WriteFixedString(string str, int length);
        string ReadFixedString();
        void Flush();
        T ReadMapValue<T>();
        string ReadMapString();
        List<int> GetMapper();
        void MapperBegin();
    }

    public interface ISerializerContext
    {
        Stream BaseStream { get; }
        SerializeInfo ReadSerializeInfo();
        void WriteSerializeInfo(SerializeInfo info);
    }
}
