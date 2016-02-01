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
using Nistec.IO;
using System.IO;


namespace Nistec.Serialization
{

    /// <summary>
    /// Provide an extension methods for ISerialEntity
    /// </summary>
    public static class SerializerExtension
    {

        #region  IEntityFormatter

        /// <summary>
        /// Serialize <see cref="ISerialEntity"/> entity to <see cref="NetStream"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static NetStream Serialize(this ISerialEntity entity)
        {
            NetStream ns = new NetStream();
            var streamer = new BinaryStreamer(ns);
            streamer.WriteSerialEntity(entity,true);
            return ns;
        }

        /// <summary>
        /// Deserialize <see cref="NetStream"/> to <see cref="ISerialEntity"/> entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this NetStream stream) where T : ISerialEntity
        {
            var streamer = new BinaryStreamer(stream);
            return streamer.ReadSerialEntity<T>(true);
        }

        
        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="stream"></param>
        public static void EntityWrite(this ISerialEntity entity, Stream stream)
        {
            BinaryStreamer streamer = new BinaryStreamer(stream);
            streamer.WriteContextType(SerialContextType.SerialEntityType);
            entity.EntityWrite(stream, streamer);
        }
        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="stream"></param>
        public static void EntityRead(this ISerialEntity entity, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("EntityRead.stream");
            }
            if (stream.Length == 0)
            {
                throw new ArgumentException("EntityRead. stream is empty");
            }
            BinaryStreamer streamer = new BinaryStreamer(stream);
            streamer.ReadByte();
            entity.EntityRead(stream,streamer);
        }

        /// <summary>
        /// Encode entity to byte array.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static byte[] EntityEncode(this ISerialEntity entity)
        {
            using (NetStream ms = new NetStream())
            {
                entity.EntityWrite(ms, null);
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Decode entity from byte arrray.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="bytes"></param>
        public static void EntityDecode(this ISerialEntity entity, byte[] bytes)
        {
            using (NetStream ms = new NetStream(bytes))
            {
                entity.EntityRead(ms, null);
            }
        }



        /// <summary>
        /// Get entity as stream.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="writeContextType"></param>
        /// <returns></returns>
        public static NetStream GetEntityStream(this ISerialEntity entity, bool writeContextType)
        {
            NetStream ns = new NetStream();
            var streamer = new BinaryStreamer(ns);
            if (writeContextType)
            {
                streamer.WriteContextType(SerialContextType.SerialEntityType);
            }
            entity.EntityWrite(ns, streamer);
            return ns;
        }


        #endregion

    }
}
