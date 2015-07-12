using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Arith.DataStructures.Decorations;

namespace Arith.DataStructures
{
    /// <summary>
    /// a linked list that links the last and first items in a loop
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularLinkedList<T> : CircularLinkedListDecoration<T>, ICircularLinkedList<T>
    {
        #region Ctor
        public CircularLinkedList(params T[] items)
            : base(LinkedList<T>.New(items))
        {

        }
        #endregion
    }



}
