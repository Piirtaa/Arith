using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.DataStructures;
using Arith.Decorating;
using Arith.Extensions;
using System.Runtime.Serialization;

namespace Arith.DataStructures.Decorations
{
    public interface ILinkedListDecoration<T> : ILinkedList<T>,
        IDecorationOf<ILinkedList<T>> 
    { 
    }

    public abstract class LinkedListDecorationBase<T> : DecorationBase,
        ILinkedListDecoration<T>, IDecorationOf<ILinkedList<T>>
    {
        #region Ctor
        public LinkedListDecorationBase(object decorated)
            : base(decorated)
        {
        }
        #endregion

        #region ISerializable
        protected LinkedListDecorationBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        protected override void ISerializable_GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.ISerializable_GetObjectData(info, context);
        }
        #endregion

        #region Methods
        /// <summary>
        /// gets the first linked list below this
        /// </summary>
        public ILinkedList<T> DecoratedOf
        {
            get { return this.Decorated.AsBelow<ILinkedList<T>>(false); }
        }
        #endregion

        #region Decorated Methods
        public Func<T, ILinkedListNode<T>> NodeBuildingStrategy
        {
            get { return DecoratedOf.NodeBuildingStrategy; }
            set { DecoratedOf.NodeBuildingStrategy = value; }
        }
        public virtual ILinkedListNode<T> FirstNode { get { return this.DecoratedOf.FirstNode; } }
        public virtual ILinkedListNode<T> LastNode { get { return this.DecoratedOf.LastNode; } }
        public virtual bool Contains(T val) { return this.DecoratedOf.Contains(val); }
        public virtual bool Contains(ILinkedListNode<T> item) { return this.DecoratedOf.Contains(item); }
        public virtual ILinkedListNode<T> InsertNode(ILinkedListNode<T> node, ILinkedListNode<T> before, ILinkedListNode<T> after) { return this.DecoratedOf.InsertNode(node, before, after); }
        public virtual ILinkedList<T> Remove(ILinkedListNode<T> item) { return this.DecoratedOf.Remove(item); }
        #endregion
    }
}
