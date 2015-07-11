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
    public interface ILinkedListDecoration<T> : ILinkedList<T>, IDecorationOf<ILinkedList<T>> { }

    public abstract class LinkedListDecorationBase<T> : DecorationOfBase<ILinkedList<T>>, ILinkedListDecoration<T>
    {
        #region Ctor
        public LinkedListDecorationBase(ILinkedList<T> decorated)
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
        public override ILinkedList<T> This
        {
            get { return this; }
        }

        #endregion

        #region Decorated Methods
        public Func<T, ILinkedListNode<T>> NodeBuildingStrategy
        {
            get { return this.Decorated.NodeBuildingStrategy; }
            set { this.Decorated.NodeBuildingStrategy = value; }
        }
        public virtual ILinkedListNode<T> FirstNode { get { return this.Decorated.FirstNode; } }
        public virtual ILinkedListNode<T> LastNode { get { return this.Decorated.LastNode; } }
        public virtual bool Contains(T val) { return this.Decorated.Contains(val); }
        public virtual bool Contains(ILinkedListNode<T> item) { return this.Decorated.Contains(item); }
        public virtual ILinkedListNode<T> AddFirst(T val) { return this.Decorated.AddFirst(val); }
        public virtual ILinkedListNode<T> AddLast(T val) { return this.Decorated.AddLast(val); }
        public virtual ILinkedListNode<T> Insert(T val, ILinkedListNode<T> before, ILinkedListNode<T> after) { return this.Decorated.Insert(val, before, after); }
        public virtual ILinkedListNode<T> InsertNode(ILinkedListNode<T> node, ILinkedListNode<T> before, ILinkedListNode<T> after) { return this.Decorated.InsertNode(node, before, after); }
        public virtual ILinkedList<T> Remove(ILinkedListNode<T> item) { return this.Decorated.Remove(item); }
        #endregion
    }
}
