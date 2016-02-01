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
using System.Data;
using System.Reflection;
using System.Collections;

namespace Nistec.Generic
{

    /// <summary>
    /// Represent KeySet collection
    /// </summary>
    [Serializable]
    public class KeySet : Dictionary<string, object>
    {
        #region ctor
        public KeySet()
        {

        }
        public KeySet(params string[] keys)
        {
            this.AddRange(keys);
        }

        public KeySet(DataColumn[] columnKeys)
        {
            string[] keys = (from property in columnKeys
                             select property.ColumnName).ToArray();
            this.AddRange(keys);
        }
        #endregion

        #region methods

        public void AddRange(string[] keys)
        {
            foreach (string s in keys)
            {
                this[s] = null;
            }
        }

        public string this[int index]
        {
            get { return this.Keys.ElementAt(index); }
        }

        /// <summary>
        /// Set DataTable primary key 
        /// </summary>
        /// <param name="dt"></param>
        public void SetPrimaryKeys(DataTable dt)
        {
            if (Count == 0)
            {
                return;
            }
            List<DataColumn> columns = new List<DataColumn>();
            for (int i = 0; i < this.Count; i++)
            {
                columns.Add(dt.Columns[this[i]]);
            }
            dt.PrimaryKey = columns.ToArray();
        }

        public string[] ToArray()
        {
            return this.Keys.ToArray();
        }

        /// <summary>
        /// Get Keys as string 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Count == 0)
            {
                return "";
            }
            return FormatPrimaryKey();
            //return FormatPrimaryKey(this.ToArray());
        }

        public string ToString(string separator, bool sorted)
        {
            if (Count == 0)
            {
                return "";
            }
            return FormatPrimaryKey(separator, sorted);
            //return string.Join(separator, this);
        }

        public string ToString(bool sorted)
        {
            if (Count == 0)
            {
                return "";
            }
            return FormatPrimaryKey("_", sorted);
        }

        public string CreatePrimaryKey(object instance, bool sorted = false)
        {
            IEnumerable<object> values = AttributeProvider.GetPropertiesValues(instance, this.ToArray(), sorted);
            return FormatPrimaryKey(values.ToArray());
        }

        public string CreatePrimaryKey(IDictionary<string, object> record, bool sorted = false)
        {
            IEnumerable<object> values = (sorted) ?
                from p in record.Where(p => this.Keys.Contains(p.Key)).OrderBy(p => p.Key)
                select p.Value
                :
                from p in record.Where(p => this.Keys.Contains(p.Key))
                select p.Value;
            return FormatPrimaryKey(values.ToArray());
        }

        public string FormatPrimaryKey(bool sorted = false)
        {
            IEnumerable<object> values = (sorted) ?
                from p in this.OrderBy(p => p.Key)
                select p.Value
                :
                from p in this
                select p.Value;
            return FormatPrimaryKey(values.ToArray());
        }

        public string FormatPrimaryKey(string separator, bool sorted = false)
        {
            IEnumerable<object> values = (sorted) ?
                from p in this.OrderBy(p => p.Key)
                select p.Value
                :
                from p in this
                select p.Value;
            return FormatPrimaryKey(separator,values.ToArray());
        }

        public string CreatePrimaryKey(bool sorted = false)
        {
            return CreatePrimaryKey(this, sorted);
        }
        #endregion

        #region static BuildKeys

        public static KeySet BuildKeys(IDictionary dic, string[] fields)
        {
            KeySet k = new KeySet(fields);
            for (int i = 0; i < fields.Length; i++)
            {
                k[fields[i]] = dic[fields[i]];
            }
            return k;
        }

        public static KeySet Get(params object[] keyValue)
        {
            KeySet k = new KeySet();
            k.SetKeyValues(keyValue);
            return k;
        }

        #endregion

        #region KeyFields

 
        public bool HasValues()
        {
            return this.All(p => p.Value != null);
        }

        public bool IsMatch(IDictionary<string, object> record)
        {
            var result = this.All(k => record.Any(r => k.Key == r.Key && k.Value == r.Value));
            return result;
        }

        #endregion

        #region static format

        public IList<string> KeyList()
        {
            return this.Keys.ToList<string>();
        }

        public IEnumerable<string> Sorted()
        {
            return this.Keys.OrderBy(k => k);
        }

        public static string FormatPrimaryKey(object instance, IEnumerable<string> keys, bool sorted = false)
        {
            IEnumerable<object> values = AttributeProvider.GetPropertiesValues(instance, keys, sorted);
            return FormatPrimaryKey(values.ToArray());
        }

        public static string FormatPrimaryKey(object[] keys)
        {
            return string.Join("_", keys);
        }

        public static string FormatPrimaryKey(string separator,object[] keys)
        {
            return string.Join(separator, keys);
        }

        
        #endregion
    }
}
