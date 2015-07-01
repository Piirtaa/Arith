using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Arith
{
    [DebuggerDisplay("")]
    public class LinkedList<T>
    {
        #region Declarations
        protected enum InsertSlotEnum { First, Middle, Last, FirstAndLast }

        protected readonly object _stateLock = new object();
        protected LinkedListNode<T> _firstNode = null;

        //we keep a reference to last digit because it's very expensive to walk the list every time
        protected LinkedListNode<T> _lastNode = null;
        #endregion

        #region Ctor
        public LinkedList(params T[] items)
        {
            if (items == null)
                return;

            foreach (var each in items)
                this.AddLast(each);
        }
        #endregion

        #region Calculated Properties
        public bool IsEmpty { get { return this._firstNode == null; } }
        public LinkedListNode<T> FirstNode { get { return _firstNode; } }
        public LinkedListNode<T> LastNode { get { return _lastNode; } }
        public virtual T[] Values
        {
            get
            {
                List<T> list = new List<T>();
                LinkedListNode<T> node = this._firstNode;

                while (node != null)
                {
                    list.Add(node.Value);

                    if (node.IsLast)
                        break;

                    node = node.NextNode;
                }

                return list.ToArray();
            }
        }
        public virtual LinkedListNode<T>[] Nodes
        {
            get
            {
                List<LinkedListNode<T>> list = new List<LinkedListNode<T>>();
                LinkedListNode<T> node = this._firstNode;

                while (node != null)
                {
                    list.Add(node);
                    if (node.IsLast)
                        break;

                    node = node.NextNode;
                }

                return list.ToArray();
            }
        }
        #endregion

        #region Methods
        public LinkedListNode<T> Filter(Func<LinkedListNode<T>, bool> filter)
        {
            if (this._firstNode == null || filter == null)
                return null;

            LinkedListNode<T> node = this._firstNode;
            while (!filter(node))
            {
                if (node.IsLast)
                    return null;

                node = node.NextNode;
            }

            return node;
        }
        /// <summary>
        /// does any node on the linked list have the provided value
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool Contains(T val)
        {
            if (this._firstNode == null)
                return false;

            LinkedListNode<T> node = this._firstNode;
            while (!node.Value.Equals(val))
            {
                node = node.NextNode;
                if (node == null)
                    return false;
            }

            return true;
        }
        public bool Contains(LinkedListNode<T> item)
        {
            if (this._firstNode == null)
                return false;

            LinkedListNode<T> node = this._firstNode;
            while (!object.ReferenceEquals(node, item))
            {
                node = node.NextNode;
                if (node == null)
                    return false;
            }

            return true;
        }
        public virtual LinkedListNode<T> AddFirst(T val)
        {
            return this.InsertNode(new LinkedListNode<T>(val, this), null, this._firstNode);
        }
        public virtual LinkedListNode<T> AddLast(T val)
        {
            return this.InsertNode(new LinkedListNode<T>(val, this), this.LastNode, null);
        }
        public LinkedListNode<T> Insert(T val, LinkedListNode<T> before, LinkedListNode<T> after)
        {
            return this.InsertNode(new LinkedListNode<T>(val, this), before, after);
        }

        public virtual LinkedList<T> Remove(LinkedListNode<T> item)
        {
            lock(this._stateLock)
            {
                //validate it's contained
                if (item != null && !this.Contains(item))
                    throw new ArgumentOutOfRangeException("item");

                //grab the window nodes
                var before = item.PreviousNode;
                var after = item.NextNode;

                //wire them to each other
                if (before != null)
                {
                    before.NextNode = after;
                }
                if (after != null)
                {
                    after.PreviousNode = before;
                }

                //reset first and last pointers
                if (item.IsFirst && item.IsLast)
                {
                    this._firstNode = null;
                    this._lastNode = null; 
                }
                else if (item.IsFirst)
                {
                    this._firstNode = after;
                }
                else if (item.IsLast)
                {
                    this._lastNode = before;
                }
            }
            return this;
        }
        public virtual bool AreAdjacent(LinkedListNode<T> before, LinkedListNode<T> after)
        {
            if (before == null && after == null)
            {
                return false;
            }

            //if the before node is null, 
            if (before == null)
            {
                return after.PreviousNode == null;
            }

            if (after == null)
            {
                return before.NextNode == null;
            }

            if (!object.ReferenceEquals(before.NextNode, after))
                return false;

            if (!object.ReferenceEquals(after.PreviousNode, before))
                return false;

            return true;
        }
        #endregion

        #region Helpers
        protected LinkedListNode<T> GetLastByWalkingList()
        {
            LinkedListNode<T> node = this._firstNode;

            while (node != null)
            {
                node = node.NextNode;
            }

            return node;
        }
        /// <summary>
        /// validates the nodes are adjacent and returns the position we're inserting at as InsertSlot enum
        /// </summary>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        protected InsertSlotEnum ValidateInsertSlot(LinkedListNode<T> before, LinkedListNode<T> after)
        {
            //validate before node
            if (before != null && !this.Contains(before))
                throw new ArgumentOutOfRangeException("before");

            //validate after node
            if (after != null && !this.Contains(after))
                throw new ArgumentOutOfRangeException("after");

            InsertSlotEnum rv = InsertSlotEnum.Middle;

            if (before == null)
            {
                rv = InsertSlotEnum.First;

                //it's a first item insert
                if (after == null)
                {
                    rv = InsertSlotEnum.FirstAndLast;
                    return rv;
                }

                //validate the after is the current first
                if (!after.IsFirst)
                    throw new ArgumentOutOfRangeException("invalid slot");

                return rv;
            }
            else if (after == null)
            {
                rv = InsertSlotEnum.Last;

                //validate the after is the current first
                if (!before.IsLast)
                    throw new ArgumentOutOfRangeException("invalid slot");

                return rv;
            }
            else
            {
                if (!this.AreAdjacent(before, after))
                    throw new ArgumentOutOfRangeException("invalid slot");
            }
            return rv;
        }

        /// <summary>
        /// this is the "gateway" method to appending the list.  To insert first, before should be null,
        /// to insert last, after should be null.  
        /// </summary>
        /// <param name="node"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        protected virtual LinkedListNode<T> InsertNode(LinkedListNode<T> node, LinkedListNode<T> before, LinkedListNode<T> after)
        {
            if (node == null)
                return null;

            lock (this._stateLock)
            {
                var slotType = this.ValidateInsertSlot(before, after);

                switch (slotType)
                {
                    case InsertSlotEnum.First:
                        node.NextNode = this._firstNode;
                        this._firstNode = node;
                        node.NextNode.PreviousNode = node;
                        break;
                    case InsertSlotEnum.FirstAndLast:
                        node.NextNode = null;
                        node.PreviousNode = null;
                        this._firstNode = node;
                        this._lastNode = node;
                        break;
                    case InsertSlotEnum.Last:
                        node.PreviousNode = this._lastNode;
                        this._lastNode = node;
                        node.PreviousNode.NextNode = node;
                        break;
                    case InsertSlotEnum.Middle:
                        node.PreviousNode = before;
                        if (before != null)
                        {
                            before.NextNode = node;
                        }
                        node.NextNode = after;
                        if (after != null)
                        {
                            after.PreviousNode = node;
                        }
                        break;
                }
            }
            return node;
        }
        #endregion
    }


    /// <summary>
    /// a linked list node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 
    [DebuggerDisplay("{Value}")]
    public class LinkedListNode<T>
    {
        #region Declarations

        #endregion

        #region Ctor
        protected internal LinkedListNode(T value, LinkedList<T> parentList)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            this.Value = value;
            this.NextNode = null;
            this.PreviousNode = null;

            if (parentList == null)
                throw new ArgumentNullException("parentList");

            this.ParentList = parentList;

        }
        #endregion

        #region Properties
        public T Value { get; protected set; }
        public LinkedListNode<T> NextNode { get; protected internal set; }
        public LinkedListNode<T> PreviousNode { get; protected internal set; }
        public LinkedList<T> ParentList { get; protected set; }
        #endregion


        #region Calculated Properties
        public bool IsFirst
        {
            get
            {
                return object.ReferenceEquals(this.ParentList.FirstNode, this);
            }
        }
        public bool IsLast
        {
            get
            {
                return object.ReferenceEquals(this.ParentList.LastNode, this);
            }
        }
        #endregion
    }

    internal class LinkedListTests
    {
        internal static void Test()
        {
            LinkedList<int> listOfInt = new LinkedList<int>();
            for (int i = 0; i < 100; i++)
            {
                listOfInt.AddLast(i);
                Debug.Assert(listOfInt.Contains(i));
                Debug.Assert(listOfInt.LastNode.Value == i);
                Debug.Assert(listOfInt.FirstNode.Value == 0);
                var vals = listOfInt.Values;
                if (vals != null && vals.Length > 1)
                {
                    Debug.Assert(listOfInt.LastNode.PreviousNode.Value == i - 1);
                    Debug.Assert(listOfInt.AreAdjacent(listOfInt.LastNode.PreviousNode, listOfInt.LastNode));
                    Debug.Assert(listOfInt.LastNode.NextNode == null);
                }
            }

            for (int i = 0; i > -100; i--)
            {
                var node = listOfInt.AddFirst(i);
                Debug.Assert(listOfInt.FirstNode.Value == i);
                Debug.Assert(listOfInt.AreAdjacent(listOfInt.FirstNode, node.NextNode));
            }


            while (listOfInt.IsEmpty == false)
            {
                var last = listOfInt.LastNode;
                listOfInt.Remove(last);

                if (!listOfInt.IsEmpty)
                {
                    Debug.Assert(listOfInt.LastNode.NextNode == null);
                    Debug.Assert(object.ReferenceEquals(last.PreviousNode, listOfInt.LastNode));
                }
            }


        }
    }
}
