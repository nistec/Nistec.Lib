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


namespace Nistec.Runtime
{
    public interface IMessage : IMessageStream
    {
       /// <summary>
        /// Get or Set The message key.
        /// </summary>
        string Key { get; set; }
        /// <summary>
        /// Get or Set The serializer formatter.
        /// </summary>
        Formatters Formatter { get; set; }
        /// <summary>
        /// Get or Set The message key.
        /// </summary>
        string Id { get; set; }
        /// <summary>
        /// Get or Set The message command.
        /// </summary>
        string Command { get; set; }
        /// <summary>
        /// Get or Set indicate wether the message is a duplex type.
        /// </summary>
        bool IsDuplex { get; set; }
        /// <summary>
        ///  Get or Set The message expiration.
        /// </summary>
        int Expiration { get; set; }
        /// <summary>
        /// Get or Set The last time that message was modified.
        /// </summary>
        DateTime Modified { get; set; }
        /// <summary>
        /// Get or Set The extra arguments for current message.
        /// </summary>
        GenericNameValue Args { get; set; }
    }

    public interface IMessageStream
    {
        NetStream GetBodyStream();

        NetStream BodyStream { get; }

        string TypeName { get; }

        void SetBody(object value);

        void SetBody(byte[] body, Type type);

        object DecodeBody();

        bool IsEmpty { get; }
    }

   

    
}
