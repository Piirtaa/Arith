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
        ILinkedListDecoration<T>, 
        IDecorationOf<ILinkedList<T>>
    {
        #region Ctor
        public LinkedListDecorationBase(object decorated, string cakeName = null)
            : base(decorated, cakeName)
        {
            if (this.InnerList == null)
                throw new InvalidOperationException("inner list must be LinkedList");
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

        #region Properties
        /// <summary>
        /// gets the first linked list below this
        /// </summary>
        public ILinkedList<T> DecoratedOf
        {
            get { return this.Decorated.AsBelow<ILinkedList<T>>(false); }
        }
        /// <summary>
        /// the core object in an ILinkedList decoration is always LinkedList
        /// </summary>
        public LinkedList<T> InnerList
        {
            get { return this.AsInnermost<LinkedList<T>>(false); }
        }
        #endregion

        #region Decorated Methods
        public virtual ILinkedListNode<T> FirstNode { get { return this.DecoratedOf.FirstNode; } }
        public virtual ILinkedListNode<T> LastNode { get { return this.DecoratedOf.LastNode; } }
        public virtual bool Contains(T val) { return this.DecoratedOf.Contains(val); }
        public virtual bool Contains(ILinkedListNode<T> item) { return this.DecoratedOf.Contains(item); }
        #endregion
    }
}
