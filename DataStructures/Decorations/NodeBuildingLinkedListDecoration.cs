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
    /// adds node building
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHasNodeBuilding<T> : ILinkedListDecoration<T>
    {
        ILinkedListNode<T> BuildNode(T obj);
        Func<T, ILinkedList<T>, ILinkedListNode<T>> NodeBuildingStrategy { get; set; }
    }

    public class NodeBuildingLinkedListDecoration<T> : LinkedListDecorationBase<T>,
        IHasNodeBuilding<T>,
        IHasA<IHasLinkedListMutability<T>>
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public NodeBuildingLinkedListDecoration(object decorated,
            Func<T, ILinkedList<T>, ILinkedListNode<T>> nodeBuildingStrategy = null,
            string decorationName = null)
            : base(decorated, decorationName)
        {
            this.NodeBuildingStrategy = nodeBuildingStrategy;
        }
        #endregion

        #region Static
        public static NodeBuildingLinkedListDecoration<T> New(object decorated,
            Func<T, ILinkedList<T>, ILinkedListNode<T>> nodeBuildingStrategy = null,
            string decorationName = null)
        {
            return new NodeBuildingLinkedListDecoration<T>(decorated,
                nodeBuildingStrategy,
                decorationName);
        }
        #endregion

        #region ISerializable
        protected NodeBuildingLinkedListDecoration(SerializationInfo info, StreamingContext context)
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
            return new NodeBuildingLinkedListDecoration<T>(thing,
                this.NodeBuildingStrategy,
                this.DecorationName);
        }
        #endregion

        #region Properties
        /// <summary>
        /// gets the mutable face
        /// </summary>
        public IHasLinkedListMutability<T> MutableList
        {
            get { return this.As<IHasLinkedListMutability<T>>(false); }
        }
        /// <summary>
        /// override/replace this strategy if we want anything other than a new LinkedListNode 
        /// </summary>
        public Func<T, ILinkedList<T>, ILinkedListNode<T>> NodeBuildingStrategy { get; set; }
        #endregion

        #region Methods
        public ILinkedListNode<T> BuildNode(T obj)
        {
            ILinkedListNode<T> rv = null;

            //get the topmost linked list decoration 
            var face = this.As<ILinkedList<T>>(false);

            if (this.NodeBuildingStrategy != null)
            {
                rv = this.NodeBuildingStrategy(obj, face);
            }
            else
            {
                rv = LinkedListNode<T>.New(obj, face);
            }

            return rv;
        }
        public void AddNodes(params T[] list)
        {
            list.WithEach(x =>
            {
                this.AddLast(x);
            });
        }
        public ILinkedListNode<T> AddFirst(T val)
        {
            return this.Insert(val, null, this.FirstNode);
        }
        public ILinkedListNode<T> AddLast(T val)
        {
            return this.Insert(val, this.LastNode, null);
        }
        public ILinkedListNode<T> Insert(
            T val, ILinkedListNode<T> before, ILinkedListNode<T> after)
        {
            ILinkedListNode<T> node = null;
            node = this.BuildNode(val);
            return this.MutableList.InsertNode(node, before, after);
        }
        #endregion
    }

    public static class NodeBuildingLinkedListDecorationExtensions
    {
        public static void DoWhileNodeBuilding<T>(this ILinkedList<T> thing,
            Func<T, ILinkedList<T>, ILinkedListNode<T>> nodeBuildingStrategy,
            Action<NodeBuildingLinkedListDecoration<T>> action)
        {
            var decorated = NodeBuildingLinkedListDecoration<T>.New(thing, nodeBuildingStrategy);
            action(decorated);
            decorated.Undecorate();
        }

        public static NodeBuildingLinkedListDecoration<T> HasNodeBuilding<T>(this ILinkedList<T> thing,
            Func<T, ILinkedList<T>, ILinkedListNode<T>> nodeBuildingStrategy = null,
            string decorationName = null)
        {
                return NodeBuildingLinkedListDecoration<T>.New(
                    thing,
                    nodeBuildingStrategy,
                    decorationName);
        }

        public static NodeBuildingLinkedListDecoration<T> GetNodeBuildingCake<T>(
            this ILinkedList<T> thing,
    Func<T, ILinkedList<T>, ILinkedListNode<T>> nodeBuildingStrategy = null,
    string decorationName = null)
        {
            var rv = thing.GetMutabilityCake<T>(decorationName).
                HasNodeBuilding<T>(nodeBuildingStrategy, decorationName);
            return rv;
        }
    }


    public class NodebuildingLinkedListTests
    {
        public static void Test()
        {
            var listOfInt = new LinkedList<int>().GetNodeBuildingCake<int>();
            for (int i = 0; i < 100; i++)
            {
                listOfInt.AddLast(i);
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
                var node = listOfInt.AddFirst(i);
                Debug.Assert(listOfInt.FirstNode.NodeValue == i);
                Debug.Assert(listOfInt.FirstNode.IsPreceding(node.NextNode));
            }


            while (listOfInt.InnerList.IsEmpty() == false)
            {
                var last = listOfInt.LastNode;
                listOfInt.MutableList.Remove(last);

                if (!listOfInt.InnerList.IsEmpty())
                {
                    Debug.Assert(listOfInt.LastNode.NextNode == null);
                    Debug.Assert(object.ReferenceEquals(last.PreviousNode, listOfInt.LastNode));
                }
            }


        }
    }
}
