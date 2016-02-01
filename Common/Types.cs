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
using System.Globalization;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


namespace Nistec
{

	#region Strings

    public sealed class Strings
    {
        internal static readonly CompareInfo m_InvariantCompareInfo;

        static Strings()
        {
            Strings.m_InvariantCompareInfo = CultureInfo.InvariantCulture.CompareInfo;
        }

        public static string StrReverse(string Expression)
        {
            if (Expression == null)
            {
                return "";
            }
            int num1 = Expression.Length;
            if (num1 == 0)
            {
                return "";
            }
            int num3 = num1 - 1;
            for (int num2 = 0; num2 <= num3; num2++)
            {
                char ch1 = Expression[num2];
                switch (char.GetUnicodeCategory(ch1))
                {
                    case UnicodeCategory.Surrogate:
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.EnclosingMark:
                        return Strings.InternalStrReverse(Expression, num2, num1);
                }
            }
            char[] chArray1 = Expression.ToCharArray();
            Array.Reverse(chArray1);
            return new string(chArray1);
        }

        private static string InternalStrReverse(string Expression, int SrcIndex, int Length)
        {
            StringBuilder builder1 = new StringBuilder(Length);
            builder1.Length = Length;
            TextElementEnumerator enumerator1 = StringInfo.GetTextElementEnumerator(Expression, SrcIndex);
            if (!enumerator1.MoveNext())
            {
                return "";
            }
            int num2 = 0;
            int num1 = Length - 1;
            while (num2 < SrcIndex)
            {
                builder1[num1] = Expression[num2];
                num1--;
                num2++;
            }
            int num3 = enumerator1.ElementIndex;
            while (num1 >= 0)
            {
                SrcIndex = num3;
                if (enumerator1.MoveNext())
                {
                    num3 = enumerator1.ElementIndex;
                }
                else
                {
                    num3 = Length;
                }
                for (num2 = num3 - 1; num2 >= SrcIndex; num2--)
                {
                    builder1[num1] = Expression[num2];
                    num1--;
                }
            }
            return builder1.ToString();
        }

        public static int Asc(char String)
        {
            int num1;
            int num2 = Convert.ToInt32(String);
            if (num2 < 0x80)
            {
                return num2;
            }
            try
            {
                byte[] buffer1;
                int num3;
                Encoding encoding1 = Types.GetFileIOEncoding();
                char[] chArray1 = new char[] { String };
                if (encoding1.GetMaxByteCount(1) == 1)
                {
                    buffer1 = new byte[1];
                    num3 = encoding1.GetBytes(chArray1, 0, 1, buffer1, 0);
                    return buffer1[0];
                }
                buffer1 = new byte[2];
                num3 = encoding1.GetBytes(chArray1, 0, 1, buffer1, 0);
                if (num3 == 1)
                {
                    return buffer1[0];
                }
                if (BitConverter.IsLittleEndian)
                {
                    byte num4 = buffer1[0];
                    buffer1[0] = buffer1[1];
                    buffer1[1] = num4;
                }
                num1 = BitConverter.ToInt16(buffer1, 0);
            }
            catch (Exception exception1)
            {
                throw exception1;
            }
            return num1;
        }

        public static int Asc(string String)
        {
            if ((String == null) || (String.Length == 0))
            {
                throw new ArgumentException("Argument_Length Zero", "String");
            }
            char ch1 = String[0];
            return Strings.Asc(ch1);
        }

        public static int AscW(string String)
        {
            if ((String == null) || (String.Length == 0))
            {
                throw new ArgumentException("Argument_Length Zero", "String");
            }
            return String[0];
        }

        public static int AscW(char String)
        {
            return String;
        }

        public static char Chr(int CharCode)
        {
            char ch1;
            if ((CharCode < -32768) || (CharCode > 0xffff))
            {
                throw new ArgumentException("Argument_Range Two Bytes ", "CharCode");
            }
            if ((CharCode >= 0) && (CharCode <= 0x7f))
            {
                return Convert.ToChar(CharCode);
            }
            try
            {
                int num1;
                Encoding encoding1 = Encoding.GetEncoding(Types.GetLocaleCodePage());
                if ((encoding1.GetMaxByteCount(1) == 1) && ((CharCode < 0) || (CharCode > 0xff)))
                {
                    throw new Exception("Error Encoding");
                }
                char[] chArray1 = new char[2];
                byte[] buffer1 = new byte[2];
                Decoder decoder1 = encoding1.GetDecoder();
                if ((CharCode >= 0) && (CharCode <= 0xff))
                {
                    buffer1[0] = (byte)(CharCode & 0xff);
                    num1 = decoder1.GetChars(buffer1, 0, 1, chArray1, 0);
                }
                else
                {
                    buffer1[0] = (byte)((CharCode & 0xff00) / 0x100);
                    buffer1[1] = (byte)(CharCode & 0xff);
                    num1 = decoder1.GetChars(buffer1, 0, 2, chArray1, 0);
                }
                ch1 = chArray1[0];
            }
            catch (Exception exception1)
            {
                throw exception1;
            }
            return ch1;
        }

        public static char ChrW(int CharCode)
        {
            if ((CharCode < -32768) || (CharCode > 0xffff))
            {
                throw new ArgumentException("Argument_Range Two Bytes", "CharCode");
            }
            return Convert.ToChar((int)(CharCode & 0xffff));
        }

        public static int StrCmp(string sLeft, string sRight, bool TextCompare)
        {
            if (sLeft == null)
            {
                sLeft = "";
            }
            if (sRight == null)
            {
                sRight = "";
            }
            if (TextCompare)
            {
                return Types.GetCultureInfo().CompareInfo.Compare(sLeft, sRight, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
            }
            return string.CompareOrdinal(sLeft, sRight);
        }


        /// <summary>
        /// Split string to segements by maxLength per segement
        /// </summary>
        /// <param name="s"></param>
        /// <param name="limt"></param>
        /// <param name="maxLengthPerSigment"></param>
        /// <returns></returns>
        public static string[] SplitString(string s, int limt, int maxLengthPerSigment)
        {
            return SplitString(s, limt, maxLengthPerSigment, null);
        }

        /// <summary>
        /// Split string to segements by sperator and maxLength per segement
        /// </summary>
        /// <param name="s"></param>
        /// <param name="limt"></param>
        /// <param name="maxLengthPerSigment"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string[] SplitString(string s, int limt, int maxLengthPerSigment, string separator)
        {
            string source = s;//"1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890";// 1234567890 1234567890 1234567890 ";
            //int sigment = 1;
            List<string> strResult = new List<string>();
            //int limt=70;
            //int maxCharPerSigment=65;
            int length = source.Length;
            if (length <= maxLengthPerSigment)
            {
                return new string[] { s };
            }

            if (length > limt)
            {
                //sigment = (int)Math.Ceiling((float)length / (float)maxCharPerSigment);
                source = source.Substring(0, limt);//
                length = source.Length;
            }

            //strResult = new string[sigment];
            int current = 0;
            int currentIndex = 0;
            int currentStartIndex = 0;
            int currentLength = 0;
            //int currentCharPerSigment = maxLengthPerSigment;

            do
            {
                currentStartIndex += maxLengthPerSigment;// currentCharPerSigment;
                if (currentStartIndex > length)
                {
                    currentStartIndex = length;
                    currentIndex = length;
                }
                else if (separator != null)
                {
                    currentIndex = source.LastIndexOf(separator, currentStartIndex);
                    if (currentIndex > -1) //currentIndex++;
                        currentStartIndex = currentIndex;
                }
                else
                {
                    currentIndex = currentStartIndex;
                }

                if (currentIndex == -1)
                {
                    currentLength = currentIndex - current;
                    strResult.Add(source.Substring(current, length - current));
                    break;
                }
                else
                {
                    currentLength = currentIndex - current;
                    strResult.Add(source.Substring(current, currentLength));
                }
                current += (currentLength);
                //currentCharPerSigment += currentIndex;
            } while (currentStartIndex < length);

            return strResult.ToArray();

        }

        public static List<string> SplitToList(string s, char spliter)
        {
            if (string.IsNullOrEmpty(s))
                return null;

            List<string> list = new List<string>();
            return s.Split(spliter).ToList<string>();

        }

        public static string ArrayToString(string[] array, char spliter, bool trimEnd)
        {
            if (array == null || array.Length == 0)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            foreach (string s in array)
            {
                sb.AppendFormat("{0}{1}", s, spliter);
            }
            if (trimEnd)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        public static string IntTimeToString(int time)
        {
            string t = time.ToString();
            t = t.Insert(t.Length - 2, ":");
            return t;
            //return string.Format("{0}:{1}", t.Substring(0, 2), t.Substring(2, 2));
        }
        public static int StringTimeToInt(string time)
        {
            string t = time.Replace(":", "").TrimStart('0');
            return Convert.ToInt32(t);
        }
        public static TimeSpan StringTimeToTimeSpan(string time)
        {
            string[] t = time.Split(':');
            return new TimeSpan(Convert.ToInt32(t[0]), Convert.ToInt32(t[1]), 0);
        }

        public static string Trim(string value, int length = 0)
        {
            if (value == null)
                return null;
            value = value.Trim();

            if (length > 0 && value.Length > length)
                return value.Substring(0, length);

            return value;
        }

        public static string[] SplitTrim(string s, params char[] spliter)
        {
            if (s == null || spliter == null)
            {
                throw new ArgumentNullException();
            }
            string[] array = s.Split(spliter);
            foreach (string a in array)
            {
                a.Trim();
            }
            return array;
        }

        public static string[] SplitTrim(string s, params string[] spliter)
        {
            if (s == null || spliter == null)
            {
                throw new ArgumentNullException();
            }
            string[] array = s.Split(spliter, StringSplitOptions.None);
            foreach (string a in array)
            {
                a.Trim();
            }
            return array;
        }

        public static string[] SplitTrim(string str, string defaultValue, char splitter = ',')
        {
            string[] list = string.IsNullOrEmpty(str) ? new string[] { defaultValue } : SplitTrim(str, splitter);
            return list;
        }

        public static string[] ReTrim(string[] str, string defaultValue = "")
        {
            if (str == null)
            {
                throw new ArgumentNullException("ReTrim.str");
            }
            for (int i = 0; i < str.Length; i++)
            {
                str[i] = string.IsNullOrEmpty(str[i]) ? defaultValue : str[i].Trim();
            }
            return str;
        }

        public static string JoinTrim(string[] str, string splitter = ",")
        {
            if (str == null)
            {
                throw new ArgumentNullException("JoinTrim.str");
            }

            return string.Join(splitter, ReTrim(str));
        }

        public static bool IsWhiteSpace(string s)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            if (s.Length == 0)
                return false;

            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsWhiteSpace(s[i]))
                    return false;
            }

            return true;
        }

        public static string NullEmpty(string s)
        {
            return (string.IsNullOrEmpty(s)) ? null : s;
        }
        public static int Length(string value)
        {
            if (value == null)
                return 0;
            else
                return value.Length;
        }

        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (!char.IsUpper(s[0]))
                return s;

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                    break;

                chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
            }

            return new string(chars);
        }

    }
    
    #endregion

 	#region Types
	
	public static class Types //Types
	{
        public static DateTime MinDate { get { return new DateTime(1900, 1, 1); } }
		
        public static bool IsNull(object value, int minLengrg = 0)
        {
            return (value == null || value == DBNull.Value || value.ToString().Length < minLengrg) ? true : false;
        }

        /// <summary>
        /// IsEmpty object|string|Guid or number==0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmpty(object value)
        {

            if (value == null || value == DBNull.Value)
                return true;
            Type type = value.GetType();

            if (type == typeof(string))
                return string.IsNullOrEmpty(value.ToString());
            if (type == typeof(Guid))
                return Guid.Empty == new Guid(value.ToString());
            if (type == typeof(int))
                return Types.ToInt(value) == 0;
            if (type == typeof(long))
                return Types.ToLong(value) == 0;
            if (type  == typeof(decimal))
                return Types.ToDecimal(value, 0) == 0;
            if (type == typeof(float))
                return Types.ToFloat(value, 0) == 0;
            if (type == typeof(double))
                return Types.ToDouble(value, 0) == 0;
            return false;
        }

        public static string NzOr(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
                return b;
            return a;
        }

        public static string NzOr(object a, object b)
        {
            if (a == null && b == null)
                return null;
            if (a == null)
                return b.ToString();
            if (b == null)
                return a.ToString();
            return NzOr(a.ToString(), b.ToString());
        }

        /// <summary>
        /// Get indicate wether the value is null or empty, if yes return the given valueIfNull argument.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public static string NZorEmpty(object value, string valueIfNull)
        {
            try
            {
                return (value == null || value == DBNull.Value || value.ToString() == String.Empty) ? valueIfNull : value.ToString();
            }
            catch
            {
                return valueIfNull;
            }
        }
        public static bool IsDbNull(object value)
        {
            return (value == null || value == DBNull.Value);
        }

		#region NZ


        public static object ChangeType(object value, Type type)
        {
            try
            {

                if (type.IsNullableType())
                {
                    Type underLineType = Nullable.GetUnderlyingType(type) ?? type;

                    //Coalesce to set the safe value using default(t) or the safe type.
                    if (value == null || value.ToString() == "")
                        return GenericTypes.Default(type);
                    else if (underLineType == typeof(bool))
                        return StringToBool(value.ToString(), false);
                    return Convert.ChangeType(value, underLineType);
                }
                if (IsNumericType(type))
                {
                    return IsNumeric(value) == false ? GenericTypes.Default(type) : Convert.ChangeType(value, type);
                }
                if (type == typeof(bool))
                {
                    return StringToBool(value.ToString(), false);
                }
                if (type == typeof(DateTime))
                {
                    return ToDateTime(value);//, MinDate);
                }
                if (type == typeof(Guid))
                {
                    return ToGuid(value);
                }

                return (value == null || value == DBNull.Value) ? GenericTypes.Default(type) : Convert.ChangeType(value, type);
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                if (type == typeof(DateTime))
                    return MinDate;
                return GenericTypes.Default(type);
            }
        }

        public static object NZ(object value)
        {
            try
            {
                return (value == null || value == DBNull.Value) ? null : (object)value;
            }
            catch
            {
                return null;
            }
        }
 
		public static object NZ(object value,object valueIfNull)
		{
			try
			{
				return (value ==null || value == DBNull.Value) ? valueIfNull:(object)value;
			}
			catch
			{
				return valueIfNull;
			}
		}

		public static int NZ(object value,int valueIfNull)
		{
			try
			{
				return (value ==null || value == DBNull.Value) ? valueIfNull:ToInt(value.ToString(), NumberStyles.Number,valueIfNull);
			}
			catch
			{
				return valueIfNull;
			}
		}

        
        public static float NZ(object value, float valueIfNull)
        {
            try
            {
                return (value == null || value == DBNull.Value) ? valueIfNull : ToFloat(value.ToString(), NumberStyles.Number, valueIfNull);
            }
            catch
            {
                return valueIfNull;
            }
        }

		public static double NZ(object value,double valueIfNull)
		{
			try
			{
                return (value == null || value == DBNull.Value) ? valueIfNull : ToDouble(value.ToString(), NumberStyles.Number, valueIfNull);
			}
			catch
			{
				return valueIfNull;
			}
		}

		public static decimal NZ(object value,decimal valueIfNull)
		{
			try
			{
				return (value ==null || value == DBNull.Value) ? valueIfNull: ToDecimal(value.ToString(), NumberStyles.Number,valueIfNull);
			}
			catch
			{
				return valueIfNull;
			}
		}
        
		
        public static DateTime NZ(object value,DateTime valueIfNull)
		{
			try
			{
				return (value ==null || value == DBNull.Value) ? valueIfNull:ToDateTime(value.ToString(),valueIfNull);
			}
			catch
			{
				return valueIfNull;
			}
		}

		public static string NZ(object value,string valueIfNull)
		{
			try
			{
				return (value ==null || value == DBNull.Value) ? valueIfNull:value.ToString();
			}
			catch
			{
				return valueIfNull;
			}
		}
		public static bool NZ(object value,bool valueIfNull)
		{
			try
			{
                return (value == null || value == DBNull.Value) ? valueIfNull : StringToBool(value.ToString(),valueIfNull);
			}
			catch
			{
				return valueIfNull;
			}
		}

		#endregion

		#region Types Number Methods

	
		public static double Fix(double Number)
		{
			if (Number >= 0)
			{
				return Math.Floor(Number);
			}
			return -Math.Floor(-Number);
		}

		public static string GetOnlyDigits(string sourceValue)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char localChar in sourceValue)
			{
				if (Char.IsDigit(localChar)) sb.Append(localChar);
			}
			return sb.ToString();
		}

		public static int GetNextDozen(int value)
		{
			int mod = 0, auxVal = value;
			do
			{
				mod = auxVal%10;
				if (mod != 0) auxVal++;
			}
			while(mod != 0);

			return auxVal;
		}

        public static Guid ToGuid(object value)
        {
            Guid g = Guid.Empty;

            if (value == null || value== DBNull.Value)
                return g;
            Guid.TryParse(value.ToString(), out g);
            return g;
        }

        public static Guid ToGuid(object value, Guid defaultValue)
        {
            Guid g = defaultValue;

            if (value == null)
                return g;
            Guid.TryParse(value.ToString(), out g);
            return g;
        }

  
        public static double ToDouble(object value, double defaultValue)
        {
            if (value == null)
                return defaultValue;
            double val;//0;
            if (Double.TryParse(value.ToString(), NumberStyles.Number, null, out val))
                return val;
            return defaultValue;
        }
        public static decimal ToDecimal(object value, decimal defaultValue)
        {
            if (value == null)
                return defaultValue;
            decimal val;//0;
            if (Decimal.TryParse(value.ToString(), NumberStyles.Number, null, out val))
                return val;
            return defaultValue;
        }

        public static int ToInt(object value)
        {
            if (value == null)
                return 0;
            int val;//0;
            if (int.TryParse(value.ToString(), out val))
                return val;
            return 0;
        }
        public static int ToInt(object value, int defaultValue)
        {
            if (value == null)
                return defaultValue;
            int val;//0;
            if (int.TryParse(value.ToString(), out val))
                return val;
            return defaultValue;
        }

        public static long ToLong(object value)
        {
            if (value == null)
                return 0;
            long val;//0;
            if (long.TryParse(value.ToString(), out val))
                return val;
            return 0;
        }
        public static long ToLong(object value, long defaultValue)
        {
            if (value == null)
                return defaultValue;
            long val;//0;
            if (long.TryParse(value.ToString(), out val))
                return val;
            return defaultValue;
        }

        public static byte ToByte(object value, byte defaultValue)
        {
            if (value == null)
                return defaultValue;
            byte val;//0;
            if (byte.TryParse(value.ToString(),  out val))
                return val;
            return defaultValue;
        }

        public static float ToFloat(object value, float defaultValue)
        {
            if (value == null)
                return defaultValue;
            float val;//0;
            if (float.TryParse(value.ToString(), out val))
                return val;
            return defaultValue;
        }

        public static double ToDouble(object value, NumberStyles style, double defaultValue)
		{
			if(value==null)
				return defaultValue;
            double val;//0;
            if (Double.TryParse(value.ToString(), style, null, out val))
                return val;
            return defaultValue;
		}

        public static decimal ToDecimal(object value, NumberStyles style, decimal defaultValue)
		{
			if(value==null)
				return defaultValue;
            decimal val;//0;
            if (Decimal.TryParse(value.ToString(), style, null, out val))
                return val;
            return defaultValue;
		}
        public static int ToInt(object value, NumberStyles style, int defaultValue)
		{
			if(value==null)
				return defaultValue;
            int val;//0;
            if (int.TryParse(value.ToString(), style, null, out val))
                return val;
            return defaultValue;
    	}

		public static bool ToBool(object value,bool defaultValue)
		{
			if(value==null)
				return defaultValue;
			return StringToBool(value.ToString(),defaultValue);
		}

        public static byte ToByte(object value, NumberStyles style, byte defaultValue)
        {
            if (value == null)
                return defaultValue;
             byte val;//0;
            if (byte.TryParse(value.ToString(),style,null, out val))
                return val;
            return defaultValue;
        }

        public static float ToFloat(object value, NumberStyles style, float defaultValue)
        {
            if (value == null)
                return defaultValue;
            float val;// 0F;
            if (float.TryParse(value.ToString(), style, null, out val))
                return val;
            return defaultValue;
        }

		private static double DecimalToDouble(IConvertible ValueInterface)
		{
			return Convert.ToDouble(ValueInterface.ToDecimal(null));
		}


		internal static NumberFormatInfo GetNormalizedNumberFormat(NumberFormatInfo InNumberFormat)
		{
			NumberFormatInfo info2;
			NumberFormatInfo info5 = InNumberFormat;
			if (((((info5.CurrencyDecimalSeparator != null) && (info5.NumberDecimalSeparator != null)) && ((info5.CurrencyGroupSeparator != null) && (info5.NumberGroupSeparator != null))) && (((info5.CurrencyDecimalSeparator.Length == 1) && (info5.NumberDecimalSeparator.Length == 1)) && ((info5.CurrencyGroupSeparator.Length == 1) && (info5.NumberGroupSeparator.Length == 1)))) && (((info5.CurrencyDecimalSeparator[0] == info5.NumberDecimalSeparator[0]) && (info5.CurrencyGroupSeparator[0] == info5.NumberGroupSeparator[0])) && (info5.CurrencyDecimalDigits == info5.NumberDecimalDigits)))
			{
				return InNumberFormat;
			}
			info5 = null;
			NumberFormatInfo info4 = InNumberFormat;
			if ((((info4.CurrencyDecimalSeparator != null) && (info4.NumberDecimalSeparator != null)) && ((info4.CurrencyDecimalSeparator.Length == info4.NumberDecimalSeparator.Length) && (info4.CurrencyGroupSeparator != null))) && ((info4.NumberGroupSeparator != null) && (info4.CurrencyGroupSeparator.Length == info4.NumberGroupSeparator.Length)))
			{
				int num3 = info4.CurrencyDecimalSeparator.Length - 1;
				int num1 = 0;
				while (num1 <= num3)
				{
					if (info4.CurrencyDecimalSeparator[num1] != info4.NumberDecimalSeparator[num1])
					{
						goto Label_019D;
					}
					num1++;
				}
				int num2 = info4.CurrencyGroupSeparator.Length - 1;
				for (num1 = 0; num1 <= num2; num1++)
				{
					if (info4.CurrencyGroupSeparator[num1] != info4.NumberGroupSeparator[num1])
					{
						goto Label_019D;
					}
				}
				return InNumberFormat;
			}
			info4 = null;
			Label_019D:
				info2 = (NumberFormatInfo) InNumberFormat.Clone();
			NumberFormatInfo info3 = info2;
			info3.CurrencyDecimalSeparator = info3.NumberDecimalSeparator;
			info3.CurrencyGroupSeparator = info3.NumberGroupSeparator;
			info3.CurrencyDecimalDigits = info3.NumberDecimalDigits;
			info3 = null;
			return info2;
		}

			#endregion

		#region Boolean

		public static bool StringToBool(string value,bool defaultValue)
		{
            bool val = defaultValue;
			try
			{
                if (value == "0" || value=="off")
                    return false;
                if (value == "1" || value=="on")
                    return true;
                if(Boolean.TryParse(value, out val))
                    return Boolean.Parse(value);
                return defaultValue;
			}
			catch
			{
				return defaultValue;
			}
		}

        public static int BoolToInt(object value, bool defaultValue)
        {
            return BoolToInt(ToBool(value,defaultValue));
        }

        public static int BoolToInt(bool value)
        {
            return value ? 1 : 0;
        }
		#endregion

		#region Types Percent Methods

			public static bool IsPercentString(string val, System.IFormatProvider provider )
			{
				System.Globalization.NumberFormatInfo l_Info;
				if (provider==null)
					l_Info = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
				else
					l_Info = (System.Globalization.NumberFormatInfo)provider.GetFormat(typeof(System.Globalization.NumberFormatInfo));

				if (val.IndexOf(l_Info.PercentSymbol) == -1)
					return false;
				else
					return true;
			}

			public static double StringToPercentDouble(string val,
				System.Globalization.NumberStyles style , 
				System.IFormatProvider provider,
				bool p_ConsiderAllStringAsPercent)
			{
				bool l_IsPercentString = IsPercentString(val,provider);
				if (l_IsPercentString)
				{
					return double.Parse(val.Replace("%",""),style,provider) / 100.0;
				}
				else
				{
					if (p_ConsiderAllStringAsPercent)
						return double.Parse(val,style,provider) / 100.0;
					else
						return double.Parse(val,style,provider);
				}
			}
			public static float StringToPercentFloat(string val,
				System.Globalization.NumberStyles style , 
				System.IFormatProvider provider ,
				bool p_ConsiderAllStringAsPercent)
			{
				bool l_IsPercentString = IsPercentString(val,provider);
				if (l_IsPercentString)
				{
					return float.Parse(val.Replace("%",""),style,provider) / 100;
				}
				else
				{
					if (p_ConsiderAllStringAsPercent)
						return float.Parse(val,style,provider) / 100;
					else
						return float.Parse(val,style,provider);
				}
			}
			public static decimal StringToPercentDecimal(string val,
				System.Globalization.NumberStyles style , 
				System.IFormatProvider provider,
				bool p_ConsiderAllStringAsPercent )
			{
				bool l_IsPercentString = IsPercentString(val,provider);
				if (l_IsPercentString)
				{
					return decimal.Parse(val.Replace("%",""),style,provider) / 100.0M;
				}
				else
				{
					if (p_ConsiderAllStringAsPercent)
						return decimal.Parse(val,style,provider) / 100.0M;
					else
						return decimal.Parse(val,style,provider);
				}
			}


			#endregion

		#region DateTime
		
		private static Calendar CurrentCalendar
		{
			get
			{
				return Thread.CurrentThread.CurrentCulture.Calendar;
			}
		}


		public static DateTime DateFromString(string Value)
		{
            return ToDateTime(Value, Types.GetCultureInfo());
		}

        public static DateTime ToDateTime(object Value)
        {
            return ToDateTime(Value, DateTime.Now);
        }

        public static DateTime ToDateTime(object Value, DateTime defaultValue)
        {
            if (Value == null || Value==DBNull.Value)
                return defaultValue;
            return ToDateTime(Value.ToString(),defaultValue);
        }

        public static DateTime ToDateTime(string Value, DateTime defaultValue)
        {
            DateTime val;
            if (DateTime.TryParse(Value, out val))
                return val;
            return defaultValue;
        }

        public static DateTime ToDateTime(string Value,DateTimeFormatInfo dtf, DateTime defaultValue)
        {
            DateTime val;
            if (DateTime.TryParse(Value, dtf, DateTimeStyles.None, out val))
                return val;
            return defaultValue;
        }

        public static DateTime ToDateTime(string Value, CultureInfo culture, DateTime defaultValue)
        {
            DateTime val;
            if (DateTime.TryParse(Value, culture, DateTimeStyles.None, out val))
                return val;
            return defaultValue;
        }

        public static DateTime ToDateTime(string Value, DateTimeFormatInfo dtf)
		{
            return ToDateTime(Value, dtf, DateTime.Now);
		}

        public static DateTime ToDateTime(string Value, CultureInfo culture)
		{
            return ToDateTime(Value, culture, DateTime.Now);
		}

		/// <summary>
		/// Get string date and return formated string date ,on exception return defult value
		/// </summary>
		/// <param name="s">string dateTime</param>
		/// <param name="format">format</param>
		/// <param name="defaultValue">value to return if invalid cast exception</param>
		/// <returns>String Date Time formated</returns>
		public static string  FormatDate(string s,string format, DateTime defaultValue)
		{
			try
			{
                return ToDateTime(s, defaultValue).ToString(format);
			}
			catch(Exception)
			{
				return defaultValue.ToString(format);
			}
		}

		/// <summary>
		/// Get string date and return formated string date ,on exception return defult value
		/// </summary>
		/// <param name="s">string dateTime</param>
		/// <param name="format">format</param>
		/// <param name="defaultValue">string value to return if invalid cast exception</param>
		/// <returns>String Date Time formated</returns>
		public static string FormatDate(string s,string format, string defaultValue)
		{
            DateTime val;
            try
			{
                if (string.IsNullOrEmpty(s))
                    return defaultValue;
                if (DateTime.TryParse(s, out val))
                    return val.ToString(format);
                return defaultValue;
			}
			catch(Exception)
			{
				return defaultValue;
			}
		}

        //public static bool IsDate(string s)
        //{
        //    DateTime val;
        //    if (string.IsNullOrEmpty(s) || s.Length != 10)
        //        return false;
        //    return DateTime.TryParse(s, out val);
        //}

        public static bool IsDate(string s)
        {
            return IsDate(s, CultureInfo.CurrentCulture.Name);
        }
        public static bool IsDate(string s, string cultureName)
        {
            DateTime val;
            if (string.IsNullOrEmpty(s) || s.Length != 10)
                return false;
            return DateTime.TryParse(s, DateFormatInfo(cultureName), System.Globalization.DateTimeStyles.AssumeLocal, out val);
        }

        public static System.Globalization.DateTimeFormatInfo DateFormatInfo(string cultureName)//"he-IL"
        {
            return new CultureInfo(cultureName, false).DateTimeFormat; 
        }

        public static bool IsDateTime(string s)
        {
            return IsDateTime(s, CultureInfo.CurrentCulture.Name);
        }
        public static bool IsDateTime(string s, string cultureName)
        {
            DateTime val;
            if (string.IsNullOrEmpty(s) || s.Length < 10)
                return false;
            //return DateTime.TryParse(s, out val);
            return DateTime.TryParse(s, DateFormatInfo(cultureName), System.Globalization.DateTimeStyles.None, out val);
        }

        public static bool IsDateTime(object o)
        {
            if (o == null)
                return false;
            if (o is DateTime)
                return true;
            return IsDateTime(o.ToString());
        }

        public static int DateTimeToInt(DateTime value)
        {
            return int.Parse(value.ToString("yyyyMMdd"));
        }

        public static int TodayInt()
        {
            return int.Parse(DateTime.Today.ToString("yyyyMMdd"));
        }


		public static string DateToString(DateTime value,string format)
		{
			return value.ToString(format, CultureInfo.CurrentCulture);
		}

		public static string DateToString(DateTime value)
		{
			string sep = System.Globalization.DateTimeFormatInfo.CurrentInfo.DateSeparator;
			string format = String.Empty;
		
			DateTime t = new DateTime(2000,2,1);
			string   tStr  = t.ToShortDateString();					
			if(tStr.IndexOf("1") > tStr.IndexOf("2"))
			{
				format = "MM" + sep + "dd" + sep + "yyyy";
			}
			else
			{
				format = "dd" + sep + "MM" + sep + "yyyy";
			}
		
			return value.ToString(format, CultureInfo.CurrentCulture);
		}
		
		public static DateTime DateValue(string StringDate)
		{
			DateTime time2 = DateFromString(StringDate);
			return time2.Date;
		}


 
        public static DateTime DateStart(DateTime d)
        {
            return new DateTime(d.Year,d.Month,d.Day,0,0,0);
        }
        public static DateTime DateEnd(DateTime d)
        {
            return new DateTime(d.Year, d.Month, d.Day, 23, 59, 59);
        }

        #endregion

		#region Bytes

		public static string BytesToHexString(byte[] byteArray)
		{
			StringBuilder sb = new StringBuilder(40);
			foreach(byte bValue in byteArray)
			{
				sb.AppendFormat(CultureInfo.CurrentCulture, bValue.ToString("x2", CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture));
			}
			return sb.ToString();
		}


		public static byte[] BytesFromString(string stringValue)
		{
			return (new UnicodeEncoding()).GetBytes(stringValue);
		}

		public static string BytesToString(byte[] byteArray)
		{
			return (new UnicodeEncoding()).GetString(byteArray);
		}
	
		#endregion

		#region Formats

		public struct BoolRange
		{
			public object T;
			public object F;
		}


		#endregion

        #region enum

        public static object ParseEnum(Type type, object value, object defaultValue)
        {
            try
            {
                if (value == null)
                    return defaultValue;
                if (!Enum.IsDefined(type, value))
                    return defaultValue;
                return Enum.Parse(type, value.ToString(), true);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int ParseEnum(Type type, object value, int defaultValue)
        {
            try
            {
                if (value == null)
                    return defaultValue;
                if (!Enum.IsDefined(type, value))
                    return defaultValue;
                return (int)Enum.Parse(type, value.ToString(), true);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T ToEnum<T>(object value, T defaultValue)
        {
            try
            {
                if (value == null)
                    return defaultValue;
                if (!Enum.IsDefined(typeof(T), value))
                    return defaultValue;
                return (T)Enum.Parse(typeof(T), value.ToString(), true);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        ///  Converts the string representation of the name or numeric value of one or
        ///  more enumerated constants to an equivalent enumerated object. A string parameter
        ///  is not case-insensitive.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static T ToEnum<T>(object value)
        {
            if (value == null)
                throw new ArgumentNullException("Enum.Parse value");
            if (!Enum.IsDefined(typeof(T), value))
                throw new ArgumentException("Enum not defined");
            return (T)Enum.Parse(typeof(T), value.ToString(), true);
        }

        #endregion

        #region Info


        private static System.Globalization.CultureInfo GetCulture(System.Globalization.CultureInfo cultureInfo)
            {
                if (cultureInfo == null)
                    return System.Globalization.CultureInfo.CurrentCulture;
                else
                    return cultureInfo;
            }

            public static CultureInfo GetCultureInfo()
            {
                return Thread.CurrentThread.CurrentCulture;
            }

            public static DateTimeFormatInfo GetDateTimeFormatInfo()
            {
                return Thread.CurrentThread.CurrentCulture.DateTimeFormat;
            }

            internal static Encoding GetFileIOEncoding()
            {
                return Encoding.Default;
            }

            internal static int GetLocaleCodePage()
            {
                return Thread.CurrentThread.CurrentCulture.TextInfo.ANSICodePage;
            }

            public static bool IsGuid(string candidate, out Guid output)
            {
                Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

                bool isValid = false;

                output = Guid.Empty;

                if (candidate != null)
                {
                    if (isGuid.IsMatch(candidate))
                    {
                        output = new Guid(candidate);
                        isValid = true;
                    }
                }
                return isValid;
            }

            public static bool IsGuid(string guid)
            {
                Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

                bool isValid = false;

                if (guid != null)
                {
                    if (isGuid.IsMatch(guid))
                    {
                        isValid = true;
                    }
                }
                return isValid;
            }


            /// <summary>
            /// IsValidTime
            /// </summary>
            /// <param name="time">12:30</param>
            /// <returns></returns>
            public static bool IsValidTime(string time)
            {
                if (string.IsNullOrEmpty(time))
                    return false;
                string[] args = time.Split(':');
                if (args == null || args.Length != 2)
                    return false;
                int h = Nistec.Types.ToInt(args[0], -1);
                int m = Nistec.Types.ToInt(args[1], -1);
                if (h < 0 || m < 0)
                    return false;
                return (h >= 0 && h <= 23 && m >= 0 && m <= 59);

            }
            public static bool IsValidTime(string time, ref int[] value)
            {
                if (string.IsNullOrEmpty(time))
                    return false;
                string[] args = time.Split(':');
                if (args == null || args.Length != 2)
                    return false;
                int h = Nistec.Types.ToInt(args[0], -1);
                int m = Nistec.Types.ToInt(args[1], -1);
                if (h < 0 || m < 0)
                    return false;
                if (h >= 0 && h <= 23 && m >= 0 && m <= 59)
                {
                    value = new int[] { h, m };
                    return true;
                }
                return false;

            }
            /// <summary>
            /// IsValidMonthDay
            /// </summary>
            /// <param name="date">22/11</param>
            /// <returns></returns>
            public static bool IsValidMonthDay(string date, ref int[] value)
            {
                if (string.IsNullOrEmpty(date))
                    return false;
                string[] args = date.Split('/');
                if (args == null || args.Length != 2)
                    return false;
                int d = Nistec.Types.ToInt(args[0], -1);
                int m = Nistec.Types.ToInt(args[1], -1);
                if (m < 0 || d < 0)
                    return false;
                if (m >= 0 && m <= 12 && d >= 0 && d <= 31)
                {
                    value = new int[] { d, m };
                    return true;
                }
                return false;
            }

            public static bool? IsTrue(object value, bool? nullValue = null)
            {
                if (value == null)
                    return nullValue;
                switch (value.ToString().ToLower())
                {
                    case "1":
                    case "on":
                    case "true":
                        return true;
                    case "0":
                    case "off":
                    case "false":
                        return false;
                    default:
                        return nullValue;
                }
            }

            
            public static bool IsBool(object Expression)
            {
                if (Expression != null)
                {
                    if (Expression is bool)
                    {
                        return true;
                    }
                    if (Expression is string)
                    {
                        string val = (string)Expression;
                        if (!(val.ToLower().Contains("false") || val.ToLower().Contains("true")))
                        {
                            return false;
                        }
                        try
                        {
                            bool res = bool.Parse((string)Expression);
                            return true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                return false;
            }
            public static bool IsDBNull(object Expression)
            {
                if ((Expression != null) && (Expression == DBNull.Value))
                {
                    return true;
                }
                return false;
            }

            public static Boolean IsNumeric(object input)
            {
                if (input == null || input == DBNull.Value)
                    return false;
                Double temp;
                return Double.TryParse(input.ToString(), out temp);
            }
            public static Boolean IsNumeric(String input)
            {
                Double temp;
                return Double.TryParse(input, out temp);
            }

            public static Boolean IsNumeric(String input, NumberStyles numberStyle)
            {
                Double temp;
                return Double.TryParse(input, numberStyle, CultureInfo.CurrentCulture, out temp);
            }

            internal static bool IsNumericTypeCode(TypeCode TypCode)
            {
                switch (TypCode)
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        return true;
                }
                return false;
            }

            public static bool IsNumber(string s)
            {
                try
                {
                    double result = 0;
                    return Double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.CurrentInfo, out result);
                }
                catch
                {
                    return false;
                }
            }
            public static bool IsNumber(string s, System.Globalization.NumberStyles style)
            {
                try
                {
                    double result = 0;
                    return Double.TryParse(s, style, System.Globalization.NumberFormatInfo.CurrentInfo, out result);
                }
                catch
                {
                    return false;
                }
            }

            public static bool IsNumber(object obj)
            {
                if (obj == null)
                    return false;
                return IsNumber(obj.ToString());
            }

        public static bool IsNumericType(Type type)
        {

            if (type == typeof(float)
                || type == typeof(double)
                || type == typeof(byte)
                || type == typeof(short)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(decimal)
                || type == typeof(Int16)
                || type == typeof(Int32)
                || type == typeof(Int64)
                || type == typeof(UInt16)
                || type == typeof(UInt32)
                || type == typeof(UInt64)
                || type == typeof(uint))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region Nullable

        public static DateTime? ToNullableDate(object value, string culture = "he-IL")
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
                return (DateTime?)null;
            DateTime val;
            if (DateTime.TryParse(value.ToString(), new CultureInfo(culture, false).DateTimeFormat, DateTimeStyles.AssumeLocal, out val))
                return (DateTime?)val;
            return (DateTime?)null;
        }
        public static int? ToNullableInt(object value)
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
                return (int?)null;
            int val;
            if (int.TryParse(value.ToString(), out val))
                return (int?)val;
            return (int?)null;
        }

            internal static Type GetNonNullableType(this Type type)
            {
                if (IsNullableType(type))
                {
                    return type.GetGenericArguments()[0];
                }
                return type;
            }

            internal static Type GetNullableType(Type type)
            {
                if (type.IsValueType && !IsNullableType(type))
                {
                    return typeof(Nullable<>).MakeGenericType(type);
                }
                return type;
            }

            internal static bool IsNullableType(this Type type)
            {
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
        
        #endregion

            #region Is

            internal static bool IsBool(Type type)
            {
                return GetNonNullableType(type) == typeof(bool);
            }

            internal static bool IsNumeric(Type type)
            {
                type = GetNonNullableType(type);
                if (!type.IsEnum)
                {
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Double:
                        case TypeCode.Single:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return true;
                    }
                }
                return false;
            }

            internal static bool IsInteger(Type type)
            {
                type = GetNonNullableType(type);
                if (type.IsEnum)
                {
                    return false;
                }
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                    default:
                        return false;
                }
            }


            internal static bool IsArithmetic(Type type)
            {
                type = GetNonNullableType(type);
                if (!type.IsEnum)
                {
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Double:
                        case TypeCode.Single:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return true;
                    }
                }
                return false;
            }

            internal static bool IsUnsignedInt(Type type)
            {
                type = GetNonNullableType(type);
                if (!type.IsEnum)
                {
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return true;
                    }
                }
                return false;
            }

            internal static bool IsIntegerOrBool(Type type)
            {
                type = GetNonNullableType(type);
                if (!type.IsEnum)
                {
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Int64:
                        case TypeCode.Int32:
                        case TypeCode.Int16:
                        case TypeCode.UInt64:
                        case TypeCode.UInt32:
                        case TypeCode.UInt16:
                        case TypeCode.Boolean:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                            return true;
                    }
                }
                return false;
            }

            private static bool IsDelegate(Type t)
            {
                return t.IsSubclassOf(typeof(System.MulticastDelegate));
            }

            internal static bool IsConvertible(Type type)
            {
                type = GetNonNullableType(type);
                if (type.IsEnum)
                {
                    return true;
                }
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Char:
                        return true;
                    default:
                        return false;
                }
            }
            internal static Type FindGenericType(Type definition, Type type)
            {
                while (type != null && type != typeof(object))
                {
                    if (type.IsGenericType && AreEquivalent(type.GetGenericTypeDefinition(), definition))
                    {
                        return type;
                    }
                    if (definition.IsInterface)
                    {
                        foreach (Type itype in type.GetInterfaces())
                        {
                            Type found = FindGenericType(definition, itype);
                            if (found != null)
                                return found;
                        }
                    }
                    type = type.BaseType;
                }
                return null;
            }

            internal static bool IsUnsigned(Type type)
            {
                type = GetNonNullableType(type);
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte:
                    case TypeCode.UInt16:
                    case TypeCode.Char:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                    default:
                        return false;
                }
            }

            internal static bool IsFloatingPoint(Type type)
            {
                type = GetNonNullableType(type);
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                    default:
                        return false;
                }
            }

            internal static bool AreEquivalent(Type t1, Type t2)
            {
                return t1 == t2 || t1.IsEquivalentTo(t2);
            }

            internal static bool AreReferenceAssignable(Type dest, Type src)
            {
                // WARNING: This actually implements "Is this identity assignable and/or reference assignable?"
                if (AreEquivalent(dest, src))
                {
                    return true;
                }
                if (!dest.IsValueType && !src.IsValueType && dest.IsAssignableFrom(src))
                {
                    return true;
                }
                return false;
            }
            #endregion
            public static object StringToObject(string type, string value)
            {
                if (type == null)
                    return value;
                switch (type.ToLower())
                {
                    case "boolean":
                    case "bool":
                        return ToBool(value, false);
                    case "byte":
                    case "sbyte":
                        return ToByte(value, (byte)0);
                    case "int16":
                    case "int32":
                    case "int":
                        return ToInt(value);
                    case "int64":
                    case "long":
                        return ToLong(value);
                    case "uint16":
                    case "uint32":
                    case "uint":
                        return (uint)ToInt(value);
                    case "uint64":
                    case "ulong":
                        return (ulong)ToLong(value);
                    case "single":
                    case "float":
                        return ToFloat(value, 0f);
                    case "double":
                        return ToDouble(value, 0);
                    case "datetime":
                    case "date":
                        return ToDateTime(value);
                    case "decimal":
                        return ToDecimal(value, 0);
                    case "guid":
                        return ToGuid(value);
                    default:
                        return value;
                }
            }
    }
	#endregion
}
