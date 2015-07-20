using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Arith.Decorating;
using Arith.Extensions;

namespace Arith.DataStructures
{


    [DebuggerDisplay("{DebuggerText}")]
    public class LinkedList<T> : ILinkedList<T>, IHasDebuggerText
    {
        #region Declarations
        protected enum InsertSlotEnum { First, Middle, Last, FirstAndLast }

        protected readonly object _stateLock = new object();
        protected ILinkedListNode<T> _firstNode = null;

        //we keep a reference to last digit because it's very expensive to walk the list every time
        protected ILinkedListNode<T> _lastNode = null;
        #endregion

        #region Ctor
        public LinkedList(params T[] items)
        {
            if (items == null)
                return;

            //define the default node building strategy
            this.NodeBuildingStrategy = (x) =>
            {
                return new LinkedListNode<T>(x, this);
            };

            foreach (var each in items)
                this.AddLast(each);
        }
        #endregion

        #region Static Builders
        public static LinkedList<T> New(params T[] items)
        {
            return new LinkedList<T>(items);
        }
        #endregion

        #region Calculated Properties
        public virtual T[] Values
        {
            get
            {
                List<T> list = new List<T>();
                ILinkedListNode<T> node = this.FirstNode;

                while (node != null)
                {
                    list.Add(node.Value);

                    if (node.IsLast())
                        break;

                    node = node.NextNode;
                }

                return list.ToArray();
            }
        }
        public virtual ILinkedListNode<T>[] Nodes
        {
            get
            {
                List<ILinkedListNode<T>> list = new List<ILinkedListNode<T>>();
                ILinkedListNode<T> node = this.FirstNode;

                while (node != null)
                {
                    list.Add(node);

                    if (node.IsLast())
                        break;

                    node = node.NextNode;
                }

                return list.ToArray();
            }
        }
        #endregion

        #region IHasDebuggerText
        /// <summary>
        /// used by DebuggerDisplay attribute
        /// </summary>
        public string DebuggerText
        {
            get
            {
                var vals = this.Values;
                var list = vals.ProjectList((x) =>
                {
                    if (x is IHasDebuggerText)
                    {
                        return (x as IHasDebuggerText).DebuggerText;
                    }
                    return x;
                });
                return string.Join(",", list);
            }
        }
        #endregion

        #region ILinkedList
        /// <summary>
        /// override/replace this strategy if we want anything other than a new LinkedListNode 
        /// </summary>
        public Func<T, ILinkedListNode<T>> NodeBuildingStrategy { get; set; }
        public ILinkedListNode<T> FirstNode { get { return _firstNode; } }
        public ILinkedListNode<T> LastNode { get { return _lastNode; } }
        /// <summary>
        /// does any node on the linked list have the provided value
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool Contains(T val)
        {
            var match = this.Filter((x) =>
            {
                return x.Value.Equals(val);
            });

            return match != null;
        }
        public virtual bool Contains(ILinkedListNode<T> item)
        {
            var match = this.Filter((x) =>
            {
                return object.ReferenceEquals(x, item);
            });

            return match != null;
        }

        /// <summary>
        /// this is the "gateway" method to appending the list.  To insert first, before should be null,
        /// to insert last, after should be null.  
        /// </summary>
        /// <param name="node"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public virtual ILinkedListNode<T> InsertNode(ILinkedListNode<T> node, ILinkedListNode<T> before, ILinkedListNode<T> after)
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

        public virtual ILinkedList<T> Remove(ILinkedListNode<T> item)
        {
            lock (this._stateLock)
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
                if (item.IsFirst() && item.IsLast())
                {
                    this._firstNode = null;
                    this._lastNode = null;
                }
                else if (item.IsFirst())
                {
                    this._firstNode = after;
                }
                else if (item.IsLast())
                {
                    this._lastNode = before;
                }
            }
            return this;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// validates the nodes are adjacent and returns the position we're inserting at as InsertSlot enum
        /// </summary>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        protected InsertSlotEnum ValidateInsertSlot(ILinkedListNode<T> before, ILinkedListNode<T> after)
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
                if (!after.IsFirst())
                    throw new ArgumentOutOfRangeException("invalid slot");

                return rv;
            }
            else if (after == null)
            {
                rv = InsertSlotEnum.Last;

                //validate the after is the current first
                if (!before.IsLast())
                    throw new ArgumentOutOfRangeException("invalid slot");

                return rv;
            }
            else
            {
                if (!before.IsPreceding(after))
                    throw new ArgumentOutOfRangeException("invalid slot");
            }
            return rv;
        }

        #endregion
    }


    /// <summary>
    /// a linked list node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{Value}")]
    public class LinkedListNode<T> : ILinkedListNode<T>
    {
        #region Declarations

        #endregion

        #region Ctor
        public LinkedListNode(T value, ILinkedList<T> parentList)
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

        #region Static Builders
        public static LinkedListNode<T> New(T value, ILinkedList<T> parentList)
        {
            return new LinkedListNode<T>(value, parentList);
        }
        #endregion

        #region Properties
        public T Value { get; protected set; }
        public ILinkedListNode<T> NextNode { get; set; }
        public ILinkedListNode<T> PreviousNode { get; set; }
        public ILinkedList<T> ParentList { get; protected set; }
        #endregion

    }

    public class LinkedListTests
    {
        public static void Test()
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
                    Debug.Assert(listOfInt.LastNode.PreviousNode.IsPreceding(listOfInt.LastNode));
                    Debug.Assert(listOfInt.LastNode.NextNode == null);
                }
            }

            for (int i = 0; i > -100; i--)
            {
                var node = listOfInt.AddFirst(i);
                Debug.Assert(listOfInt.FirstNode.Value == i);
                Debug.Assert(listOfInt.FirstNode.IsPreceding(node.NextNode));
            }


            while (listOfInt.IsEmpty() == false)
            {
                var last = listOfInt.LastNode;
                listOfInt.Remove(last);

                if (!listOfInt.IsEmpty())
                {
                    Debug.Assert(listOfInt.LastNode.NextNode == null);
                    Debug.Assert(object.ReferenceEquals(last.PreviousNode, listOfInt.LastNode));
                }
            }


        }
    }
}
