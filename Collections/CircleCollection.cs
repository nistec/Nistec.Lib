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
using System.Text;

namespace Nistec.Collections
{
    /// <summary>
    /// Circle collection. Elements will be circled clockwise.
    /// </summary>
    public class CircleCollection<T>
    {
        private List<T> m_Items = null;
        private int     m_Index  = 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CircleCollection()
        {
            m_Items = new List<T>();
        }


        #region methd Add

        /// <summary>
        /// Adds specified items to the collection.
        /// </summary>
        /// <param name="items">Items to add.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>items</b> is null.</exception>
        public void Add(T[] items)
        {
            if(items == null){
                throw new ArgumentNullException("items");
            }

            foreach(T item in items){
                Add(item);
            }
        }

        /// <summary>
        /// Adds specified item to the collection.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>item</b> is null.</exception>
        public void Add(T item)
        {
            if(item == null){
                throw new ArgumentNullException("item");
            }

            m_Items.Add(item);

            // Reset loop index.
            m_Index = 0;
        }

        #endregion
        
        #region method Remove

        /// <summary>
        /// Removes specified item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>item</b> is null.</exception>
        public void Remove(T item)
        {
            if(item == null){
                throw new ArgumentNullException("item");
            }

            m_Items.Remove(item);

            // Reset loop index.
            m_Index = 0;
        }

        #endregion
        
        #region method Clear

        /// <summary>
        /// Clears all items from collection.
        /// </summary>
        public void Clear()
        {
            m_Items.Clear();

            // Reset loop index.
            m_Index = 0;
        }

        #endregion

        #region method Contains

        /// <summary>
        /// Gets if the collection contain the specified item.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>Returns true if the collection contain the specified item, otherwise false.</returns>
        public bool Contains(T item)
        {
            return m_Items.Contains(item);
        }

        #endregion

        #region method Next

        /// <summary>
        /// Gets next item from the collection. This method is thread-safe.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is raised when thre is no items in the collection.</exception>
        public T Next()
        {
            if(m_Items.Count == 0){
                throw new InvalidOperationException("There is no items in the collection.");
            }

            lock(m_Items){
                T item = m_Items[m_Index];

                m_Index++;
                if(m_Index >= m_Items.Count){
                    m_Index = 0;
                }

                return item;
            }
        }

        #endregion

        #region method ToArray

        /// <summary>
        /// Copies all elements to new array, all elements will be in order they added. This method is thread-safe.
        /// </summary>
        /// <returns>Returns elements in a new array.</returns>
        public T[] ToArray()
        {
            lock(m_Items){
                return m_Items.ToArray();
            }
        }

        #endregion

        #region method ToCurrentOrderArray

        /// <summary>
        /// Copies all elements to new array, all elements will be in current circle order. This method is thread-safe.
        /// </summary>
        /// <returns>Returns elements in a new array.</returns>
        public T[] ToCurrentOrderArray()
        {
            lock(m_Items){
                int index  = m_Index;
                T[] retVal = new T[m_Items.Count];
                for(int i=0;i<m_Items.Count;i++){
                    retVal[i] = m_Items[index];

                    index++;
                    if(index >= m_Items.Count){
                        index = 0;
                    }
                }

                return retVal;
            }
        }

        #endregion

        #region Properties Implementation

        /// <summary>
        /// Gets number of items in the collection.
        /// </summary>
        public int Count
        {
            get{ return m_Items.Count; }
        }

        /// <summary>
        /// Gets item at the specified index.
        /// </summary>
        /// <param name="index">Item zero based index.</param>
        /// <returns>Returns item at the specified index.</returns>
        public T this[int index]
        {
            get{ return m_Items[index]; }
        }

        #endregion

    }
}
