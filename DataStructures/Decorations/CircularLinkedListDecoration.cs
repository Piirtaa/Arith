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
    public interface ICircularLinkedList<T> : ILinkedListDecoration<T>,
                IHasDecoration<IHasHooks<T>>
    {
    }

    public class CircularLinkedListDecoration<T> : LinkedListDecorationBase<T>, 
        ICircularLinkedList<T>
    {
        #region Declarations
        private readonly object _stateLock = new object();

        protected bool _isInserting = false;
        protected bool _isMutating = false;
        #endregion

        #region Ctor
        public CircularLinkedListDecoration(ILinkedList<T> decorated)
            : base(decorated)
        {
             //define the default node building strategy
            this.NodeBuildingStrategy = (x) =>
            {
                return new CircularLinkedListNode<T>(x, this);
            };

            //notice i'm chaining with whatever existing strategy (+= operator instead of =)
            this.PostNodeInsertionStrategy += (x) =>
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
        public static CircularLinkedListDecoration<T> New(ILinkedList<T> decorated)
        {
            return new CircularLinkedListDecoration<T>(decorated);
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
        public override IDecorationOf<ILinkedList<T>> ApplyThisDecorationTo(ILinkedList<T> thing)
        {
            return new CircularLinkedListDecoration<T>(thing);
        }
        #endregion

        #region Properties
        /// <summary>
        /// set if we have any post insert action/scrubs to perform
        /// </summary>
        public Action<ILinkedListNode<T>> PostNodeInsertionStrategy
        {
            get { return this.As<HookedLinkedListDecoration<T>>().PostNodeInsertionStrategy; }
            set { this.As<HookedLinkedListDecoration<T>>().PostNodeInsertionStrategy = value; }
        }
        /// <summary>
        /// if the list changes in any way (inserts or removal) this strategy is run.  happens after postnodeinsert hook
        /// </summary>
        public Action<ILinkedList<T>> PostMutateStrategy
        {
            get { return this.As<HookedLinkedListDecoration<T>>().PostMutateStrategy; }
            set { this.As<HookedLinkedListDecoration<T>>().PostMutateStrategy = value; }
        }
        #endregion

        #region Overrides
        public override ILinkedList<T> Remove(ILinkedListNode<T> item)
        {
            var rv = base.Remove(item);

            return rv;
        }
        public override ILinkedListNode<T> InsertNode(ILinkedListNode<T> node, ILinkedListNode<T> before, ILinkedListNode<T> after)
        {
            var rv = base.InsertNode(node, before, after);
            return rv;
        }
        #endregion
    }

    public static class CircularLinkedListDecorationExtensions
    {
        public static CircularLinkedListDecoration<T> HasCircularity<T>(this ILinkedList<T> thing)
        {
            return CircularLinkedListDecoration<T>.New(thing);
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

            while (!counterNode.Value.Equals(this.Value))
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

            while (!counterNode.Value.Equals(val))
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

            while (!counterNode.Value.Equals(val))
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

            while (!counterNode.Value.Equals(val))
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
            var listOfInt = new LinkedList<int>().HasHooks().HasCircularity();
            for (int i = 0; i < 100; i++)
            {
                var node = listOfInt.AddLast(i);
                Debug.Assert(listOfInt.Contains(i));
                Debug.Assert(listOfInt.LastNode.Value == i);
                Debug.Assert(listOfInt.FirstNode.Value == 0);
                var vals = (listOfInt.Inner as LinkedList<int>).Values;
                if (vals != null && vals.Length > 1)
                {
                    Debug.Assert(listOfInt.LastNode.PreviousNode.Value == i - 1);
                    Debug.Assert(listOfInt.LastNode.PreviousNode.IsPreceding(listOfInt.LastNode));
                    Debug.Assert(listOfInt.LastNode.IsPreceding(listOfInt.FirstNode));
                }

                var cnode = node as ICircularLinkedListNode<int>;
                //move cursor back
                if (!cnode.MoveBack(out cnode))
                    Debug.Assert(cnode.Value == i - 1);
                else
                    Debug.Assert(cnode.Value == 0);

                //move it forward
                if (!cnode.MoveForward(out cnode))
                    Debug.Assert(cnode.Value == i);
                else
                    Debug.Assert(cnode.Value == 0);

                if (!cnode.IsFirst())
                {
                    //move it all the way back
                    if (!cnode.MoveBackBy(i, out cnode))
                        Debug.Assert(cnode.Value == 0);

                    //move it all the way forward
                    if (!cnode.MoveForwardBy(i, out cnode))
                        Debug.Assert(cnode.Value == i);
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
                var first = listOfInt.FirstNode;

                var last = listOfInt.LastNode;
                listOfInt.Remove(last);

                if (!listOfInt.IsEmpty())
                {
                    Debug.Assert(listOfInt.LastNode.NextNode == listOfInt.FirstNode);
                    Debug.Assert(object.ReferenceEquals(last.PreviousNode, listOfInt.LastNode));
                }
            }

        }

        public static void SequenceTest()
        {
            var listOfInt = new LinkedList<int>().HasHooks().HasCircularity();;
            for (int i = 0; i < 10; i++)
            {
                listOfInt.AddLast(i);
            }
            ICircularLinkedListNode<int> node = listOfInt.FirstNode as ICircularLinkedListNode<int>;
            Debug.WriteLine("forward sequence");
            for (int i = 0; i < 21; i++)
            {
                Debug.Write(node.Value + ",");
                node.MoveForward(out node);
            }
            Debug.WriteLine("");
            Debug.WriteLine("backward sequence");
            for (int i = 0; i < 21; i++)
            {
                Debug.Write(node.Value + ",");
                node.MoveBack(out node);
            }


        }
    }

}
