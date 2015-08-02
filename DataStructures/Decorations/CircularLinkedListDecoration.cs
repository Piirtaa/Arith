using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Decorating;
using System.Runtime.Serialization;
using Arith.DataStructures;
using System.Diagnostics;

namespace Arith.DataStructures.Decorations
{
    /// <summary>
    /// nodes in a circular linked list have a few more navigation options
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICircularLinkedListNode<T> : ILinkedListNode<T>
    {
        /// <summary>
        /// moves to the next node.  if move goes to first node, returns true for rollover
        /// </summary>
        /// <param name="rollover"></param>
        /// <returns></returns>
        bool MoveForward(out ICircularLinkedListNode<T> next);

        /// <summary>
        /// moves to the previous node.  if move goes to last node, returns true for rollover
        /// </summary>
        /// <param name="rollover"></param>
        /// <returns></returns>
        bool MoveBack(out ICircularLinkedListNode<T> next);

        /// <summary>
        /// returns the node that is the same number of steps forward from First 
        /// that this instance is, but backwards from First
        /// </summary>
        /// <returns></returns>
        ICircularLinkedListNode<T> GetListComplement();
        bool MoveForwardBy(T val, out ICircularLinkedListNode<T> next);
        bool MoveBackBy(T val, out ICircularLinkedListNode<T> next);
    }

    /// <summary>
    /// provides circularity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICircularLinkedList<T> : ILinkedListDecoration<T>
    {
    }

    public class CircularLinkedListDecoration<T> : LinkedListDecorationBase<T>,
        ICircularLinkedList<T>,
        IHasA<IHasLinkedListHooking<T>>,
        IHasA<IHasNodeBuilding<T>>
    {
        #region Declarations
        private readonly object _stateLock = new object();

        protected bool _isInserting = false;
        protected bool _isMutating = false;
        #endregion

        #region Ctor
        public CircularLinkedListDecoration(object decorated, string cakeName = null)
            : base(decorated, cakeName)
        {

            this.OuterNodeBuildingList.NodeBuildingStrategy = (x, list) =>
            {
                ICircularLinkedList<T> clist = list as ICircularLinkedList<T>;
                return new CircularLinkedListNode<T>(x, clist);
            };

            //notice i'm chaining with whatever existing strategy (+= operator instead of =)
            this.OuterHookingList.PostNodeInsertionStrategy += (x) =>
            {
                //if we have circularity issues (ie. we're on the first or last node) then we work that out
                if (this.FirstNode != null)
                {
                    this.FirstNode.PreviousNode = this.LastNode;
                    this.LastNode.NextNode = this.FirstNode;
                }
            };
        }
        #endregion

        #region Static
        public static CircularLinkedListDecoration<T> New(object decorated, string cakeName = null)
        {
            return new CircularLinkedListDecoration<T>(decorated, cakeName);
        }
        #endregion

        #region ISerializable
        protected CircularLinkedListDecoration(SerializationInfo info, StreamingContext context)
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
            return new CircularLinkedListDecoration<T>(thing, this.CakeName);
        }
        #endregion

        #region Properties

        public HookedMutableLinkedListDecoration<T> OuterHookingList
        {
            get { return this.As<HookedMutableLinkedListDecoration<T>>(false); }
        }
        public NodeBuildingLinkedListDecoration<T> OuterNodeBuildingList
        {
            get { return this.As<NodeBuildingLinkedListDecoration<T>>(false); }
        }

        #endregion

    }

    public static class CircularLinkedListDecorationExtensions
    {
        public static void DoWhileCircular<T>(this ILinkedList<T> thing,
    Action<CircularLinkedListDecoration<T>> action)
        {
            var decorated = CircularLinkedListDecoration<T>.New(thing);
            action(decorated);
            decorated.Undecorate();
        }
        public static CircularLinkedListDecoration<T> HasCircularity<T>(this ILinkedList<T> thing,
            string cakeName = null)
        {
            return CircularLinkedListDecoration<T>.New(thing,
                cakeName);
        }
        public static CircularLinkedListDecoration<T> GetCircularityCake<T>(this ILinkedList<T> thing,
    string cakeName = null)
        {
            var rv = thing.GetMutabilityCake(cakeName).HasNodeBuilding().
                HasHooks().HasCircularity(cakeName);
            return rv;
        }
    }

    /// <summary>
    /// a circular linked list node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{Value}")]
    public class CircularLinkedListNode<T> : LinkedListNode<T>, ICircularLinkedListNode<T>
    {
        #region Ctor
        public CircularLinkedListNode(T value, ICircularLinkedList<T> parentList)
            : base(value, parentList)
        {
        }
        #endregion

        #region ICircularLinkedListNode
        /// <summary>
        /// moves to the next node.  if move goes to first node, returns true for rollover
        /// </summary>
        /// <param name="rollover"></param>
        /// <returns></returns>
        public bool MoveForward(out ICircularLinkedListNode<T> next)
        {

            next = this.NextNode as ICircularLinkedListNode<T>;
            return next.IsFirst();
        }
        /// <summary>
        /// moves to the previous node.  if move goes to last node, returns true for rollover
        /// </summary>
        /// <param name="rollover"></param>
        /// <returns></returns>
        public bool MoveBack(out ICircularLinkedListNode<T> next)
        {
            next = this.PreviousNode as ICircularLinkedListNode<T>;
            return next.IsLast();
        }
        /// <summary>
        /// returns the node that is the same number of steps forward from First 
        /// that this instance is, but backwards from First
        /// </summary>
        /// <returns></returns>
        public ICircularLinkedListNode<T> GetListComplement()
        {
            ICircularLinkedListNode<T> rv = this.ParentList.FirstNode as ICircularLinkedListNode<T>;
            ICircularLinkedListNode<T> counterNode = this.ParentList.FirstNode as ICircularLinkedListNode<T>;

            while (!counterNode.NodeValue.Equals(this.NodeValue))
            {
                //if we've iterated thru the whole list and not found the value then it's a bad value
                //rather than validate the value at the start of the method, which would do a list scan also
                if (counterNode.IsLast())
                    throw new ArgumentOutOfRangeException("invalid value");

                counterNode = counterNode.NextNode as ICircularLinkedListNode<T>;
                rv = rv.PreviousNode as ICircularLinkedListNode<T>;
            }
            return rv;
        }
        /// <summary>
        /// moves forward by the specified amount.  returns true for rollover.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool MoveForwardBy(T val, out ICircularLinkedListNode<T> next)
        {
            ICircularLinkedListNode<T> counterNode = this.ParentList.FirstNode as ICircularLinkedListNode<T>;
            ICircularLinkedListNode<T> nextOUT = this;
            bool rollover = false;

            while (!counterNode.NodeValue.Equals(val))
            {
                //if we've iterated thru the whole list and not found the value then it's a bad value
                //rather than validate the value at the start of the method, which would do a list scan also
                if (counterNode.IsLast())
                    throw new ArgumentOutOfRangeException("invalid value");

                counterNode = counterNode.NextNode as ICircularLinkedListNode<T>;
                var rollover2 = nextOUT.MoveForward(out nextOUT);
                if (rollover2 && !rollover)
                    rollover = true;
            }

            next = nextOUT;
            return rollover;
        }
        /// <summary>
        /// moves backwards by the specified amount.  returns true for rollover
        /// </summary>
        /// <param name="val"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool MoveBackBy(T val, out ICircularLinkedListNode<T> next)
        {
            ICircularLinkedListNode<T> counterNode = this.ParentList.FirstNode as ICircularLinkedListNode<T>;
            ICircularLinkedListNode<T> nextOUT = this;
            bool rollover = false;

            while (!counterNode.NodeValue.Equals(val))
            {
                //if we've iterated thru the whole list and not found the value then it's a bad value
                //rather than validate the value at the start of the method, which would do a list scan also
                if (counterNode.IsLast())
                    throw new ArgumentOutOfRangeException("invalid value");

                counterNode = counterNode.NextNode as ICircularLinkedListNode<T>;
                var rollover2 = nextOUT.MoveBack(out nextOUT);
                if (rollover2 && !rollover)
                    rollover = true;
            }

            next = nextOUT;
            return rollover;
        }
        #endregion

        #region Methods
        public ICircularLinkedListNode<T> FindNodeByValue(T val)
        {
            ICircularLinkedListNode<T> counterNode = this.ParentList.FirstNode as ICircularLinkedListNode<T>;

            while (!counterNode.NodeValue.Equals(val))
            {
                //if we've iterated thru the whole list and not found the value then it's a bad value
                //rather than validate the value at the start of the method, which would do a list scan also
                if (counterNode.IsLast())
                    throw new ArgumentOutOfRangeException("invalid value");

                counterNode = counterNode.NextNode as ICircularLinkedListNode<T>;
            }

            return counterNode;
        }
        #endregion
    }

    public class CircularLinkedListTests
    {
        public static void Test()
        {
            var listOfInt = new LinkedList<int>().GetCircularityCake();
            for (int i = 0; i < 100; i++)
            {
                var node = listOfInt.OuterNodeBuildingList.AddLast(i);
                Debug.Assert(listOfInt.Contains(i));
                Debug.Assert(listOfInt.LastNode.NodeValue == i);
                Debug.Assert(listOfInt.FirstNode.NodeValue == 0);
                var vals = (listOfInt.Inner as LinkedList<int>).Values;
                if (vals != null && vals.Length > 1)
                {
                    Debug.Assert(listOfInt.LastNode.PreviousNode.NodeValue == i - 1);
                    Debug.Assert(listOfInt.LastNode.PreviousNode.IsPreceding(listOfInt.LastNode));
                    Debug.Assert(listOfInt.LastNode.IsPreceding(listOfInt.FirstNode));
                }

                var cnode = node as ICircularLinkedListNode<int>;
                //move cursor back
                if (!cnode.MoveBack(out cnode))
                    Debug.Assert(cnode.NodeValue == i - 1);
                else
                    Debug.Assert(cnode.NodeValue == 0);

                //move it forward
                if (!cnode.MoveForward(out cnode))
                    Debug.Assert(cnode.NodeValue == i);
                else
                    Debug.Assert(cnode.NodeValue == 0);

                if (!cnode.IsFirst())
                {
                    //move it all the way back
                    if (!cnode.MoveBackBy(i, out cnode))
                        Debug.Assert(cnode.NodeValue == 0);

                    //move it all the way forward
                    if (!cnode.MoveForwardBy(i, out cnode))
                        Debug.Assert(cnode.NodeValue == i);
                }
            }

            for (int i = 0; i > -100; i--)
            {
                var node = listOfInt.OuterNodeBuildingList.AddFirst(i);
                Debug.Assert(listOfInt.FirstNode.NodeValue == i);
                Debug.Assert(listOfInt.FirstNode.IsPreceding(node.NextNode));
            }

            while (listOfInt.InnerList.IsEmpty() == false)
            {
                var first = listOfInt.FirstNode;

                var last = listOfInt.LastNode;
                listOfInt.OuterHookingList.Remove(last);

                if (!listOfInt.InnerList.IsEmpty())
                {
                    Debug.Assert(listOfInt.LastNode.NextNode == listOfInt.FirstNode);
                    Debug.Assert(object.ReferenceEquals(last.PreviousNode, listOfInt.LastNode));
                }
            }

        }

        public static void SequenceTest()
        {
            var listOfInt = new LinkedList<int>().GetCircularityCake<int>(); ;
            for (int i = 0; i < 10; i++)
            {
                listOfInt.OuterNodeBuildingList.AddLast(i);
            }
            ICircularLinkedListNode<int> node = listOfInt.FirstNode as ICircularLinkedListNode<int>;
            Debug.WriteLine("forward sequence");
            for (int i = 0; i < 21; i++)
            {
                Debug.Write(node.NodeValue + ",");
                node.MoveForward(out node);
            }
            Debug.WriteLine("");
            Debug.WriteLine("backward sequence");
            for (int i = 0; i < 21; i++)
            {
                Debug.Write(node.NodeValue + ",");
                node.MoveBack(out node);
            }


        }
    }

}
