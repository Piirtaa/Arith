using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Decorating;
using System.Runtime.Serialization;
using Arith.DataStructures;
using System.Diagnostics;
using Arith.Extensions;

namespace Arith.DataStructures.Decorations
{
    /// <summary>
    /// gives a list mutability
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHasLinkedListMutability<T> : ILinkedListDecoration<T>
    {
        ILinkedListNode<T> InsertNode(ILinkedListNode<T> node, ILinkedListNode<T> before, ILinkedListNode<T> after);
        ILinkedList<T> Remove(ILinkedListNode<T> item);
    }

    public class MutableLinkedListDecoration<T> : LinkedListDecorationBase<T>,
        IHasLinkedListMutability<T>
    {
        #region Declarations
        private readonly object _stateLock = new object();
        protected enum InsertSlotEnum { First, Middle, Last, FirstAndLast }
        #endregion

        #region Ctor
        public MutableLinkedListDecoration(object decorated,
            string cakeName = null)
            : base(decorated, cakeName)
        {

        }
        #endregion

        #region Static
        public static MutableLinkedListDecoration<T> New(object decorated,
            string cakeName = null)
        {
            return new MutableLinkedListDecoration<T>(decorated,
                cakeName);
        }
        #endregion

        #region ISerializable
        protected MutableLinkedListDecoration(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        /// <summary>
        /// since we don't want to expose ISerializable concerns publicly, we use a virtual protected
        /// helper function that does the actual implementation of ISerializable, and is called by the
        /// explicit interface implementation of GetObjectData.  This is the method to be overridden in 
        /// derived classes.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected override void ISerializable_GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.ISerializable_GetObjectData(info, context);
        }
        #endregion

        #region Overrides
        public override IDecoration ApplyThisDecorationTo(object thing)
        {
            return new MutableLinkedListDecoration<T>(thing, 
                this.CakeName);
        }
        #endregion

        #region Methods

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
                        var oldFirst = this.FirstNode;
                        this.InnerList.FirstNode = node;
                        this.FirstNode.NextNode = oldFirst;
                        this.FirstNode.PreviousNode = null;
                        oldFirst.PreviousNode = node;
                        break;
                    case InsertSlotEnum.FirstAndLast:
                        this.InnerList.FirstNode = node;
                        this.InnerList.LastNode = node;
                        this.FirstNode.PreviousNode = null;
                        this.FirstNode.NextNode = null;
                        break;
                    case InsertSlotEnum.Last:
                        var oldLast = this.LastNode;
                        this.InnerList.LastNode = node;
                        this.LastNode.PreviousNode = oldLast;
                        this.LastNode.NextNode = null;
                        oldLast.NextNode = node;
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
                    this.InnerList.FirstNode = null;
                    this.InnerList.LastNode = null;
                }
                else if (item.IsFirst())
                {
                    this.InnerList.FirstNode = after;
                }
                else if (item.IsLast())
                {
                    this.InnerList.LastNode = before;
                }
            }
            return this;
        }
        #endregion

        #region Helpers
        public void RemoveLast()
        {
            if (this.LastNode != null)
                this.Remove(this.LastNode);
        }
        public void RemoveFirst()
        {
            if (this.FirstNode != null)
                this.Remove(this.FirstNode);
        }
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

    public static class MutableLinkedListDecorationExtensions
    {
        public static void DoWhileMutable<T>(this ILinkedList<T> thing,
            Action<MutableLinkedListDecoration<T>> action)
        {
            var decorated = MutableLinkedListDecoration<T>.New(thing);
            action(decorated);

            decorated.Undecorate();
        }
        public static MutableLinkedListDecoration<T> HasMutability<T>(this ILinkedList<T> thing,
            string cakeName = null)
        {
                return MutableLinkedListDecoration<T>.New(thing, 
                    cakeName);
        }

        public static MutableLinkedListDecoration<T> GetMutabilityCake<T>(
            this ILinkedList<T> thing,
    string cakeName = null)
        {
            var rv = thing.HasMutability<T>(cakeName);
            return rv;
        }
    }



    public class MutableLinkedListTests
    {
        public static void Test()
        {
            var listOfInt = new LinkedList<int>().GetMutabilityCake<int>();
            for (int i = 0; i < 100; i++)
            {
                LinkedListNode<int> node = LinkedListNode<int>.New(i, listOfInt);

                listOfInt.InsertNode(node, listOfInt.LastNode, null);
                Debug.Assert(listOfInt.Contains(i));
                Debug.Assert(listOfInt.LastNode.NodeValue == i);
                Debug.Assert(listOfInt.FirstNode.NodeValue == 0);
                var vals = listOfInt.InnerList.Values;
                if (vals != null && vals.Length > 1)
                {
                    Debug.Assert(listOfInt.LastNode.PreviousNode.NodeValue == i - 1);
                    Debug.Assert(listOfInt.LastNode.PreviousNode.IsPreceding(listOfInt.LastNode));
                    Debug.Assert(listOfInt.LastNode.NextNode == null);
                }
            }

            for (int i = 0; i > -100; i--)
            {
                LinkedListNode<int> node = LinkedListNode<int>.New(i, listOfInt);

                listOfInt.InsertNode(node, null, listOfInt.FirstNode);
                Debug.Assert(listOfInt.FirstNode.NodeValue == i);
                Debug.Assert(listOfInt.FirstNode.IsPreceding(node.NextNode));
            }


            while (listOfInt.InnerList.IsEmpty() == false)
            {
                var last = listOfInt.LastNode;
                listOfInt.Remove(last);

                if (!listOfInt.InnerList.IsEmpty())
                {
                    Debug.Assert(listOfInt.LastNode.NextNode == null);
                    Debug.Assert(object.ReferenceEquals(last.PreviousNode, listOfInt.LastNode));
                }
            }


        }
    }


}
