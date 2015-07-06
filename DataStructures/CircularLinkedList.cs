using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Arith.DataStructures
{
    /// <summary>
    /// a linked list that links the last and first items in a loop
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularLinkedList<T> : LinkedList<T>
    {
        #region Ctor
        public CircularLinkedList(params T[] items)
            : base(items)
        {
            //define the default node building strategy
            this.NodeBuildingStrategy = (x) =>
            {
                return new CircularLinkedListNode<T>(x, this);
            };

            this.PostNodeInsertionStrategy = (x) =>
            {
                //if we have circularity issues (ie. we're on the first or last node) then we work that out
                if (this._firstNode != null)
                {
                    this.FirstNode.PreviousNode = this.LastNode;
                    this.LastNode.NextNode = this.FirstNode;
                }
            };
        }
        #endregion
    }


    /// <summary>
    /// a circular linked list node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 
    [DebuggerDisplay("{Value}")]
    public class CircularLinkedListNode<T> : LinkedListNode<T>
    {
        #region Ctor
        public CircularLinkedListNode(T value, CircularLinkedList<T> parentList)
            : base(value, parentList)
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// moves to the next node.  if move goes to first node, returns true for rollover
        /// </summary>
        /// <param name="rollover"></param>
        /// <returns></returns>
        public bool MoveForward(out CircularLinkedListNode<T> next)
        {

            next = this.NextNode as CircularLinkedListNode<T>;
            return next.IsFirst;
        }
        /// <summary>
        /// moves to the previous node.  if move goes to last node, returns true for rollover
        /// </summary>
        /// <param name="rollover"></param>
        /// <returns></returns>
        public bool MoveBack(out CircularLinkedListNode<T> next)
        {
            next = this.PreviousNode as CircularLinkedListNode<T>;
            return next.IsLast;
        }
        /// <summary>
        /// returns the node that is the same number of steps forward from First 
        /// that this instance is, but backwards from First
        /// </summary>
        /// <returns></returns>
        public CircularLinkedListNode<T> GetListComplement()
        {
            CircularLinkedListNode<T> rv = this.ParentList.FirstNode as CircularLinkedListNode<T>;
            CircularLinkedListNode<T> counterNode = this.ParentList.FirstNode as CircularLinkedListNode<T>;
           
            while (!counterNode.Value.Equals(this.Value))
            {
                //if we've iterated thru the whole list and not found the value then it's a bad value
                //rather than validate the value at the start of the method, which would do a list scan also
                if (counterNode.IsLast)
                    throw new ArgumentOutOfRangeException("invalid value");

                counterNode = counterNode.NextNode as CircularLinkedListNode<T>;
                rv = rv.PreviousNode as CircularLinkedListNode<T>;
            }
            return rv;
        }
        public bool MoveForwardBy(T val, out CircularLinkedListNode<T> next)
        {
            CircularLinkedListNode<T> counterNode = this.ParentList.FirstNode as CircularLinkedListNode<T>;
            CircularLinkedListNode<T> nextOUT = this;
            bool rollover = false;

            while (!counterNode.Value.Equals(val))
            {
                //if we've iterated thru the whole list and not found the value then it's a bad value
                //rather than validate the value at the start of the method, which would do a list scan also
                if (counterNode.IsLast)
                    throw new ArgumentOutOfRangeException("invalid value");

                counterNode = counterNode.NextNode as CircularLinkedListNode<T>;
                var rollover2 = nextOUT.MoveForward(out nextOUT);
                if (rollover2 && !rollover)
                    rollover = true;
            }

            next = nextOUT;
            return rollover;
        }
        public bool MoveBackBy(T val, out CircularLinkedListNode<T> next)
        {
            CircularLinkedListNode<T> counterNode = this.ParentList.FirstNode as CircularLinkedListNode<T>;
            CircularLinkedListNode<T> nextOUT = this;
            bool rollover = false;

            while (!counterNode.Value.Equals(val))
            {
                //if we've iterated thru the whole list and not found the value then it's a bad value
                //rather than validate the value at the start of the method, which would do a list scan also
                if (counterNode.IsLast)
                    throw new ArgumentOutOfRangeException("invalid value");

                counterNode = counterNode.NextNode as CircularLinkedListNode<T>;
                var rollover2 = nextOUT.MoveBack(out nextOUT);
                if (rollover2 && !rollover)
                    rollover = true;
            }

            next = nextOUT;
            return rollover;
        }
        public CircularLinkedListNode<T> FindNodeByValue(T val)
        {
            CircularLinkedListNode<T> counterNode = this.ParentList.FirstNode as CircularLinkedListNode<T>;

            while (!counterNode.Value.Equals(val))
            {
                //if we've iterated thru the whole list and not found the value then it's a bad value
                //rather than validate the value at the start of the method, which would do a list scan also
                if (counterNode.IsLast)
                    throw new ArgumentOutOfRangeException("invalid value");

                counterNode = counterNode.NextNode as CircularLinkedListNode<T>;
            }

            return counterNode;
        }
        #endregion
    }

    internal class CircularLinkedListTests
    {
        internal static void Test()
        {
            CircularLinkedList<int> listOfInt = new CircularLinkedList<int>();
            for (int i = 0; i < 100; i++)
            {
                var node = listOfInt.AddLast(i);
                Debug.Assert(listOfInt.Contains(i));
                Debug.Assert(listOfInt.LastNode.Value == i);
                Debug.Assert(listOfInt.FirstNode.Value == 0);
                var vals = listOfInt.Values;
                if (vals != null && vals.Length > 1)
                {
                    Debug.Assert(listOfInt.LastNode.PreviousNode.Value == i - 1);
                    Debug.Assert(listOfInt.AreAdjacent(listOfInt.LastNode.PreviousNode, listOfInt.LastNode));
                    Debug.Assert(listOfInt.AreAdjacent(listOfInt.LastNode, listOfInt.FirstNode));
                }

                var cnode = node as CircularLinkedListNode<int>;
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

                if (!cnode.IsFirst)
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
                Debug.Assert(listOfInt.AreAdjacent(listOfInt.FirstNode, node.NextNode));
            }

            while (listOfInt.IsEmpty == false)
            {
                var first = listOfInt.FirstNode;

                var last = listOfInt.LastNode;
                listOfInt.Remove(last);

                if (!listOfInt.IsEmpty)
                {
                    Debug.Assert(listOfInt.LastNode.NextNode == listOfInt.FirstNode);
                    Debug.Assert(object.ReferenceEquals(last.PreviousNode, listOfInt.LastNode));
                }
            }


        }

        internal static void SequenceTest()
        {
            CircularLinkedList<int> listOfInt = new CircularLinkedList<int>();
            for (int i = 0; i < 10; i++)
            {
                listOfInt.AddLast(i);
            }
            CircularLinkedListNode<int> node = listOfInt.FirstNode as CircularLinkedListNode<int>;
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
