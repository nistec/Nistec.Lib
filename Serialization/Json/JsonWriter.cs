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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Serialization
{
    internal sealed class JsonWriter
    {
        public static JsonWriter Get(JsonSettings settings)
        {
            return new JsonWriter(settings);
        }

        readonly int _MAX_DEPTH = 20;
        int _current_depth = 0;

        private StringBuilder _output = new StringBuilder();
        private StringBuilder _before = new StringBuilder();
        private Dictionary<string, int> _GlobalTypes = new Dictionary<string, int>();
        private Dictionary<object, int> _CircularItems = new Dictionary<object, int>();
        private JsonSettings _Settings;
        private bool _useEscapedUnicode = false;
        private bool _isCircular = false;


        internal JsonWriter(JsonSettings settings)
        {
            _Settings = settings ?? JsonSerializer.DefaultOption;
            _useEscapedUnicode = _Settings.UseEscapedUnicode;
        }

        internal string ConvertToJson(object obj, Type baseType)
        {
            WriteValue(obj, baseType);

            string str = "";
            if (_Settings.UseTypesExtension && _GlobalTypes != null && _GlobalTypes.Count > 0)
            {
                StringBuilder sb = _before;
                if (_isCircular)
                    sb.Append("\"$circular\":true,");
                sb.Append("\"$types\":{");
                bool pendingSeparator = false;
                foreach (var kv in _GlobalTypes)
                {
                    if (pendingSeparator) sb.Append(',');
                    pendingSeparator = true;
                    sb.Append('\"');
                    sb.Append(kv.Key);
                    sb.Append("\":\"");
                    sb.Append(kv.Value);
                    sb.Append('\"');
                }
                sb.Append("},");
                sb.Append(_output.ToString());
                str = sb.ToString();
            }
            else
                str = _output.ToString();

            return str;
        }

        private void WriteValue(object obj, Type baseType)
        {
            if (obj == null || obj is DBNull)
            {
                _output.Append("null");
                return;
            }
            Type type = baseType == null ? obj.GetType() : baseType;

            switch (type.Name)
            {
                case "Boolean":
                    _output.Append(((bool)obj) ? "true" : "false"); break;// conform to standard
                case "Byte":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                case "SByte":
                case "Int16":
                case "Int32":
                case "Int64":
                case "Single":
                case "Double":
                case "Decimal":
                    _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo)); break;
                case "Char":
                case "String":
                    WriteString(obj.ToString()); break;
                case "DateTime":
                    WriteDateTime((DateTime)obj); break;
                case "TimeSpan":
                    WriteTimeSpan((TimeSpan)obj); break;
                case "Guid":
                    WriteGuid((Guid)obj); break;
                case "Byte[]":
                    WriteBytes((byte[])obj); break;
                case "Char[]":
                    WriteArray<Char>((Char[])obj); break;
                case "Int16[]":
                    WriteArray<Int16>((Int16[])obj); break;
                case "Int32[]":
                    WriteArray<Int32>((Int32[])obj); break;
                case "Int64[]":
                    WriteArray<Int64>((Int64[])obj); break;
                case "String[]":
                    WriteArray<String>((String[])obj); break;
                case "Object[]":
                    WriteArray<Object>((Object[])obj); break;
                default:
                    if (SerializeTools.IsGenericKeyStringDictionary(type))
                    {
                        if (_Settings.UseExtraKeyValueDictionary)
                            WriteDictionaryKeyValue((IDictionary)obj);
                        else
                            WriteStringDictionary((IDictionary)obj);
                    }
                    else if (SerializeTools.IsDictionary(type))
                    {
                        if (_Settings.UseExtraKeyValueDictionary)
                            WriteDictionaryKeyValue((IDictionary)obj);
                        else
                            WriteDictionary((IDictionary)obj);
                    }

                    else if (SerializeTools.IsDataSet(type))
                        WriteDataset((DataSet)obj);

                    else if (SerializeTools.IsDataTable(type))
                            this.WriteDataTable((DataTable)obj);
                 
                    else if (type == typeof(DataRow))
                        this.WriteDataRow((DataRow)obj);

                    else if (type == typeof(StringDictionary))
                        WriteStringDictionary((StringDictionary)obj);

                    else if (type == typeof(NameValueCollection))
                        WriteNameValue((NameValueCollection)obj);

                    else if (SerializeTools.IsEnumerable(type))
                        WriteArray((IEnumerable)obj);

                    else if (type.IsEnum)
                        WriteEnum((Enum)obj);

                    else if (JsonActivator.Get.IsTypeRegistered(type))
                        WriteCustom(obj);

                    else if (SerializeTools.IsISerialJson(type))
                        WriteSerialJson((ISerialJson)obj);
                    else
                        WriteObject(obj, baseType);

                    break;
            }

        }

        private void WriteSerialJson(ISerialJson serialJson)
        {
            _output.Append(serialJson.EntityWrite(null));
        }
        private void WriteNameValue(NameValueCollection nameValueCollection)
        {
            _output.Append('{');

            bool pendingSeparator = false;

            foreach (string key in nameValueCollection)
            {
                if (pendingSeparator) _output.Append(',');

                WritePair(key, nameValueCollection[key]);

                pendingSeparator = true;
            }
            _output.Append('}');
        }

        private void WriteStringDictionary(StringDictionary stringDictionary)
        {
            _output.Append('{');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in stringDictionary)
            {
                if (pendingSeparator) _output.Append(',');

                WritePair((string)entry.Key, entry.Value);

                pendingSeparator = true;
            }
            _output.Append('}');
        }

        private void WriteCustom(object obj)
        {
            SerializeFunc s;
            JsonActivator.Get._customSerializer.TryGetValue(obj.GetType(), out s);
            WriteStringValue(s(obj));
        }

        private void WriteEnum(Enum e)
        {
            if (_Settings.UseEnumValues)
                WriteValue(Convert.ToInt32(e), null);
            else
                WriteStringValue(e.ToString());
        }

        private void WriteGuid(Guid g)
        {
            if (_Settings.UseBinaryGuid == false)
                WriteStringValue(g.ToString());
            else
                WriteBytes(g.ToByteArray());
        }

        private void WriteBytes(byte[] bytes)
        {

            WriteStringValue(Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None));
        }

        private void WriteDateTime(DateTime dateTime)
        {
            // datetime format standard : yyyy-MM-ddTHH:mm:ss
            DateTime dt = dateTime;
            if (_Settings.UseUTCDateTime)
                dt = dateTime.ToUniversalTime();

            _output.Append('\"');
            _output.Append(dt.Year.ToString("0000", NumberFormatInfo.InvariantInfo));
            _output.Append('-');
            _output.Append(dt.Month.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append('-');
            _output.Append(dt.Day.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append('T');
            _output.Append(dt.Hour.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append(':');
            _output.Append(dt.Minute.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append(':');
            _output.Append(dt.Second.ToString("00", NumberFormatInfo.InvariantInfo));
            if (_Settings.EnableDateTimeMilliseconds)
            {
                _output.Append('.');
                _output.Append(dt.Millisecond.ToString("000", NumberFormatInfo.InvariantInfo));
            }
            if (_Settings.UseUTCDateTime)
                _output.Append('Z');

            _output.Append('\"');
        }

        private void WriteTimeSpan(TimeSpan time)
        {
            _output.Append('\"');
            _output.Append(time.Hours.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append(':');
            _output.Append(time.Minutes.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append(':');
            _output.Append(time.Seconds.ToString("00", NumberFormatInfo.InvariantInfo));
            if (_Settings.EnableDateTimeMilliseconds)
            {
                _output.Append('.');
                _output.Append(time.Milliseconds.ToString("000", NumberFormatInfo.InvariantInfo));
            }
            _output.Append('\"');
        }
        private JsonSchema GetSchema(DataTable ds)
        {
            if (ds == null) return null;

            JsonSchema m = new JsonSchema();
            m.Info = new List<string>();
            m.Name = ds.TableName;

            foreach (DataColumn c in ds.Columns)
            {
                m.Info.Add(ds.TableName);
                m.Info.Add(c.ColumnName);
                m.Info.Add(c.DataType.ToString());
            }
            // FEATURE : serialize relations and constraints here

            return m;
        }

        private JsonSchema GetSchema(DataSet ds)
        {
            if (ds == null) return null;

            JsonSchema m = new JsonSchema();
            m.Info = new List<string>();
            m.Name = ds.DataSetName;

            foreach (DataTable t in ds.Tables)
            {
                foreach (DataColumn c in t.Columns)
                {
                    m.Info.Add(t.TableName);
                    m.Info.Add(c.ColumnName);
                    m.Info.Add(c.DataType.ToString());
                }
            }
            return m;
        }

        private string GetXmlSchema(DataTable dt)
        {
            using (var writer = new StringWriter())
            {
                dt.WriteXmlSchema(writer);
                return dt.ToString();
            }
        }

        private void WriteDataset(DataSet ds)
        {
            _output.Append('{');
            if (_Settings.UseExtensions)
            {
                WritePair("$schema", _Settings.UseDatasetSchema ? (object)GetSchema(ds) : ds.GetXmlSchema());
                _output.Append(',');
            }
            bool tablesep = false;
            foreach (DataTable table in ds.Tables)
            {
                if (tablesep) _output.Append(',');
                tablesep = true;
                WriteDataTableData(table);
            }
            // end dataset
            _output.Append('}');
        }

        private void WriteDataRowAsArray(DataRow row)
        {

            DataTable table = row.Table;
            _output.Append('\"');
            _output.Append(table.TableName);
            _output.Append("\":[");

            DataColumnCollection cols = table.Columns;

            _output.Append('{');

            bool pendingSeperator = false;
            foreach (DataColumn column in cols)
            {
                if (pendingSeperator) _output.Append(',');
                WritePair(column.ColumnName, row[column], column.DataType);
                pendingSeperator = true;
            }
            _output.Append('}');
            _output.Append(']');
        }

        private void WriteDataRowData(DataRow row)
        {

            DataTable table = row.Table;

            DataColumnCollection cols = table.Columns;
            bool pendingSeperator = false;
            foreach (DataColumn column in cols)
            {
                if (pendingSeperator) _output.Append(',');
                WritePair(column.ColumnName, row[column], column.DataType);
                pendingSeperator = true;
            }
        }

        void WriteDataRow(DataRow row)
        {
            this._output.Append('{');
            if (_Settings.UseExtensions)
            {
                var dt = row.Table;
                this.WritePair("$schema", _Settings.UseDatasetSchema ? (object)this.GetSchema(dt) : this.GetXmlSchema(dt));
                this._output.Append(',');
            }

            WriteDataRowData(row);

            // end datatable
            this._output.Append('}');
        }

        internal string WriteDataTableToJson(DataTable dt)
        {
            this._output.Append('{');
            WriteDataTableData(dt);
            this._output.Append('}');

            return _output.ToString();

        }


        private void WriteDataTableData(DataTable table)
        {
            //_output.Append('\"');
            //_output.Append(table.TableName);
            //_output.Append("\":[");
            
            _output.Append("[");

            DataColumnCollection cols = table.Columns;
            bool rowseparator = false;
            foreach (DataRow row in table.Rows)
            {
                if (rowseparator) _output.Append(',');
                rowseparator = true;
                _output.Append('{');

                bool pendingSeperator = false;
                foreach (DataColumn column in cols)
                {
                    if (pendingSeperator) _output.Append(',');
                    WritePair(column.ColumnName, row[column], column.DataType);
                    pendingSeperator = true;
                }
                _output.Append('}');
            }

            _output.Append(']');
        }

        void WriteDataTable(DataTable dt)
        {
            if (_Settings.UseExtensions)
            {
                this._output.Append('{');
                this.WritePair("$schema", _Settings.UseDatasetSchema ? (object)this.GetSchema(dt) : this.GetXmlSchema(dt));
                this._output.Append(',');
            }
            WriteDataTableData(dt);

            // end datatable
            if (_Settings.UseExtensions)
                this._output.Append('}');
        }

        //void WriteDataTable(DataTable dt)
        //{
        //    this._output.Append('{');

        //    if (_Settings.UseExtensions)
        //    {
        //        this.WritePair("$schema", _Settings.UseDatasetSchema ? (object)this.GetSchema(dt) : this.GetXmlSchema(dt));
        //        this._output.Append(',');
        //    }
        //    WriteDataTableData(dt);

        //    // end datatable
        //    this._output.Append('}');
        //}


        bool _TypesWritten = false;


        private void WriteObject(object obj, Type baseType)
        {
            int i = 0;
            if (_CircularItems.TryGetValue(obj, out i) == false)
                _CircularItems.Add(obj, _CircularItems.Count + 1);
            else
            {
                if (_current_depth > 0)
                {
                    _isCircular = true;
                    _output.Append("{\"$i\":" + i + "}");
                    return;
                }
            }
            if (_Settings.UseTypesExtension == false)
                _output.Append('{');
            else
            {
                if (_TypesWritten == false)
                {
                    _output.Append('{');
                    _before = _output;
                    _output = new StringBuilder();
                }
                else
                    _output.Append('{');
            }
            _TypesWritten = true;
            _current_depth++;
            if (_current_depth > _MAX_DEPTH)
                throw new Exception("Serializer encountered maximum depth of " + _MAX_DEPTH);


            Dictionary<string, string> map = new Dictionary<string, string>();
            Type t = baseType == null ? obj.GetType() : baseType;
            bool append = false;
            if (_Settings.UseExtensions)
            {
                if (_Settings.UseTypesExtension == false)
                    WritePairString("$type", JsonActivator.Get.GetTypeAssemblyName(t));
                else
                {
                    int dt = 0;
                    string ct = JsonActivator.Get.GetTypeAssemblyName(t);
                    if (_GlobalTypes.TryGetValue(ct, out dt) == false)
                    {
                        dt = _GlobalTypes.Count + 1;
                        _GlobalTypes.Add(ct, dt);
                    }
                    WritePairString("$type", dt.ToString());
                }
                append = true;
            }

            JsonFields[] g = JsonActivator.Get.GetFieldsGetter(t, _Settings.ShowReadOnlyProperties, _Settings.IgnoreAttributes);
            int c = g.Length;
            for (int ii = 0; ii < c; ii++)
            {
                var p = g[ii];
                object o = p.Field(obj);
                if ((o == null || o is DBNull) && _Settings.SerializeNullValues == false)
                {
                    //append = false;
                }
                else
                {
                    if (append)
                        _output.Append(',');

                    WritePair(p.Name, o);
                    if (o != null && _Settings.UseExtensions)
                    {
                        Type tt = o.GetType();
                        if (tt == typeof(System.Object))
                            map.Add(p.Name, tt.ToString());
                    }
                    append = true;
                }
            }
            if (map.Count > 0 && _Settings.UseExtensions)
            {
                _output.Append(",\"$map\":");
                WriteStringDictionary(map);
            }
            //_current_depth--;
            _output.Append('}');
            _current_depth--;
        }

        private void WritePairString(string name, string value)
        {
            if ((value == null) && _Settings.SerializeNullValues == false)
                return;
            WriteStringValue(name);

            _output.Append(':');

            WriteStringValue(value);
        }

        private void WritePair(string name, object value)
        {
            if ((value == null || value is DBNull) && _Settings.SerializeNullValues == false)
                return;
            WriteStringValue(name);

            _output.Append(':');

            WriteValue(value, null);
        }

        private void WritePair(string name, object value, Type baseType)
        {
            if ((value == null || value is DBNull) && _Settings.SerializeNullValues == false)
                return;
            WriteStringValue(name);

            _output.Append(':');

            WriteValue(value, baseType);
        }
        private void WriteArray(IEnumerable array)
        {
            _output.Append('[');

            bool pendingSeperator = false;

            foreach (object obj in array)
            {
                if (pendingSeperator) _output.Append(',');

                WriteValue(obj, null);

                pendingSeperator = true;
            }
            _output.Append(']');
        }

        private void WriteArray<T>(IEnumerable<T> array)
        {
            _output.Append('[');

            bool pendingSeperator = false;

            foreach (var obj in array)
            {
                if (pendingSeperator) _output.Append(',');

                WriteValue(obj, typeof(T));

                pendingSeperator = true;
            }
            _output.Append(']');
        }
        private void WriteStringDictionary(IDictionary dic)
        {
            _output.Append('{');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in dic)
            {
                if (pendingSeparator) _output.Append(',');

                WritePair((string)entry.Key, entry.Value);

                pendingSeparator = true;
            }
            _output.Append('}');
        }

        private void WriteDictionary(IDictionary dic)
        {
            _output.Append('{');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in dic)
            {
                if (pendingSeparator) _output.Append(',');
                WritePair((string)entry.Key, entry.Value);
                pendingSeparator = true;
            }
            _output.Append('}');
        }


        private void WriteDictionaryKeyValue(IDictionary dic)
        {
            _output.Append('[');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in dic)
            {
                if (pendingSeparator) _output.Append(',');
                _output.Append('{');

                WritePair("k", entry.Key);
                _output.Append(",");
                WritePair("v", entry.Value);

                _output.Append('}');

                pendingSeparator = true;
            }
            _output.Append(']');
        }

        private void WriteKeyValue(IKeyValue kv)
        {

            _output.Append('{');

            bool pendingSeparator = false;

            foreach (var entry in kv.ToList())
            {
                if (pendingSeparator) _output.Append(',');
                WritePair((string)entry.Key, entry.Value);
                pendingSeparator = true;
            }
            _output.Append('}');

        }

        private void WriteStringValue(string s)
        {
            _output.Append('\"');
            _output.Append(s);
            _output.Append('\"');
        }

        private void WriteString(string s)
        {
            _output.Append('\"');

            int runIndex = -1;
            int l = s.Length;
            for (var index = 0; index < l; ++index)
            {
                var c = s[index];

                if (_useEscapedUnicode)
                {
                    if (c >= ' ' && c < 128 && c != '\"' && c != '\\')
                    {
                        if (runIndex == -1)
                            runIndex = index;

                        continue;
                    }
                }
                else
                {
                    if (c != '\t' && c != '\n' && c != '\r' && c != '\"' && c != '\\')// && c != ':' && c!=',')
                    {
                        if (runIndex == -1)
                            runIndex = index;

                        continue;
                    }
                }

                if (runIndex != -1)
                {
                    _output.Append(s, runIndex, index - runIndex);
                    runIndex = -1;
                }

                switch (c)
                {
                    case '\t': _output.Append("\\t"); break;
                    case '\r': _output.Append("\\r"); break;
                    case '\n': _output.Append("\\n"); break;
                    case '"':
                    case '\\': _output.Append('\\'); _output.Append(c); break;
                    default:
                        if (_useEscapedUnicode)
                        {
                            _output.Append("\\u");
                            _output.Append(((int)c).ToString("X4", NumberFormatInfo.InvariantInfo));
                        }
                        else
                            _output.Append(c);

                        break;
                }
            }

            if (runIndex != -1)
                _output.Append(s, runIndex, s.Length - runIndex);


            _output.Append('\"');
        }
    }
}
