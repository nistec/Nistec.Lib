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
using Nistec.Generic;
using Nistec.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Serialization
{
    public static class JsonConverter
    {

        public static object ChangeType(object value, Type type, bool useUTCDateTime)
        {
            try
            {
                //if (value == null || value == DBNull.Value || value.ToString() == "")
                //{
                //    if (IsNullable(type))
                //        return value;
                //    else
                //        return UnderlyingTypeOf(type);
                //}


                if (type.IsNullableType())
                {
                    Type underLineType = Nullable.GetUnderlyingType(type) ?? type;

                    //Coalesce to set the safe value using default(t) or the safe type.
                    if (value == null || value.ToString() == "")
                        return GenericTypes.Default(type);
                    
                    else if (underLineType == typeof(bool))
                        return Types.StringToBool(value.ToString(), false);
                    else if (underLineType == typeof(DateTime))
                        return ToDateTimeObject(value.ToString(), true);

                    return Convert.ChangeType(value, underLineType, CultureInfo.InvariantCulture);
                }
                if (type == typeof(string))
                {
                    return (string)value;
                }
                if (Types.IsNumericType(type))
                {
                    return Types.IsNumeric(value) == false ? GenericTypes.Default(type) : Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                }
                if (type == typeof(bool))
                {
                    return Types.StringToBool(value.ToString(), false);
                }
                if (type == typeof(DateTime))
                {
                    return ToDateTime(value.ToString(), useUTCDateTime);
                }
                if (type == typeof(Guid))
                {
                    return Types.ToGuid(value);
                }
                return (value == null || value == DBNull.Value) ? GenericTypes.Default(type) : Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return GenericTypes.Default(type);
            }
        }


        #region   private methods
      
  
        public static bool IsNullable(Type t)
        {
            if (!t.IsGenericType) return false;
            Type g = t.GetGenericTypeDefinition();
            return (g.Equals(typeof(Nullable<>)));
        }

        public static Type UnderlyingTypeOf(Type t)
        {
            return t.GetGenericArguments()[0];
        }

        internal static int ToInteger(string s, int index, int count)
        {
            s = s.Substring(index, count);
            s = s.TrimStart('0','-', '+');
            s = s.TrimEnd();
            return Types.ToInt(s);
        }

        public static Guid ToGuid(string s)
        {
            if (s == null)
                return Guid.Empty;

            if (s.Length > 30)
                return new Guid(s);
            else
                return new Guid(Convert.FromBase64String(s));
        }

        public static object ToEnum(Type type, object value)
        {
            if (value == null)
                return value;
            return EnumExtension.Parse(type, value.ToString(), null);
        }
        public static object ToDateTimeObject(string value, bool useUTCDateTime)
        {

            if (value == null || value == string.Empty)
                return null;
            if (value.Length < 19)
                return null;

            return ToDateTime(value, useUTCDateTime);
        }

        public static DateTime ToDateTime(string value, bool useUTCDateTime)
        {
            bool utc = false;
            //                   0123456789012345678 9012 9/3
            // datetime format = yyyy-MM-dd HH:mm:ss .nnn  Z

            int hour = 0;
            int min = 0;
            int sec = 0;
            int ms = 0;

            int year = ToInteger(value, 0, 4);
            int month = ToInteger(value, 5, 2);
            int day = ToInteger(value, 8, 2);
            if (value.Length < 16)
            {
                if (useUTCDateTime == false && utc == false)
                    return new DateTime(year, month, day);
                else
                    return new DateTime(year, month, day, 0, 0, 0, ms, DateTimeKind.Utc).ToLocalTime();
            }

            if (value.Length > 11)
                hour = ToInteger(value, 11, 2);
            if (value.Length > 14)
                min = ToInteger(value, 14, 2);
            if (value.Length > 17)
                sec = ToInteger(value, 17, 2);
            if (value.Length > 21 && value[19] == '.')
                ms = ToInteger(value, 20, 3);

            //if (value.EndsWith("Z"))
            if (value[value.Length - 1] == 'Z')
                utc = true;

            if (useUTCDateTime == false && utc == false)
                return new DateTime(year, month, day, hour, min, sec, ms);
            else
                return new DateTime(year, month, day, hour, min, sec, ms, DateTimeKind.Utc).ToLocalTime();
        }

        #endregion

        #region formatter

        internal static string Indent = "   ";

        internal static void AppendIndent(StringBuilder sb, int count)
        {
            for (; count > 0; --count) sb.Append(Indent);
        }
        public static string PrintJson(string input)
        {
            var output = new StringBuilder();
            int depth = 0;
            int len = input.Length;
            char[] chars = input.ToCharArray();
            for (int i = 0; i < len; ++i)
            {
                char ch = chars[i];

                if (ch == '\"') // found string span
                {
                    bool str = true;
                    while (str)
                    {
                        output.Append(ch);
                        ch = chars[++i];
                        if (ch == '\\')
                        {
                            output.Append(ch);
                            ch = chars[++i];
                        }
                        else if (ch == '\"')
                            str = false;
                    }
                }

                switch (ch)
                {
                    case '{':
                    case '[':
                        output.Append(ch);
                        output.AppendLine();
                        AppendIndent(output, ++depth);
                        break;
                    case '}':
                    case ']':
                        output.AppendLine();
                        AppendIndent(output, --depth);
                        output.Append(ch);
                        break;
                    case ',':
                        output.Append(ch);
                        output.AppendLine();
                        AppendIndent(output, depth);
                        break;
                    case ':':
                        output.Append(" : ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(ch))
                            output.Append(ch);
                        break;
                }
            }

            return output.ToString();
        }
        #endregion
    }
}
