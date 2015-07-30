using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.DataStructures;
using Arith.Decorating;
using Arith.Extensions;
using System.Runtime.Serialization;
using Arith.Domain.Digits;

namespace Arith.Domain.Numbers.Decorations
{
    public interface IDigitNodeDecoration : IDigitNode, IDecoration,
        IDecorationOf<IDigitNode>
    { }

    public abstract class DigitNodeDecorationBase : DecorationBase,
        IDigitNodeDecoration
    {
        #region Ctor
        public DigitNodeDecorationBase(object decorated,
            string decorationName = null)
            : base(decorated, decorationName)
        {

        }
        #endregion

        #region ISerializable
        protected DigitNodeDecorationBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        protected override void ISerializable_GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.ISerializable_GetObjectData(info, context);
        }
        #endregion

        #region IDecoratedOf
        /// <summary>
        /// gets the first linked list below this
        /// </summary>
        public IDigitNode DecoratedOf
        {
            get { return this.Decorated.AsBelow<IDigitNode>(false); }
        }
        #endregion

        #region IDigitNode
        public virtual void SetValue(string symbol)
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            this.DecoratedOf.SetValue(symbol);
        }
        public virtual bool Add(string symbol)
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            var rv = this.DecoratedOf.Add(symbol);
            return rv;
        }
        public virtual bool Subtract(string symbol)
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            var rv = this.DecoratedOf.Subtract(symbol);
            return rv;
        }
        public virtual bool AddOne()
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            var rv = this.DecoratedOf.AddOne();
            return rv;
        }
        public virtual bool SubtractOne()
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            var rv = this.DecoratedOf.SubtractOne();
            return rv;
        }
        #endregion


        #region ILinkedListNode
        public virtual IDigit NodeValue
        {
            get { return this.DecoratedOf.NodeValue; }
        }

        public virtual ILinkedListNode<IDigit> NextNode
        {
            get
            {
                return this.DecoratedOf.NextNode;
            }
            set
            {
                this.DecoratedOf.NextNode = value;
            }
        }

        public virtual ILinkedListNode<IDigit> PreviousNode
        {
            get
            {
                return this.DecoratedOf.PreviousNode;
            }
            set
            {
                this.DecoratedOf.PreviousNode = value;
            }
        }

        public virtual ILinkedList<IDigit> ParentList
        {
            get { return this.DecoratedOf.ParentList; }
        }
        #endregion
    }
}
