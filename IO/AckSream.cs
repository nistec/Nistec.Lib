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
//using Nistec.Channels;
using Nistec.Runtime;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Nistec.IO
{

    /// <summary>
    /// Represent a ack stream for named pipe/tcp communication.
    /// </summary>
    [Serializable]
    public class AckStream : NetStream, IDisposable
    {
        #region static
        public static AckStream GetAckStream(bool state, string action)
        {
            return new AckStream(state ? MessageState.Ok : MessageState.Failed,
                state ? action + " succseed" : action = " failed");
        }
         public static AckStream GetAckStream(object value, string action)
        {
            if (value == null)
                return AckStream.GetAckStream(false, action);
            return new AckStream(value);
        }
         public static AckStream GetAckNotFound(string action, string key)
         {
             return new AckStream(MessageState.ItemNotFound, action + ": " + key + " , item not found.");
         }

        // public static AckStream Read(NetworkStream stream, Type returnType, int readTimeout, int InBufferSize)
        //{
        //    return new AckStream(stream, returnType, readTimeout, InBufferSize);
        //}

        // public static AckStream Read(PipeStream stream, Type returnType, int InBufferSize)
        //{
        //    return new AckStream(stream, returnType, InBufferSize);
        //}

         public static AckStream Read(NetStream stream, TransformType returnType, int InBufferSize = 4096)
        {
            if (stream != null)
                stream.Position = 0;
            return new AckStream(stream, returnType, InBufferSize);
        }
        #endregion

        #region properties

        public MessageState State { get; private set; }
        public Formatters Formatter { get { return Formatters.BinarySerializer; } }
        public string Message { get; private set; }
        public DateTime Modified { get; private set; }
        public object Value { get; private set; }

        public T GetValue<T>()
        {
            if(Value==null)
                return default(T);
            return GenericTypes.Cast<T>(Value);
        }

        #endregion

        #region ctor

        public AckStream(MessageState state, string message)
        {
            Modified = DateTime.Now;
            State = state;
            Message = message;
            WriteAck(null, null, state, message);
        }

        public AckStream(NetStream stream, string typeName)
        {
            Modified = DateTime.Now;
            State= MessageState.Ok;
            Message = null;
            WriteAck(stream, typeName,MessageState.Ok, null);
        }
         
        public AckStream(object value)
        {
            Modified = DateTime.Now;
            State = MessageState.Ok;
            Message = null;
            WriteAck(value, value == null ? null : value.GetType().FullName, MessageState.Ok, null);
        }

        public AckStream(NetworkStream stream, TransformType returnType, int readTimeout, int InBufferSize)
        {
            //CopyWithTerminateCount(stream, readTimeout, InBufferSize);
            CopyFrom(stream, readTimeout, InBufferSize);
            ReadAck(returnType);
        }

        public AckStream(PipeStream stream, TransformType returnType, int InBufferSize)
        {
            CopyFrom(stream, InBufferSize);
            ReadAck(returnType);
        }

        private AckStream(NetStream stream, TransformType returnType, int InBufferSize)
            : base(stream.ToArray())
        {
            ReadAck(returnType);
        }

        //public AckStream(NetworkStream stream, Type returnType, int readTimeout, int InBufferSize)
        //{
        //    //CopyWithTerminateCount(stream, readTimeout, InBufferSize);
        //    CopyFrom(stream,readTimeout, InBufferSize);
        //    ReadAck(returnType);
        //}

        //public AckStream(PipeStream stream, Type returnType, int InBufferSize)
        //{
        //    CopyFrom(stream, InBufferSize);
        //    ReadAck(returnType);
        //}

        //private AckStream(NetStream stream, Type returnType, int InBufferSize)
        //    : base(stream.ToArray())
        //{
        //    ReadAck(returnType);
        //}

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Message = null;
                Value = null;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region  ISerialEntity

        void WriteAck(object value, string typeName, MessageState state, string message)
        {
            IBinaryStreamer streamer = new BinaryStreamer(this);

            base.Clear();

            if (typeName == null)
                typeName = value == null ? null : value.GetType().FullName;


            //streamer.WriteValue((int)0);
            streamer.WriteValue((int)State);
            streamer.WriteString(message);
            streamer.WriteValue(DateTime.Now);
            streamer.WriteValue(value);
            streamer.WriteString(typeName);
            int count = this.iLength;
            //this.Replace(count, 1);
            streamer.Flush();
        }

        void ReadAck()
        {
            IBinaryStreamer streamer = new BinaryStreamer(this);
            this.Position = 0;
            //int count = streamer.ReadValue<int>();
            State = (MessageState)streamer.ReadValue<int>();
            Message = streamer.ReadString();
            Modified = streamer.ReadValue<DateTime>();
            object value = streamer.ReadValue();
            string TypeName = streamer.ReadString();
            if (value != null && value.GetType() == typeof(NetStream) && !TypeName.Contains("NetStream"))
            {
                Value = streamer.StreamToValue((NetStream)value);
            }
            else
            {
                Value = value;
            }
        }

        //void ReadAck(Type returnType)
        //{
        //    IBinaryStreamer streamer = new BinaryStreamer(this);
        //    this.Position = 0;
        //    //int count = streamer.ReadValue<int>();
        //    State = (MessageState)streamer.ReadValue<int>();
        //    Message = streamer.ReadString();
        //    Modified = streamer.ReadValue<DateTime>();
        //    object value = streamer.ReadValue();
        //    string TypeName = streamer.ReadString();

        //    if (value == null)
        //        Value = value;
        //    else if (returnType == value.GetType())
        //        Value = value;
        //    else if (value.GetType() == typeof(NetStream) && !TypeName.Contains("NetStream"))
        //        Value = streamer.StreamToValue((NetStream)value);
        //    else
        //        Value = value;

        //    //if (TypeName != null && Type.GetType(TypeName) == typeof(JsonType))
        //    //    Value = ValueToJson();

        //}

        void ReadAck(TransformType returnType)
        {
            IBinaryStreamer streamer = new BinaryStreamer(this);
            this.Position = 0;
            //int count = streamer.ReadValue<int>();
            State = (MessageState)streamer.ReadValue<int>();
            Message = streamer.ReadString();
            Modified = streamer.ReadValue<DateTime>();
            object value = streamer.ReadValue();
            string TypeName = streamer.ReadString();

            if (value == null)
                Value = value;
            else if(returnType== TransformType.Stream)
            {
                if(value.GetType() == typeof(NetStream))
                    Value = value;
                else
                    Value = BinarySerializer.SerializeToStream(value);
            }
            else if (value.GetType() == typeof(NetStream))// && !TypeName.Contains("NetStream"))
                Value = streamer.StreamToValue((NetStream)value);
            else
                Value = value;

            //if (TypeName != null && Type.GetType(TypeName) == typeof(JsonType))
            //    Value = ValueToJson();

        }

        #endregion

        public string ValueToJson()
        {
            return JsonSerializer.Serialize(Value);
        }

        public string ToJson()
        {
            ReadAck();
            return JsonSerializer.Serialize(Value);
        }

        //public NetStream ToJsonStream()
        //{
        //    string json = ToJson();
        //    NetStream nstream = new NetStream();
        //    var bytes= Encoding.UTF8.GetBytes(json);
        //    nstream.Write(bytes, 0, bytes.Length);
        //    return nstream;
        //}

    }

}
