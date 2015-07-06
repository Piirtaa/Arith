using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Arith.DataStructures
{
    /// <summary>
    /// lookup using 2 strings from the same set as a keys       
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SquareLookup<T>
    {
        #region Declarations
        private readonly object _stateLock = new object();
        private Hashtable _stringToIntMap = new Hashtable();
        private T[,] _values = null;
        #endregion

        #region Ctor
        public SquareLookup(string[] keys)
        {
            foreach (string each in keys)
            {
                if (this._stringToIntMap.ContainsKey(each))
                    throw new ArgumentOutOfRangeException("key taken");

                    this._stringToIntMap.Add(each, this._stringToIntMap.Count);
            }
            
            this._values = new T[this._stringToIntMap.Count, this._stringToIntMap.Count];
        }
        #endregion

        #region Methods
        public void Add(string key1, string key2, T value)
        {
            if (string.IsNullOrEmpty(key1))
                throw new ArgumentOutOfRangeException("key1");

            if (string.IsNullOrEmpty(key2))
                throw new ArgumentOutOfRangeException("key2");


            if (string.IsNullOrEmpty(key1))
                throw new ArgumentOutOfRangeException("key1");

            lock (this._stateLock)
            {
                this._values[(int)this._stringToIntMap[key1], (int)this._stringToIntMap[key2]] = value;
            }
        }
        public T Get(string key1, string key2)
        {
            return this._values[(int)this._stringToIntMap[key1], (int)this._stringToIntMap[key2]];
        }
        #endregion
    }
}
