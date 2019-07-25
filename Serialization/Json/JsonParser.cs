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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nistec.Serialization
{
    /// <summary>
    /// Represenent a json parser, see spec at http://www.json.org/
    /// </summary>
    internal sealed class JsonParser
    {
        enum Token
        {
            None = -1, 
            TagOpen,
            TagClose,
            TagArrayOpen,
            TagArrayClose,
            Colon,
            Comma,
            String,
            Number,
            True,
            False,
            Null
        }

        readonly string json;
        readonly StringBuilder sb = new StringBuilder();
        Token nextToken = Token.None;
        int index;
        bool ignorecase = false;
        bool isGenericType;
        bool isStringType;
        Type returnType;
        Type elementType;

        public static object Parse(string json, Type returnType, bool ignorecase)
        {
            if (json == null)
                return null;
            if (returnType == null)
                returnType = SerializeTools.GetMethodReturnType("Nistec.Serialization.JsonParser", "Parse");

            JsonParser parser = new JsonParser(json, ignorecase, returnType);
            return parser.ParseValue();
        }
        public static void ParseTo(Dictionary<string, object> d,string json, bool ignorecase = false)
        {
            if (json == null)
                return;
            JsonParser parser = new JsonParser(json, ignorecase, typeof(Dictionary<string, object>));
            parser.ParseTo(d);
        }
        public static void ParseTo(Dictionary<string, string> d,string json, bool ignorecase = false)
        {
            if (json == null)
                return;
            JsonParser parser = new JsonParser(json, ignorecase, typeof(Dictionary<string, string>));
            parser.ParseTo(d);
        }
        public static void ParseTo<T>(List<T> d, string json, bool ignorecase = false)
        {
            if (json == null)
                return;
            JsonParser parser = new JsonParser(json, ignorecase, typeof(List<T>));
            parser.ParseTo<T>(d);
        }

        //public static Dictionary<string,object> ParseToDictionary(string json, bool ignorecase=false)
        //{
        //    if (json == null)
        //        return null;
        //    JsonParser parser = new JsonParser(json, ignorecase,typeof(Dictionary<string, object>));
        //    return parser.ParseObject();
        //}
        //public static Dictionary<string, string> ParseToDictionaryString(string json, bool ignorecase = false)
        //{
        //    if (json == null)
        //        return null;
        //    JsonParser parser = new JsonParser(json, ignorecase, typeof(Dictionary<string, string>));
        //    return parser.ParseObjectString();
        //}
        private JsonParser(string json, bool ignorecase, Type returnType)
        {
            this.json = json;//.ToCharArray();
            this.ignorecase = ignorecase;
            if (returnType == null)
            {
                throw new ArgumentNullException("JsonParser.returnType");
            }
            this.returnType = returnType;
            this.elementType = SerializeTools.GetElementType(returnType);
            this.isGenericType = returnType.IsGenericType;
            this.isStringType = elementType == typeof(string);
        }

        //public KeyValuePair<string, object> ParseToken()
        //{
        //    string name = null;
        //    object value = null;

        //    ConsumeToken();

        //    while (true)
        //    {
        //        var token = FindNext();
        //        switch (token)
        //        {

        //            case Token.Comma:
        //                ConsumeToken();
        //                break;

        //            case Token.TagClose:
        //                ConsumeToken();
        //                return new KeyValuePair<string, object>(name, value);

        //            default:
        //                {
        //                    // name
        //                    name = ParseString();
        //                    if (ignorecase)
        //                        name = name.ToLower();

        //                    // :
        //                    if (NextToken() != Token.Colon)
        //                    {
        //                        throw new Exception("JsonParser error: Expected colon at index " + index);
        //                    }
        //                    // value
        //                    value = ParseValue();
        //                    break;
        //                }
        //        }
        //    }
        //}
        private void ParseTo<T>(List<T> d)
        {

            ConsumeToken(); // {

            while (true)
            {
                var token = FindNext();
                switch (token)
                {

                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.TagClose:
                        ConsumeToken();
                        return;

                    default:
                        {
                            // name
                            //string name = ParseString();
                            //if (ignorecase)
                            //    name = name.ToLower();
                            //// :
                            //if (NextToken() != Token.Colon)
                            //{
                            //    throw new Exception("JsonParser error: Expected colon at index " + index);
                            //}
                            // value
                            object value = ParseValue();

                            d.Add(GenericTypes.Convert<T>(value));
                        }
                        break;
                }
            }
        }
        private void ParseTo<T>(Dictionary<string, T> d)
        {

            ConsumeToken(); // {
            
            while (true)
            {
                var token = FindNext();
                switch (token)
                {

                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.TagClose:
                        ConsumeToken();
                        return;

                    default:
                        {
                            // name
                            bool readToken = (token != Token.String);
                            string name = ParseString(readToken);
                            
                            if (ignorecase)
                                name = name.ToLower();
                            // :
                            if (NextToken() != Token.Colon)
                            {
                                throw new Exception("JsonParser error: Expected colon at index " + index);
                            }
                            // value
                            object value = ParseValue();

                            d[name] = GenericTypes.Convert<T>(value);
                        }
                        break;
                }
            }
        }
        private void ParseTo(Dictionary<string, string> d)
        {

            ConsumeToken(); // {

            while (true)
            {
                var token = FindNext();
                switch (token)
                {

                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.TagClose:
                        ConsumeToken();
                        return;

                    default:
                        {
                            // name
                            bool readToken = (token != Token.String);
                            string name = ParseString(readToken);

                            if (ignorecase)
                                name = name.ToLower();
                            // :
                            if (NextToken() != Token.Colon)
                            {
                                throw new Exception("JsonParser error: Expected colon at index " + index);
                            }
                            // value
                            object value = ParseValue();

                            d[name] = value == null ? null : value.ToString();
                        }
                        break;
                }
            }
        }
        private void ParseTo(Dictionary<string, object> d)
        {

            ConsumeToken(); // {
            
            while (true)
            {
                var token = FindNext();
                switch (token)
                {

                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.TagClose:
                        ConsumeToken();
                        return;

                    default:
                        {
                            // name
                            bool readToken = (token != Token.String);
                            string name = ParseString(readToken);
                            
                            if (ignorecase)
                                name = name.ToLower();
                            // :
                            if (NextToken() != Token.Colon)
                            {
                                throw new Exception("JsonParser error: Expected colon at index " + index);
                            }
                            // value
                            object value = ParseValue();

                            d[name] = value;
                        }
                        break;
                }
            }
        }
        private Dictionary<string, string> ParseObjectString()
        {
            Dictionary<string, string> table = new Dictionary<string, string>();

            ConsumeToken(); // {
            
            while (true)
            {
                var token = FindNext();
                switch (token)
                {

                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.TagClose:
                        ConsumeToken();
                        return table;

                    default:
                        {
                            bool readToken = (token != Token.String);
                            // name
                            string name = ParseString(readToken);
                            
                            if (ignorecase)
                                name = name.ToLower();

                            // :
                            if (NextToken() != Token.Colon)
                            {
                                throw new Exception("JsonParser error: Expected colon at index " + index);
                            }

                            // value
                            object value = ParseValue();

                            table[name] = value == null ? null : value.ToString();
                        }
                        break;
                }
            }
        }

        private Dictionary<string, object> ParseObject()
        {
            Dictionary<string, object> table = new Dictionary<string, object>();

            ConsumeToken(); // {
            
            while (true)
            {
                var token = FindNext();
                switch (token)
                {

                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.TagClose:
                        ConsumeToken();
                        return table;

                    default:
                        {
                            // name
                            bool readToken = (token != Token.String);
                            string name = ParseString(readToken);
                            
                            if (ignorecase)
                                name = name.ToLower();

                            // :
                            if (NextToken() != Token.Colon)
                            {
                                throw new Exception("JsonParser error: Expected colon at index " + index);
                            }

                            // value
                            object value = ParseValue();

                            table[name] = value;
                        }
                        break;
                }
            }
        }

        private List<string> ParseArrayString()
        {
            List<string> array = new List<string>();
            ConsumeToken(); // [

            while (true)
            {
                var token = FindNext();
                switch (token)
                {
                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.TagArrayClose:
                        ConsumeToken();
                        return array;

                    default:
                        var val = ParseValue();
                        array.Add(val == null ? null : val.ToString());
                        break;
                }
            }
        }
        private List<object> ParseArray()
        {
            List<object> array = new List<object>();
            ConsumeToken(); // [

            while (true)
            {
                var token = FindNext();
                switch (token)
                {
                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.TagArrayClose:
                        ConsumeToken();
                        return array;

                    default:
                        array.Add(ParseValue());
                        break;
                }
            }
        }

        private object ParseValue()
        {
            var token = FindNext();
            switch (token)
            {
                case Token.Number:
                    return ParseNumber();

                case Token.String:
                    return ParseString(false);

                case Token.TagOpen:
                    return ParseObject();

                case Token.TagArrayOpen:
                    if (isStringType)
                    {
                        return ParseArrayString();
                        //var list= ParseArrayString();
                        //if (isGenericType)
                        //    return list.ToArray();
                        //return list;
                    }
                    else
                    {
                        return ParseArray();
                        //var list = ParseArray();
                        //if (isGenericType)
                        //    return list.ToArray();
                        //return list;
                    }
                case Token.True:
                    ConsumeToken();
                    return true;

                case Token.False:
                    ConsumeToken();
                    return false;

                case Token.Null:
                    ConsumeToken();
                    return null;

                case Token.Colon:
                    ConsumeToken();
                    return ":";

            }

            throw new Exception("JsonParser error: Invalid token at index" + index);
        }

        private string ParseString(bool readToken)
        {
            ConsumeToken(); // "

            sb.Length = 0;

            int runIndex = -1;

            if (readToken)
            {
                //if (json[index] == '"')
                index++;
            }

            while (index < json.Length)
            {
                var c = json[index++];

                if (c == '"')
                {
                    //if (runIndex == -1)
                    //{
                    //    runIndex = index;
                    //    continue;
                    //}
                    if (runIndex != -1)
                    {
                        if (sb.Length == 0)
                            return json.Substring(runIndex, index - runIndex - 1);

                        sb.Append(json, runIndex, index - runIndex - 1);
                    }
                    return sb.ToString();
                }
                if (c != '\\')
                {
                    if (runIndex == -1)
                        runIndex = index - 1;

                    continue;
                }

                if (index == json.Length) break;

                if (runIndex != -1)
                {
                    sb.Append(json, runIndex, index - runIndex - 1);
                    runIndex = -1;
                }

                switch (json[index++])
                {
                    case '"':
                        sb.Append('"');
                        break;

                    case '\\':
                        sb.Append('\\');
                        break;

                    case '/':
                        sb.Append('/');
                        break;

                    case 'b':
                        sb.Append('\b');
                        break;

                    case 'f':
                        sb.Append('\f');
                        break;

                    case 'n':
                        sb.Append('\n');
                        break;

                    case 'r':
                        sb.Append('\r');
                        break;

                    case 't':
                        sb.Append('\t');
                        break;

                    case 'u':
                        {
                            int remainingLength = json.Length - index;
                            if (remainingLength < 4) break;

                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint = ParseUnicode(json[index], json[index + 1], json[index + 2], json[index + 3]);
                            sb.Append((char)codePoint);

                            // skip 4 chars
                            index += 4;
                        }
                        break;
                }
            }

            throw new Exception("JsonParser error: Unexpected reached end of string");
        }
        private string __ParseString()
        {
            ConsumeToken(); // "

            sb.Length = 0;

            int runIndex = -1;

            while (index < json.Length)
            {
                var c = json[index++];

                if (c == '"')
                {
                    if (runIndex != -1)
                    {
                        if (sb.Length == 0)
                            return json.Substring(runIndex, index - runIndex - 1);

                        sb.Append(json, runIndex, index - runIndex - 1);
                    }
                    return sb.ToString();
                }
                if (c != '\\')
                {
                    if (runIndex == -1)
                        runIndex = index - 1;

                    continue;
                }

                if (index == json.Length) break;

                if (runIndex != -1)
                {
                    sb.Append(json, runIndex, index - runIndex - 1);
                    runIndex = -1;
                }

                switch (json[index++])
                {
                    case '"':
                        sb.Append('"');
                        break;

                    case '\\':
                        sb.Append('\\');
                        break;

                    case '/':
                        sb.Append('/');
                        break;

                    case 'b':
                        sb.Append('\b');
                        break;

                    case 'f':
                        sb.Append('\f');
                        break;

                    case 'n':
                        sb.Append('\n');
                        break;

                    case 'r':
                        sb.Append('\r');
                        break;

                    case 't':
                        sb.Append('\t');
                        break;

                    case 'u':
                        {
                            int remainingLength = json.Length - index;
                            if (remainingLength < 4) break;

                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint = ParseUnicode(json[index], json[index + 1], json[index + 2], json[index + 3]);
                            sb.Append((char)codePoint);

                            // skip 4 chars
                            index += 4;
                        }
                        break;
                }
            }

            throw new Exception("JsonParser error: Unexpected reached end of string");
        }

        private uint ParseSingleChar(char c1, uint multipliyer)
        {
            uint p1 = 0;
            if (c1 >= '0' && c1 <= '9')
                p1 = (uint)(c1 - '0') * multipliyer;
            else if (c1 >= 'A' && c1 <= 'F')
                p1 = (uint)((c1 - 'A') + 10) * multipliyer;
            else if (c1 >= 'a' && c1 <= 'f')
                p1 = (uint)((c1 - 'a') + 10) * multipliyer;
            return p1;
        }

        private uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            uint p1 = ParseSingleChar(c1, 0x1000);
            uint p2 = ParseSingleChar(c2, 0x100);
            uint p3 = ParseSingleChar(c3, 0x10);
            uint p4 = ParseSingleChar(c4, 1);

            return p1 + p2 + p3 + p4;
        }

        private long CreateLong(string s)
        {
            long num = 0;
            bool neg = false;
            foreach (char cc in s)
            {
                if (cc == '-')
                    neg = true;
                else if (cc == '+')
                    neg = false;
                else
                {
                    num *= 10;
                    num += (int)(cc - '0');
                }
            }

            return neg ? -num : num;
        }

        private object ParseNumber()
        {
            ConsumeToken();

            // Need to start back one place because the first digit is also a token
            var startIndex = index - 1;
            bool isDecimal = false;
            do
            {
                if (index == json.Length)
                    break;
                var c = json[index];

                if ((c >= '0' && c <= '9') || c == '.' || c == '-' || c == '+' || c == 'e' || c == 'E')
                {
                    if (c == '.' || c == 'e' || c == 'E')
                        isDecimal = true;
                    if (++index == json.Length)
                        break;//throw new Exception("Unexpected end of string when parsing a number");
                    continue;
                }
                break;
            } while (true);

            if (isDecimal)
            {
                string s = json.Substring(startIndex, index - startIndex);
                return double.Parse(s, NumberFormatInfo.InvariantInfo);
            }
            long num;
            return JsonParser.ToLong(out num, json, startIndex, index - startIndex);
        }

        private Token FindNext()
        {
            if (nextToken != Token.None) return nextToken;

            return nextToken = InternalNextToken();
        }

        private void ConsumeToken()
        {
            nextToken = Token.None;
        }

        private Token NextToken()
        {
            var result = nextToken != Token.None ? nextToken : InternalNextToken();

            nextToken = Token.None;

            return result;
        }

     
        private Token InternalNextToken()
        {
            char c;

            // Skip past whitespace
            do
            {
                c = json[index];

                if (c > ' ') break;
                if (c != ' ' && c != '\t' && c != '\n' && c != '\r') break;

            } while (++index < json.Length);

            if (index == json.Length)
            {
                throw new Exception("JsonParser error: Unexpected reached end of string");
            }

            c = json[index];

            index++;
            
            switch (c)
            {
                case '{':
                    return Token.TagOpen;

                case '}':
                    return Token.TagClose;

                case '[':
                    return Token.TagArrayOpen;

                case ']':
                    return Token.TagArrayClose;

                case ',':
                    return Token.Comma;

                case '"':
                    return Token.String;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                case '+':
                case '.':
                    return Token.Number;

                case ':':
                    return Token.Colon;

                case 'f':
                    if (json.Length - index >= 4 &&     
                        json[index + 0] == 'a' &&
                        json[index + 1] == 'l' &&
                        json[index + 2] == 's' &&
                        json[index + 3] == 'e')
                    {
                        index += 4;
                        return Token.False;
                    }
                    break;

                case 't':
                    if (json.Length - index >= 3 &&
                        json[index + 0] == 'r' &&
                        json[index + 1] == 'u' &&
                        json[index + 2] == 'e')
                    {
                        index += 3;
                        return Token.True;
                    }
                    break;

                case 'n':
                    if (json.Length - index >= 3 &&
                        json[index + 0] == 'u' &&
                        json[index + 1] == 'l' &&
                        json[index + 2] == 'l')
                    {
                        index += 3;
                        return Token.Null;
                    }
                    break;
            }
            throw new Exception("JsonParser error: Could not find token at index " + --index);
            
        }
        internal static long ToLong(out long num, string s, int index, int count)
        {
            num = 0;
            bool neg = false;
            for (int x = 0; x < count; x++, index++)
            {
                char cc = s[index];

                if (cc == '-')
                    neg = true;
                else if (cc == '+')
                    neg = false;
                else
                {
                    num *= 10;
                    num += (int)(cc - '0');
                }
            }
            if (neg) num = -num;

            return num;
        }

    }
}
