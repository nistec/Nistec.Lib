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
using Nistec.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace Nistec.Serialization
{
    internal class JsonReader
    {
        public static JsonReader Get(JsonSettings settings)
        {
            return new JsonReader(settings);
        }

        public JsonReader(JsonSettings settings)
        {
            _Settings = settings ?? JsonSerializer.DefaultOption;
        }

        private JsonSettings _Settings;
        private bool _useGlobalTypes = false;
        private Dictionary<object, int> _CircularItems = new Dictionary<object, int>();
        private Dictionary<int, object> _CircularItemsRev = new Dictionary<int, object>();
        private bool _isCircular = true;

        public T ToObject<T>(string json)
        {
            Type t = typeof(T);
            var o = ToObject(json, t);

            if (t.IsArray)
            {
                if ((o as ICollection).Count == 0) // edge case for "[]" -> T[]
                {
                    Type tt = t.GetElementType();
                    object oo = Array.CreateInstance(tt, 0);
                    return (T)oo;
                }
                else
                    return (T)o;
            }
            else
                return (T)o;
        }

        public object ToObject(string json)
        {
            return ToObject(json, null);
        }

        public object ToObject(string json, Type type)
        {
            Type t = null;
            if (type != null && type.IsGenericType)
                t = JsonActivator.Get.GetGenericTypeDefinition(type);
            if (t == typeof(Dictionary<,>) || t == typeof(List<>))
                _Settings.UseTypesExtension = false;

            _useGlobalTypes = _Settings.UseTypesExtension;

            if (type != null && SerializeTools.IsISerialJson(type))
            {
                object instance = ActivatorUtil.CreateInstance(type);
                return ((ISerialJson)instance).EntityRead(json, null);
            }

            object o = JsonParser.Parse(json, _Settings.IgnoreCaseOnDeserialize);
            if (o == null)
                return null;


            if (type != null && type == typeof(DataSet))
                return CreateDataset(o as Dictionary<string, object>, null);

            if (type != null && type == typeof(DataTable))
            {
                if (_Settings.UseDatasetSchema)
                    return CreateDataTable(o as Dictionary<string, object>, null);
                else //if (_Settings.)
                    return ToDataTable(o as IList<object>);
            }
            if (o is IDictionary)
            {
                if (type != null && t == typeof(Dictionary<,>)) // deserialize a dictionary
                    return RootDictionary(o, type);
                else // deserialize an object
                    return ParseDictionary(o as Dictionary<string, object>, null, type, null);
            }

            if (o is List<object>)
            {
                if (type != null && t == typeof(Dictionary<,>)) // kv format
                    return RootDictionary(o, type);

                if (type != null && t == typeof(List<>)) // deserialize to generic list
                    return RootList(o, type);

                if (type == typeof(Hashtable))
                    return RootHashTable((List<object>)o);
                else
                    return (o as List<object>).ToArray();
            }

            if (type != null && o.GetType() != type)
                return ChangeType(o, type);

            return o;
        }

        #region   private methods
        private object RootHashTable(List<object> o)
        {
            Hashtable h = new Hashtable();

            foreach (Dictionary<string, object> values in o)
            {
                object key = values["k"];
                object val = values["v"];
                if (key is Dictionary<string, object>)
                    key = ParseDictionary((Dictionary<string, object>)key, null, typeof(object), null);

                if (val is Dictionary<string, object>)
                    val = ParseDictionary((Dictionary<string, object>)val, null, typeof(object), null);

                h.Add(key, val);
            }

            return h;
        }

        private object ChangeType(object value, Type type)
        {

            if (value == null)
            {
                if (IsNullable(type))
                    return value;
                else
                    return UnderlyingTypeOf(type);
            }


            if (type == typeof(int))
                return (int)((long)value);

            else if (type == typeof(long))
                return (long)value;

            else if (type == typeof(string))
                return (string)value;

            else if (type == typeof(Guid))
                return JsonConverter.ToGuid((string)value);

            else if (type.IsEnum)
                return JsonConverter.ToEnum(type, value);

            else if (type == typeof(DateTime))
                return JsonConverter.ToDateTime((string)value, _Settings.UseUTCDateTime);

            else if (JsonActivator.Get.IsTypeRegistered(type))
                return JsonActivator.Get.CreateCustom((string)value, type);

            if (IsNullable(type))
            {
                if (value == null)
                {
                    return value;
                }
                type = UnderlyingTypeOf(type);
            }

            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        private bool IsNullable(Type t)
        {
            if (!t.IsGenericType) return false;
            Type g = t.GetGenericTypeDefinition();
            return (g.Equals(typeof(Nullable<>)));
        }

        private Type UnderlyingTypeOf(Type t)
        {
            return t.GetGenericArguments()[0];
        }

        private object RootList(object parse, Type type)
        {
            Type[] gtypes = JsonActivator.Get.GetGenericArguments(type);
            IList o = (IList)ActivatorUtil.CreateInstance(type);

            foreach (var k in (IList)parse)
            {
                _useGlobalTypes = false;
                object v = k;
                if (k is Dictionary<string, object>)
                    v = ParseDictionary(k as Dictionary<string, object>, null, gtypes[0], null);
                else
                    v = ChangeType(k, gtypes[0]);

                o.Add(v);
            }
            return o;
        }


        private object RootDictionary(object parse, Type type)
        {
            Type[] gtypes = JsonActivator.Get.GetGenericArguments(type);
            Type t1 = null;
            Type t2 = null;
            if (gtypes != null)
            {
                t1 = gtypes[0];
                t2 = gtypes[1];
            }
            if (parse is Dictionary<string, object>)
            {
                IDictionary o = (IDictionary)ActivatorUtil.CreateInstance(type);

                foreach (var kv in (Dictionary<string, object>)parse)
                {
                    object v;
                    object k = ChangeType(kv.Key, t1);

                    if (kv.Value is Dictionary<string, object>)
                        v = ParseDictionary(kv.Value as Dictionary<string, object>, null, t2, null);

                    else if (t2.IsArray)
                        v = CreateArray((List<object>)kv.Value, t2, t2.GetElementType(), null);

                    else if (kv.Value is IList)
                        v = CreateGenericList((List<object>)kv.Value, t2, t1, null);

                    else
                        v = ChangeType(kv.Value, t2);

                    o.Add(k, v);
                }

                return o;
            }
            if (parse is List<object>)
                return CreateDictionary(parse as List<object>, type, gtypes, null);

            return null;
        }

        internal object ParseDictionary(Dictionary<string, object> d, Dictionary<string, object> globaltypes, Type type, object input)
        {
            object tn = "";
            if (type == typeof(NameValueCollection))
                return CreateNameValueCollection(d);
            if (type == typeof(StringDictionary))
                return CreateStringDictionary(d);
            if (_isCircular == false)
                _isCircular = d.TryGetValue("$circular", out tn);

            if (d.TryGetValue("$i", out tn))
            {
                object v = null;
                _CircularItemsRev.TryGetValue((int)(long)tn, out v);
                return v;
            }

            if (d.TryGetValue("$types", out tn))
            {
                _useGlobalTypes = true;
                globaltypes = new Dictionary<string, object>();
                foreach (var kv in (Dictionary<string, object>)tn)
                {
                    globaltypes.Add((string)kv.Value, kv.Key);
                }
            }

            bool found = d.TryGetValue("$type", out tn);

            if (found == false && type == typeof(System.Object))
            {
                return d;   // CreateDataset(d, globaltypes);
            }

            if (found)
            {
                if (_useGlobalTypes)
                {
                    object tname = "";
                    if (globaltypes != null && globaltypes.TryGetValue((string)tn, out tname))
                        tn = tname;
                }
                //--type = JsonActivator.Instance.GetTypeFromCache((string)tn);
                type = JsonActivator.Get.GetTypeFromName((string)tn);
            }

            if (type == null)
                throw new Exception("Cannot determine type");

            string typename = type.FullName;
            object o = input;
            if (o == null)
            {
                if (_Settings.UseUninitializedObject)
                    o = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                else
                    o = ActivatorUtil.CreateInstance(type);
            }
            if (_isCircular)
            {
                int i = 0;
                if (_CircularItems.TryGetValue(o, out i) == false)
                {
                    i = _CircularItems.Count + 1;
                    _CircularItems.Add(o, i);
                    _CircularItemsRev.Add(i, o);
                }
            }

            Dictionary<string, TypeInfo> props = JsonActivator.Get.GetTypesInfo(type, typename, _Settings.IgnoreCaseOnDeserialize, JsonActivator.Get.IsTypeRegistered(type));
            foreach (string n in d.Keys)
            {
                string name = n;
                if (_Settings.IgnoreCaseOnDeserialize) name = name.ToLower();
                if (name == "$map")
                {
                    ProcessMap(o, props, (Dictionary<string, object>)d[name]);
                    continue;
                }
                TypeInfo typeInfo;
                if (props.TryGetValue(name, out typeInfo) == false)
                    continue;
                if (typeInfo.CanWrite)
                {
                    object v = d[name];

                    if (v != null)
                    {
                        object oset = null;

                        switch (typeInfo.serialType)
                        {
                            case SerialJsonType.Int: oset = (int)((long)v); break;
                            case SerialJsonType.Long: oset = (long)v; break;
                            case SerialJsonType.String: oset = (string)v; break;
                            case SerialJsonType.Bool: oset = (bool)v; break;
                            case SerialJsonType.DateTime: oset = JsonConverter.ToDateTime((string)v, true); break;
                            case SerialJsonType.Enum: oset = JsonConverter.ToEnum(typeInfo.propertyType, v); break;
                            case SerialJsonType.Guid: oset = JsonConverter.ToGuid((string)v); break;

                            case SerialJsonType.Array:
                                if (!typeInfo.IsValueType)
                                    oset = CreateArray((List<object>)v, typeInfo.propertyType, typeInfo.elementType, globaltypes);
                                // what about 'else'?
                                break;
                            case SerialJsonType.ByteArray: oset = Convert.FromBase64String((string)v); break;

                            case SerialJsonType.DataSet: oset = CreateDataset((Dictionary<string, object>)v, globaltypes); break;
                            case SerialJsonType.DataTable: oset = CreateDataTable((Dictionary<string, object>)v, globaltypes); break;
                            case SerialJsonType.Hashtable: // same case as Dictionary

                            case SerialJsonType.Dictionary: oset = CreateDictionary((List<object>)v, typeInfo.propertyType, typeInfo.GenericTypes, globaltypes); break;
                            case SerialJsonType.StringKeyDictionary: oset = CreateStringKeyDictionary((Dictionary<string, object>)v, typeInfo.propertyType, typeInfo.GenericTypes, globaltypes); break;
                            case SerialJsonType.NameValue: oset = CreateNameValueCollection((Dictionary<string, object>)v); break;
                            case SerialJsonType.StringDictionary: oset = CreateStringDictionary((Dictionary<string, object>)v); break;
                            case SerialJsonType.Custom: oset = JsonActivator.Get.CreateCustom((string)v, typeInfo.propertyType); break;
                            default:
                                {
                                    if (typeInfo.IsGenericType && typeInfo.IsValueType == false && v is List<object>)
                                        oset = CreateGenericList((List<object>)v, typeInfo.propertyType, typeInfo.elementType, globaltypes);

                                    else if ((typeInfo.IsClass || typeInfo.IsStruct) && v is Dictionary<string, object>)
                                        oset = ParseDictionary((Dictionary<string, object>)v, globaltypes, typeInfo.propertyType, typeInfo.getterField(o));

                                    else if (v is List<object>)
                                        oset = CreateArray((List<object>)v, typeInfo.propertyType, typeof(object), globaltypes);

                                    else if (typeInfo.IsValueType)
                                        oset = ChangeType(v, typeInfo.changeType);

                                    else
                                        oset = v;
                                }
                                break;
                        }

                        o = typeInfo.setterField(o, oset);
                    }
                }
            }
            return o;
        }

        private StringDictionary CreateStringDictionary(Dictionary<string, object> d)
        {
            StringDictionary nv = new StringDictionary();

            foreach (var o in d)
                nv.Add(o.Key, (string)o.Value);

            return nv;
        }

        private NameValueCollection CreateNameValueCollection(Dictionary<string, object> d)
        {
            NameValueCollection nv = new NameValueCollection();

            foreach (var o in d)
                nv.Add(o.Key, (string)o.Value);

            return nv;
        }

        private void ProcessMap(object obj, Dictionary<string, TypeInfo> props, Dictionary<string, object> dic)
        {
            foreach (KeyValuePair<string, object> kv in dic)
            {
                TypeInfo p = props[kv.Key];
                object o = p.getterField(obj);
                Type t = Type.GetType((string)kv.Value);
                if (t == typeof(Guid))
                    p.setterField(obj, JsonConverter.ToGuid((string)o));
            }
        }
        /*
        private int CreateInteger(string s, int index, int count)
        {
            int num = 0;
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

        private object CreateEnum(Type type, object value)
        {
            if (value == null)
                return value;
            return EnumExtension.Parse(type, value.ToString(), null);
        }

        private Guid CreateGuid(string s)
        {
            if (s == null)
                return Guid.Empty;

            if (s.Length > 30)
                return new Guid(s);
            else
                return new Guid(Convert.FromBase64String(s));
        }

        private DateTime CreateDateTime(string value)
        {
            bool utc = false;
            //                   0123456789012345678 9012 9/3
            // datetime format = yyyy-MM-dd HH:mm:ss .nnn  Z
            int year;
            int month;
            int day;
            int hour;
            int min;
            int sec;
            int ms = 0;

            year = CreateInteger(value, 0, 4);
            month = CreateInteger( value, 5, 2);
            day = CreateInteger(value, 8, 2);
            hour = CreateInteger( value, 11, 2);
            min = CreateInteger( value, 14, 2);
            sec = CreateInteger( value, 17, 2);
            if (value.Length > 21 && value[19] == '.')
                ms = CreateInteger( value, 20, 3);

            //if (value.EndsWith("Z"))
            if (value[value.Length - 1] == 'Z')
                utc = true;

            if (_Settings.UseUTCDateTime == false && utc == false)
                return new DateTime(year, month, day, hour, min, sec, ms);
            else
                return new DateTime(year, month, day, hour, min, sec, ms, DateTimeKind.Utc).ToLocalTime();
        }
        */
        private object CreateArray(List<object> data, Type pt, Type elementType, Dictionary<string, object> globalTypes)
        {
            Array col = Array.CreateInstance(elementType, data.Count);
            // create an array of objects
            for (int i = 0; i < data.Count; i++)
            {
                object ob = data[i];
                if (ob is IDictionary)
                    col.SetValue(ParseDictionary((Dictionary<string, object>)ob, globalTypes, elementType, null), i);
                else
                    col.SetValue(ChangeType(ob, elementType), i);
            }

            return col;
        }

        private object CreateArray<T>(List<T> data)
        {
            Type elementType = typeof(T);

            Array col = Array.CreateInstance(elementType, data.Count);
            // create an array of T
            for (int i = 0; i < data.Count; i++)
            {
                col.SetValue(data[i], i);
            }

            return col;
        }

        private object CreateGenericList(List<object> data, Type propertyType, Type elementType, Dictionary<string, object> globalTypes)
        {
            IList col = (IList)ActivatorUtil.CreateInstance(propertyType);
            // create an array of objects
            foreach (object ob in data)
            {
                if (ob is IDictionary)
                    col.Add(ParseDictionary((Dictionary<string, object>)ob, globalTypes, elementType, null));

                else if (ob is List<object>)
                {
                    if (elementType.IsGenericType)
                        col.Add((List<object>)ob);//).ToArray());
                    else
                        col.Add(((List<object>)ob).ToArray());
                }
                else
                    col.Add(ChangeType(ob, elementType));
            }
            return col;
        }

        private object CreateStringKeyDictionary(Dictionary<string, object> reader, Type propertyType, Type[] types, Dictionary<string, object> globalTypes)
        {
            var col = (IDictionary)ActivatorUtil.CreateInstance(propertyType);
            Type t1 = null;
            Type t2 = null;
            if (types != null)
            {
                t1 = types[0];
                t2 = types[1];
            }

            foreach (KeyValuePair<string, object> values in reader)
            {
                var key = values.Key;
                object val = null;

                if (values.Value is Dictionary<string, object>)
                    val = ParseDictionary((Dictionary<string, object>)values.Value, globalTypes, t2, null);

                else if (types != null && t2.IsArray)
                    val = CreateArray((List<object>)values.Value, t2, t2.GetElementType(), globalTypes);

                else if (values.Value is IList)
                    val = CreateGenericList((List<object>)values.Value, t2, t1, globalTypes);

                else
                    val = ChangeType(values.Value, t2);

                col.Add(key, val);
            }

            return col;
        }

        private object CreateDictionary(List<object> reader, Type propertyType, Type[] types, Dictionary<string, object> globalTypes)
        {
            IDictionary col = (IDictionary)ActivatorUtil.CreateInstance(propertyType);
            Type t1 = null;
            Type t2 = null;
            if (types != null)
            {
                t1 = types[0];
                t2 = types[1];
            }

            foreach (Dictionary<string, object> values in reader)
            {
                object key = values["k"];
                object val = values["v"];

                if (key is Dictionary<string, object>)
                    key = ParseDictionary((Dictionary<string, object>)key, globalTypes, t1, null);
                else
                    key = ChangeType(key, t1);

                if (val is Dictionary<string, object>)
                    val = ParseDictionary((Dictionary<string, object>)val, globalTypes, t2, null);
                else
                    val = ChangeType(val, t2);

                col.Add(key, val);
            }

            return col;
        }

         private DataSet CreateDataset(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
        {
            DataSet ds = new DataSet();
            ds.EnforceConstraints = false;
            ds.BeginInit();

            // read dataset schema here
            var schema = reader["$schema"];

            if (schema is string)
            {
                TextReader tr = new StringReader((string)schema);
                ds.ReadXmlSchema(tr);
            }
            else
            {
                JsonSchema ms = (JsonSchema)ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(JsonSchema), null);
                ds.DataSetName = ms.Name;
                for (int i = 0; i < ms.Info.Count; i += 3)
                {
                    if (ds.Tables.Contains(ms.Info[i]) == false)
                        ds.Tables.Add(ms.Info[i]);
                    ds.Tables[ms.Info[i]].Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
                }
            }

            foreach (KeyValuePair<string, object> pair in reader)
            {
                if (pair.Key == "$type" || pair.Key == "$schema") continue;

                List<object> rows = (List<object>)pair.Value;
                if (rows == null) continue;

                DataTable dt = ds.Tables[pair.Key];
                ReadDataTable(rows, dt);
            }

            ds.EndInit();

            return ds;
        }

        private void ReadDataTable(List<object> rows, DataTable dt)
        {
            dt.BeginInit();
            dt.BeginLoadData();
            List<int> guidcols = new List<int>();
            List<int> datecol = new List<int>();

            foreach (DataColumn c in dt.Columns)
            {
                if (c.DataType == typeof(Guid) || c.DataType == typeof(Guid?))
                    guidcols.Add(c.Ordinal);
                if (_Settings.UseUTCDateTime && (c.DataType == typeof(DateTime) || c.DataType == typeof(DateTime?)))
                    datecol.Add(c.Ordinal);
            }

            foreach (List<object> row in rows)
            {
                object[] v = new object[row.Count];
                row.CopyTo(v, 0);
                foreach (int i in guidcols)
                {
                    string s = (string)v[i];
                    if (s != null && s.Length < 36)
                        v[i] = new Guid(Convert.FromBase64String(s));
                }
                if (_Settings.UseUTCDateTime)
                {
                    foreach (int i in datecol)
                    {
                        string s = (string)v[i];
                        if (s != null)
                            v[i] = JsonConverter.ToDateTime(s, _Settings.UseUTCDateTime);
                    }
                }
                dt.Rows.Add(v);
            }

            dt.EndLoadData();
            dt.EndInit();
        }

        DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection props =
            TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
        DataTable ToDataTable(IList<object> data)
        {
            DataTable table = new DataTable();
            if (data != null && data.Count > 0)
            {
                Dictionary<string, object> columns = (Dictionary<string, object>)data[0];

                foreach (var entry in columns)
                {
                    table.Columns.Add(entry.Key, entry.Value == null ? typeof(object) : entry.Value.GetType());
                }
                int count = table.Columns.Count;
                foreach (Dictionary<string, object> row in data)
                {
                    int i = 0;
                    object[] values = new object[count];
                    foreach (var entry in row)
                    {
                        values[i] = entry.Value;
                        i++;
                    }
                    table.Rows.Add(values);
                }
            }
            return table;
        }

        DataTable CreateDataTable(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
        {
            var dt = new DataTable();

            // read dataset schema here
            object schema= reader["$schema"];
            //reader.TryGetValue("$schema", out schema);
            
            if (schema != null && schema is string)
            {
                TextReader tr = new StringReader((string)schema);
                dt.ReadXmlSchema(tr);
            }
            else
            {
                var ms = (JsonSchema)this.ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(JsonSchema), null);
                dt.TableName = ms.Info[0];
                for (int i = 0; i < ms.Info.Count; i += 3)
                {
                    dt.Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
                }
            }


            foreach (var pair in reader)
            {
                if (pair.Key == "$type" || pair.Key == "$schema")
                    continue;

                var rows = (List<object>)pair.Value;
                if (rows == null)
                    continue;

                if (!dt.TableName.Equals(pair.Key, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                ReadDataTable(rows, dt);
            }

            return dt;
        }

       

        #endregion
    }
}
