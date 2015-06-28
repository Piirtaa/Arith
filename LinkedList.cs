using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Arith
{
    public class LinkedList<T>
    {
        #region Declarations
        private readonly object _stateLock = new object();
        protected LinkedListNode<T> _firstNode = null;
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
        public virtual LinkedListNode<T> LastNode
        {
            get
            {
                LinkedListNode<T> node = this._firstNode;

                while (node != null)
                {
                    if (this.IsLast(node))
                        return node;

                    node = node.NextNode;
                }

                return node;
            }
        }
        public virtual T[] Values
        {
            get
            {
                List<T> list = new List<T>();
                LinkedListNode<T> node = this._firstNode;

                while (node != null)
                {
                    list.Add(node.Value);

                    if (this.IsLast(node))
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
                    if (this.IsLast(node))
                        break;

                    node = node.NextNode;
                }

                return list.ToArray();
            }
        }
        #endregion

        #region Methods
        protected virtual bool IsLast(LinkedListNode<T> node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            return node.NextNode == null;
        }
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
            //build the node
            var node = new LinkedListNode<T>(val, this);

            InsertNode(node, before, after);

            return node;
        }
        public virtual bool AreAdjacent(LinkedListNode<T> before, LinkedListNode<T> after)
        {
            if (before == null)
                return false;

            if (after == null)
                return false;

            if (!object.ReferenceEquals(before.NextNode, after))
                return false;

            if (!object.ReferenceEquals(after.PreviousNode, before))
                return false;

            return true;
        }
        protected virtual void ValidateInsertSlot(LinkedListNode<T> before, LinkedListNode<T> after)
        {
            //validate before node
            if (before != null && !this.Contains(before))
                throw new ArgumentOutOfRangeException("before");

            //validate after node
            if (after != null && !this.Contains(after))
                throw new ArgumentOutOfRangeException("after");

            //validate the before and after nodes are adjacent
            if (before != null && after != null)
            {
                if (!this.AreAdjacent(before, after))
                    throw new ArgumentOutOfRangeException("invalid slot");
            }
        }
        /// <summary>
        /// this is the "gateway" method to appending the list
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
                this.ValidateInsertSlot(before, after);

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

                this.OnInsert_SetFirstNode(node);
            }
            return node;
        }
        /// <summary>
        /// this is called at the end of InsertNode and will set the _firstNode
        /// value based on indications from the inserted node.  There are 3 conditions
        /// where the _firstNode is set: empty list, insertnode.prev = null, insertnode.next = _firstNode
        /// </summary>
        /// <param name="insertNode"></param>
        protected virtual void OnInsert_SetFirstNode(LinkedListNode<T> insertNode)
        {
            if (this._firstNode != null && object.ReferenceEquals(insertNode, this._firstNode))
            {
                throw new ArgumentOutOfRangeException("insertNode");
            }

            if (insertNode.PreviousNode == null)
            {
                //if before is null we assume this is an AddFirst
                this._firstNode = insertNode;
            }
            else if (this._firstNode == null)
            {
                this._firstNode = insertNode;
            }

        }

        public virtual LinkedList<T> Remove(LinkedListNode<T> item)
        {
            //validate it's contained
            if (item != null && !this.Contains(item))
                throw new ArgumentOutOfRangeException("item");

            var before = item.PreviousNode;
            var after = item.NextNode;

            //link before and after
            if (before != null)
            {
                before.NextNode = after;
            }
            if (after != null)
            {
                after.PreviousNode = before;
            }

            if (item.IsFirst && item.IsLast)
            {
                this._firstNode = null;
            }
            else
            {
                //reset first node if we are removing it
                if (object.ReferenceEquals(item, this._firstNode))
                {
                    if (before != null)
                    {
                        this._firstNode = before;
                    }
                    else if (after != null)
                    {
                        this._firstNode = after;
                    }
                    else if (after == null && before == null)
                    {
                        this._firstNode = null;
                    }
                }
            }
            return this;
        }
        #endregion
    }


    /// <summary>
    /// a linked list node
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
                if(vals != null && vals.Length > 1) 
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
