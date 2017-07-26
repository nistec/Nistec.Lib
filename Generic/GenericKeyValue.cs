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
using System.Collections.Specialized;
using System.IO;
using System.Collections;
using Nistec.IO;
using Nistec.Serialization;
using System.Data;
using Nistec.Data;

namespace Nistec.Generic
{

    [Serializable]
    public class GenericKeyValue : GenericKeyValue<object>, ISerialEntity, IKeyValue
    {
      
        #region ctor

        public GenericKeyValue()
        {

        }
        public GenericKeyValue(List<KeyValuePair<string, object>> keyValueList)
        {
            Load(keyValueList);
        }

        public GenericKeyValue(KeyValuePair<string, object>[] keyValueArray)
        {
            this.AddRange(keyValueArray);
        }

        public GenericKeyValue(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                EntityRead(ms, null);
            }
        }

        public GenericKeyValue(object[] keyValue)
        {
            Load(ParseQuery(keyValue));
        }

        public override void Prepare(DataRow dr)
        {
            this.ToKeyValue(dr);
        }
        public static GenericKeyValue Create(params object[] keyValue)
        {
            GenericKeyValue query = new GenericKeyValue(ParseQuery(keyValue));

            return query;
        }


        #endregion

        #region properties

        public TV Get<TV>(string key)
        {
            return GenericTypes.Convert<TV>(this[key]);
        }

        public TV Get<TV>(string key, TV defaultValue)
        {
            return GenericTypes.Convert<TV>(this[key], defaultValue);
        }


        #endregion

        /// <summary>
        /// Get this as sorted <see cref="IOrderedEnumerable<KeyValuePair<string, object>>"./>
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<KeyValuePair<string, object>> Sorted()
        {
            var sortedDict = from entry in this orderby entry.Key ascending select entry;
            return sortedDict;
        }

    }

    [Serializable]
    public class GenericNameValue : GenericKeyValue<string>, ISerialEntity, IKeyValue, IDataRowAdaptor
    {

        #region collection methods

        public virtual void Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("GenericNameValue.Add key");
            }

            base.Add(key, value == null ? null : value.ToString());
        }

        #endregion

        #region ctor

        public GenericNameValue()
        {

        }
        public GenericNameValue(List<KeyValuePair<string, string>> keyValueList)
        {
            Load(keyValueList);
        }

        public GenericNameValue(KeyValuePair<string, string>[] keyValueArray)
        {
            this.AddRange(keyValueArray);
        }

        public GenericNameValue(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                EntityRead(ms, null);
            }
        }

        public GenericNameValue(NetStream stream)
        {
            EntityRead(stream, null);
        }

        public GenericNameValue(string[] keyValue)
        {
            Load(ParseQuery(keyValue));
        }

        //public static GenericNameValue Create(params string[] keyValue)
        //{
        //    var pair = ParseQuery(keyValue);
        //    GenericNameValue query = new GenericNameValue();
        //    query.Load(pair);
        //    return query;
        //}
        
        public override void Prepare(DataRow dr)
        {
            this.ToNameValue(dr);
        }

        public static GenericNameValue Create(params string[] keyValue)
        {
            GenericNameValue pair = new GenericNameValue();
            if (keyValue == null)
                return pair;
            string[] array = null;
            if(keyValue.Length==1)
            {
                array = keyValue[0].Split('|');
            }
            else
            {
                array = keyValue;
            }
            int count = array.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("keyValues parameter is not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                string o = array[i];
                if (o != null)
                {
                    pair.Add(new KeyValuePair<string, string>(array[i].ToString(), array[++i]));
                }
            }
            return pair;
        }
        internal static List<KeyValuePair<string, string>> ParseQuery(params string[] keyValue)
        {
            List<KeyValuePair<string, string>> pair = new List<KeyValuePair<string, string>>();
            if (keyValue == null)
                return pair;
            int count = keyValue.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("keyValues parameter is not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                string o = keyValue[i];
                if (o != null)
                {
                    pair.Add(new KeyValuePair<string, string>(keyValue[i].ToString(), keyValue[++i]));
                }
            }
            return pair;
        }

        #endregion

        #region properties

        public string Get(string key)
        {
            return this[key];
        }
        public TV Get<TV>(string key)
        {
            return GenericTypes.Convert<TV>(this[key]);
        }

        public TV Get<TV>(string key, TV defaultValue)
        {
            return GenericTypes.Convert<TV>(this[key], defaultValue);
        }

        public TV GetEnum<TV>(string key, TV defaultValue)
        {
            return GenericTypes.ConvertEnum<TV>(this[key], defaultValue);
        }


        #endregion

        #region converter
        public bool Contains(string key, string value)
        {
            return this.Exists(p => p.Key == key && p.Value == value);
        }

        public new bool Contains(KeyValuePair<string, string> item)
        {
            return this.Where(p => p.Key == item.Key && p.Value == item.Value).Count() > 0;
        }

        public virtual KeyValuePair<string, string> GetItem(string key, string value)
        {
            return this.Where(p => p.Key == key && p.Value == value).FirstOrDefault();
        }

        public static string JoinArg(string[] str)
        {
            return string.Join("|", str);
        }

        public string[] ToKeyValueArray()
        {
            var list = new List<string>();

            foreach (var entry in this)
            {
                list.Add(entry.Key);
                list.Add(entry.Value);
            }
            return list.ToArray();
        }
        public string ToKeyValuePipe()
        {
            string[] val = ToKeyValueArray();
            return JoinArg(val);
        }
        #endregion

        #region ParseArgs


        public static GenericNameValue ParseRequest(System.Web.HttpRequest request)
        {

            if (request == null)
            {
                throw new ArgumentException("invalid request");
            }
            return ParseRequest(request.RawUrl);
        }

        public static GenericNameValue ParseRequest(string url)
        {

            if (url == null)
                url = string.Empty;

            string qs = string.Empty;

            if (url.Contains("?"))
            {
                qs = url.Substring(url.IndexOf("?") + 1);
                url = url.Substring(0, url.IndexOf("?"));
            }

            return ParseQueryString(qs);
        }
        #endregion

        #region ParseQueryString

        public static GenericNameValue ParseQueryString(params string[] qs)
        {
            StringBuilder sb = new StringBuilder();
            int len = qs.Length;
            for (int i = 0; i < qs.Length; i++)
            {
                sb.Append(qs[i]);
                if (i < len - 1 && !qs[i].EndsWith("&") && !qs[i + 1].StartsWith("&"))
                {
                    sb.Append("&");
                }
            }

            return ParseQueryString(sb.ToString());
        }

        public static string CLeanQueryString(string qs)
        {
            return qs.Replace("&amp;", "&");
        }

        public static GenericNameValue ParseQueryString(string qs)
        {
            GenericNameValue dictionary = new GenericNameValue();

            if (qs == null)
                qs = string.Empty;

            string str = CLeanQueryString(qs);

            if (string.IsNullOrEmpty(str))
            {
                return dictionary;
            }
            if (!str.Contains('='))
            {
                return dictionary;
            }

            foreach (string arg in str.Split(new char[] { '&' }))
            {
                if (!string.IsNullOrEmpty(arg))
                {
                    string[] strArray = arg.Split(new char[] { '=' });
                    if (strArray.Length == 2)
                    {
                        string key = Regx.RegexReplace("amp;", strArray[0], "");
                        dictionary[key] = strArray[1];
                    }
                    else
                    {
                        dictionary[arg] = null;
                    }
                }
            }

            return dictionary;
        }
        public static GenericNameValue ParseQueryString(NameValueCollection qs)
        {
            GenericNameValue dictionary = new GenericNameValue();
            if (qs != null)
            {
                for (int i = 0; i < qs.Count; i++)
                {
                    dictionary[qs.Keys[i]] = qs[i];
                }
            }

            return dictionary;
        }
        #endregion

        /// <summary>
        /// Get this as sorted <see cref="IOrderedEnumerable<KeyValuePair<string, object>>"./>
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<KeyValuePair<string, string>> Sorted()
        {
            var sortedDict = from entry in this orderby entry.Key ascending select entry;
            return sortedDict;
        }

    }

    [Serializable]
    public class GenericKeyValue<T> : List<KeyValuePair<string, T>>, ISerialEntity, IKeyValue<T>
    {

        #region collection methods


        public virtual void Add(string key, T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("GenericKeyValue.Add key");
            }
            if (Contains(key))
                RemoveItem(key);
            this.Add(new KeyValuePair<string, T>(key, value));
        }

        public virtual void RemoveItem(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("GenericKeyValue.RemoveItem key");
            }
            var item = GetItem(key);
            this.Remove(item);
        }

        public virtual KeyValuePair<string, T> GetItem(string key)
        {
            return this.Where(p => p.Key == key).FirstOrDefault();
        }



        public T this[string key]
        {
            get
            {
                return this.Where(p => p.Key == key).Select(p => p.Value).FirstOrDefault();
            }
            set
            {
                Add(key, value);
            }
        }

        public bool Contains(string key)
        {
            return this.Exists(p => p.Key == key);
        }

        public int IndexOf(string key)
        {
            return this.Select((n, i) => new { Value = n, Index = i }).First().Index;
        }

        #endregion

        #region ctor

        public GenericKeyValue()
        {

        }
        public GenericKeyValue(List<KeyValuePair<string, T>> keyValueList)
        {
            Load(keyValueList);
        }

        public GenericKeyValue(KeyValuePair<string, T>[] keyValueArray)
        {
            this.AddRange(keyValueArray);
        }

        public GenericKeyValue(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                EntityRead(ms, null);
            }
        }

        public GenericKeyValue(object[] keyValue)
        {
            Load(ParseQuery(keyValue));
        }

        public virtual void Prepare(DataRow dr)
        {
            this.ToKeyValue(dr);
        }
        internal static GenericKeyValue<T> CreateList(params object[] keyValue)
        {
            GenericKeyValue<T> query = new GenericKeyValue<T>(ParseQuery(keyValue));

            return query;
        }

        internal static List<KeyValuePair<string, T>> ParseQuery(params object[] keyValue)
        {
            List<KeyValuePair<string, T>> pair = new List<KeyValuePair<string, T>>();
            if (keyValue == null)
                return pair;
            int count = keyValue.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("keyValues parameter is not correct, Not match key value arguments");
            }
            Dictionary<string, T> args = new Dictionary<string, T>();
            for (int i = 0; i < count; i++)
            {
                object o = keyValue[i];
                if (o != null)
                {
                    pair.Add(new KeyValuePair<string, T>(keyValue[i].ToString(), (T)keyValue[++i]));
                }
            }
            return pair;
        }

        public void Load(List<KeyValuePair<string, T>> keyValueList)
        {
            this.Clear();
            this.AddRange(keyValueList.ToArray());
        }

        #endregion

        #region properties


        public T Get(string key)
        {
            return this[key];
        }

        public T Get(string key, T defaultValue)
        {
            return GenericTypes.Convert<T>(this[key], defaultValue);
        }

        public Type GetKeyType()
        {
            return typeof(string);
        }

        public Type GetValueType()
        {
            return typeof(string);
        }

        #endregion

        #region  IEntityFormatter

        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            ((BinaryStreamer)streamer).WriteGenericKeyValue<T>(this);


            //streamer.WriteValue(this);
            streamer.Flush();

        }

        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            //var o =(GenericKeyValue<T>) streamer.ReadValue();
            var o = ((BinaryStreamer)streamer).ReadGenericKeyValue<T>();

            //BinaryStreamer reader = new BinaryStreamer(stream);
            //var o = reader.ReadKeyValue<T>();
            Load(o);
        }


        #endregion

        #region converter

        public IDictionary ToDictionary()
        {
            var dict = this
               .Select(item => new { Key = item.Key, Value = item.Value })
               .Distinct()
               .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
            return dict;
        }

        public IDictionary<string, T> ToGenericDictionary()
        {
            var dict = this
               .Select(item => new { Key = item.Key, Value = item.Value })
               .Distinct()
               .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
            return dict;
        }

        public List<KeyValuePair<string, object>> ToList()
        {
            return this as List<KeyValuePair<string, object>>;
        }

        #endregion

        
    }

   
}
