using Nistec.Data;
using Nistec.IO;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Generic
{


    [Serializable]
    public class KeyValueArgs : Dictionary<string, object>, ISerialEntity, IDataRowAdaptor, ISerialJson, IKeyValue<object>
    {
        #region static

        public static KeyValueArgs Get(params object[] keyValue)
        {
            if (keyValue == null)
                return null;
            return new KeyValueArgs(keyValue);
        }

        public static KeyValueArgs Convert(IDictionary<string, object> dic)
        {
            if (dic == null)
                return null;
            var nva = new KeyValueArgs();

            foreach (var entry in dic.ToArray())
            {
                nva[entry.Key] = entry.Value==null? null: entry.Value.ToString();
            }
            return nva;
        }

        #endregion

        #region ctor

        public KeyValueArgs()
        {
        }
        public KeyValueArgs(IEnumerable<KeyValuePair<string, object>> keyValueList)
        {
            Load(keyValueList);
        }
        public KeyValueArgs(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                EntityRead(ms, null);
            }
        }
        public KeyValueArgs(NetStream stream)
        {
            EntityRead(stream, null);
        }

        public KeyValueArgs(object[] keyValue)
        {
            Parse(keyValue);
            //Load(ParseQuery(keyValue));
        }

        //public static KeyValueArgs Create(params string[] keyValue)
        //{
        //    var pair = ParseQuery(keyValue);
        //    KeyValueArgs query = new KeyValueArgs();
        //    query.Load(pair);
        //    return query;
        //}

        void Parse(object[] keyValueParameters)
        {
            if (keyValueParameters == null)
            {
                throw new ArgumentNullException("keyValueParameters");
            }

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                this[keyValueParameters[i].ToString()] = keyValueParameters[++i];
            }
        }
        public virtual void Prepare(DataRow dr)
        {
            this.ToKeyValue(dr);
        }

        #endregion

        #region properties

        public object Get(string key)
        {
            object value;
            TryGetValue(key, out value);
            return value;
            //return this[key];
        }

        public TV Get<TV>(string key)
        {
            return GenericTypes.Convert<TV>(this[key]);
        }

        public TV Get<TV>(string key, TV defaultValue)
        {
            return GenericTypes.Convert<TV>(this[key], defaultValue);
        }

        #endregion

        #region collection methods

        /// <summary>
        /// Get this as sorted "IOrderedEnumerable !KeyValuePair string, object"
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<KeyValuePair<string, object>> Sorted()
        {
            var sortedDict = from entry in this orderby entry.Key ascending select entry;
            return sortedDict;
        }
        #endregion

        #region Loaders

        public virtual void Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("KeyValueArgs.Add key");
            }

            base.Add(key, value == null ? null : value.ToString());
        }

        void Load(IEnumerable<KeyValuePair<string, object>> keyValueList)
        {
            if (keyValueList == null)
            {
                throw new ArgumentNullException("KeyValueArgs.keyValueList");
            }
            //this.Clear();
            //this.AddRange(keyValueList.ToArray());
            foreach (var entry in keyValueList.ToArray())
            {
                this[entry.Key] = entry.Value;
            }
        }

        void Load(NameValueCollection qs)
        {
            if (qs == null)
            {
                throw new ArgumentNullException("KeyValueArgs.qs");
            }

            for (int i = 0; i < qs.Count; i++)
            {
                this[qs.Keys[i]] = qs[i];
            }
        }

        void Copy(IDictionary<string, object> dic)
        {
            foreach (var entry in dic.ToArray())
            {
                this[entry.Key] = entry.Value;
            }
        }

        #endregion

        #region converter
        public bool Contains(string key, object value)
        {
            //return this.Exists(p => p.Key == key && p.Value == value);
            return this.Where(p => p.Key == key && p.Value == value).Count() > 0;
        }

        public new bool Contains(KeyValuePair<string, object> item)
        {
            return this.Where(p => p.Key == item.Key && p.Value == item.Value).Count() > 0;
        }

        public virtual KeyValuePair<string, object> GetItem(string key, object value)
        {
            return this.Where(p => p.Key == key && p.Value == value).FirstOrDefault();
        }
        public object[] ToKeyValueArray()
        {
            var list = new List<object>();

            foreach (var entry in this)
            {
                list.Add(entry.Key);
                list.Add(entry.Value);
            }
            return list.ToArray();
        }
        public IDictionary<string, object> ToDictionary()
        {
            //var dict = this
            //   .Select(item => new { Key = item.Key, Value = item.Value })
            //   .Distinct()
            //   .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
            //return dict;

            return this;
        }

        #endregion

        #region  ISerialEntity

        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            ((BinaryStreamer)streamer).WriteDirectDictionary<string, object>(this);
            streamer.Flush();
        }

        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            this.Clear();
            ((BinaryStreamer)streamer).TryReadDirectToDictionary<string, object>(this,false);
        }

        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }

        #endregion

        #region ISerialJson

        public static KeyValueArgs ParseJson(string json)
        {
            if (json == null)
            {
                return null;
            }
            var nv = new KeyValueArgs();
            nv.EntityRead(json, null);
            return nv;
        }

        public string ToJson(bool pretty = false)
        {
            return EntityWrite(new JsonSerializer(JsonSerializerMode.Write, null), pretty);
        }

        public string EntityWrite(IJsonSerializer serializer, bool pretty = false)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Write, null);

            foreach (var entry in this)
            {
                serializer.WriteToken(entry.Key, entry.Value);
            }
            return serializer.WriteOutput(pretty);
        }

        public object EntityRead(string json, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, null);

            serializer.ParseTo(this, json);

            //var dic = serializer.ParseToDictionaryString(json);

            //AddRange(dic.ToArray());

            return this;
        }


        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in this)
                sb.AppendFormat("{0}:{1},", entry.Key, entry.Value);
            return sb.ToString().TrimEnd(',');
        }

    }
}
