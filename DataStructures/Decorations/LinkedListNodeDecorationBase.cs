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
    public interface ILinkedListNodeDecoration<T> : ILinkedListNode<T>,
        IDecorationOf<ILinkedListNode<T>>
    {
    }

    public abstract class LinkedListNodeDecorationBase<T> : DecorationBase,
        ILinkedListNodeDecoration<T>,
        IDecorationOf<ILinkedListNode<T>>
    {
        #region Ctor
        public LinkedListNodeDecorationBase(object decorated)
            : base(decorated)
        {
        }
        #endregion

        #region ISerializable
        protected LinkedListNodeDecorationBase(SerializationInfo info, StreamingContext context)
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
        public ILinkedListNode<T> DecoratedOf
        {
            get { return this.Decorated.AsBelow<ILinkedListNode<T>>(false); }
        }
        #endregion

        #region Decorated Methods
        public virtual T Value { get { return this.DecoratedOf.Value; } }
        public virtual ILinkedListNode<T> NextNode { get { return this.DecoratedOf.NextNode; } set { this.DecoratedOf.NextNode = value; } }
        public virtual ILinkedListNode<T> PreviousNode { get { return this.DecoratedOf.PreviousNode; } set { this.DecoratedOf.PreviousNode = value; } }
        public virtual ILinkedList<T> ParentList { get { return this.DecoratedOf.ParentList; } }
        #endregion
    }
}
