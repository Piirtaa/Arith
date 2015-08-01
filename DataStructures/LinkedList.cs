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
        protected readonly object _stateLock = new object();
        protected internal ILinkedListNode<T> _firstNode = null;

        //we keep a reference to last digit because it's very expensive to walk the list every time
        protected internal ILinkedListNode<T> _lastNode = null;
        #endregion

        #region Ctor
        public LinkedList()
        {

        }
        #endregion

        #region Static Builders
        public static LinkedList<T> New()
        {
            return new LinkedList<T>();
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
                    list.Add(node.NodeValue);

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
        public ILinkedListNode<T> FirstNode { get { return _firstNode; } set { this._firstNode = value; } }
        public ILinkedListNode<T> LastNode { get { return _lastNode; } set { this._lastNode = value; } }
       
        /// <summary>
        /// does any node on the linked list have the provided value
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool Contains(T val)
        {
            var match = this.Filter((x) =>
            {
                return x.NodeValue.Equals(val);
            }, true);

            return match != null;
        }
        public virtual bool Contains(ILinkedListNode<T> item)
        {
            var match = this.Filter((x) =>
            {
                return object.ReferenceEquals(x, item);
            }, true);

            return match != null;
        }
        #endregion

        #region Extension-y / Calculated methods

 
        /// <summary>
        /// a method to iterate thru the list either forwards or backwards
        /// </summary>
        /// <param name="action"></param>
        /// <param name="fromFirstToLast"></param>
        public void Iterate(
            Action<ILinkedListNode<T>> action,
            bool fromFirstToLast)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (fromFirstToLast)
            {
                ILinkedListNode<T> node = this.FirstNode;

                while (node != null)
                {
                    action(node);

                    if (node.IsLast())
                        break;

                    node = node.NextNode;
                }
            }
            else
            {
                ILinkedListNode<T> node = this.LastNode;

                while (node != null)
                {
                    action(node);

                    if (node.IsFirst())
                        break;

                    node = node.PreviousNode;
                }
            }
        }

        /// <summary>
        /// performs Iterate, but on 2 lists, such that each are traversed in the same
        /// steps, in parallel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        /// <param name="fromFirstToLast"></param>
        public void ParallelIterate(
            ILinkedList<T> list2,
            Action<ILinkedListNode<T>, ILinkedListNode<T>> action,
            bool fromFirstToLast)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (fromFirstToLast)
            {
                ILinkedListNode<T> node = this.FirstNode;
                ILinkedListNode<T> node2 = list2.FirstNode;

                while (node != null)
                {
                    action(node, node2);

                    if (node.IsLast())
                        break;

                    node = node.NextNode;

                    if (node2 != null)
                        node2 = node2.NextNode;
                }
            }
            else
            {
                ILinkedListNode<T> node = this.LastNode;
                ILinkedListNode<T> node2 = list2.LastNode;

                while (node != null)
                {
                    action(node, node2);

                    if (node.IsFirst())
                        break;

                    node = node.PreviousNode;

                    if (node2 != null)
                        node2 = node2.PreviousNode;
                }
            }
        }


        /// <summary>
        /// iterates from first to last and returns item from a positive filter.
        /// demonstrates good practice for iterating the list
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ILinkedListNode<T> Filter(
            Func<ILinkedListNode<T>, bool> filter,
            bool fromFirstToLast)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            if (fromFirstToLast)
            {
                ILinkedListNode<T> node = this.FirstNode;

                while (node != null)
                {
                    if (filter(node))
                        break;

                    if (node.IsLast())
                    {
                        node = null;
                        break;
                    }
                    node = node.NextNode;
                }
                return node;
            }
            else
            {
                ILinkedListNode<T> node = this.LastNode;

                while (node != null)
                {
                    if (filter(node))
                        break;

                    if (node.IsFirst())
                    {
                        node = null;
                        break;
                    }
                    node = node.PreviousNode;
                }
                return node;
            }
        }
        #endregion


    }


    /// <summary>
    /// a linked list node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{NodeValue}")]
    public class LinkedListNode<T> : ILinkedListNode<T>
    {
        #region Declarations

        #endregion

        #region Ctor
        public LinkedListNode(T value, ILinkedList<T> parentList)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            this.NodeValue = value;
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
        public T NodeValue { get; protected set; }
        public ILinkedListNode<T> NextNode { get; set; }
        public ILinkedListNode<T> PreviousNode { get; set; }
        public ILinkedList<T> ParentList { get; protected set; }
        #endregion

    }


}
