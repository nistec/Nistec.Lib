using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Generic
{
 
        
        /// <summary>
        /// Generic tuple for a key and a pair of values
        /// </summary>
        /// <typeparam name="TKey">The Type of the key</typeparam>
        /// <typeparam name="T1">The Type of the first value</typeparam>
        /// <typeparam name="T2">The Type of the second value</typeparam>    
        public struct KeyValueTuple<TKey, T1, T2>
        {
            readonly TKey key; readonly T1 val1; readonly T2 val2;

            /// <summary>
            /// The key for the tuple
            /// </summary>
            public TKey Key
            {
                get { return key; }
            }
            /// <summary>
            /// The first value
            /// </summary>
            public T1 Value1
            {
                get { return val1; }
            }
            /// <summary>
            /// The second value
            /// </summary>
            public T2 Value2
            {
                get { return val2; }
            }
            /// <summary>
            /// Creates a new tuple with the given key and values
            /// </summary>
            public KeyValueTuple(TKey key, T1 value1, T2 value2)
            {
                this.key = key;
                this.val1 = value1;
                this.val2 = value2;
            }
        }
        /// <summary>
        /// Generic tuple for a key and a trio of values
        /// </summary>
        /// <typeparam name="TKey">The Type of the key</typeparam>
        /// <typeparam name="T1">The Type of the first value</typeparam>
        /// <typeparam name="T2">The Type of the second value</typeparam>
        /// <typeparam name="T3">The Type of the third value</typeparam>
        public struct KeyValueTuple<TKey, T1, T2, T3>
        {
            readonly TKey key; readonly T1 val1; readonly T2 val2; readonly T3 val3;

            /// <summary>
            /// The key for the tuple
            /// </summary>
            public TKey Key
            {
                get { return key; }
            }
            /// <summary>
            /// The first value
            /// </summary>
            public T1 Value1
            {
                get { return val1; }
            }
            /// <summary>
            /// The second value
            /// </summary>
            public T2 Value2
            {
                get { return val2; }
            }
            /// <summary>
            /// The third value
            /// </summary>
            public T3 Value3
            {
                get { return val3; }
            }
            /// <summary>
            /// Creates a new tuple with the given key and values
            /// </summary>
            public KeyValueTuple(TKey key, T1 value1, T2 value2, T3 value3)
            {
                this.key = key;
                this.val1 = value1;
                this.val2 = value2;
                this.val3 = value3;
            }
        }
        /// <summary>
        /// Generic tuple for a key and a quartet of values
        /// </summary>
        /// <typeparam name="TKey">The Type of the key</typeparam>
        /// <typeparam name="T1">The Type of the first value</typeparam>
        /// <typeparam name="T2">The Type of the second value</typeparam>
        /// <typeparam name="T3">The Type of the third value</typeparam>
        /// <typeparam name="T4">The Type of the fourth value</typeparam>
        public struct KeyValueTuple<TKey, T1, T2, T3, T4>
        {
            readonly TKey key; readonly T1 val1; readonly T2 val2; readonly T3 val3; readonly T4 val4;

            /// <summary>
            /// The key for the tuple
            /// </summary>
            public TKey Key
            {
                get { return key; }
            }
            /// <summary>
            /// The first value
            /// </summary>
            public T1 Value1
            {
                get { return val1; }
            }
            /// <summary>
            /// The second value
            /// </summary>
            public T2 Value2
            {
                get { return val2; }
            }
            /// <summary>
            /// The third value
            /// </summary>
            public T3 Value3
            {
                get { return val3; }
            }
            /// <summary>
            /// The fourth value
            /// </summary>
            public T4 Value4
            {
                get { return val4; }
            }
            /// <summary>
            /// Creates a new tuple with the given key and values
            /// </summary>
            public KeyValueTuple(TKey key, T1 value1, T2 value2, T3 value3, T4 value4)
            {
                this.key = key;
                this.val1 = value1;
                this.val2 = value2;
                this.val3 = value3;
                this.val4 = value4;
            }
        }
    
}
