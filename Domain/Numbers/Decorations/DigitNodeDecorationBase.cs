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
    public interface IDigitNodeDecoration : IDigitNode, IDecoration { }

    public abstract class DigitNodeDecorationBase : DecorationBase,
        IDigitNodeDecoration
    {
        #region Ctor
        public DigitNodeDecorationBase(object decorated)
            : base(decorated)
        {
            var inner = decorated.GetInnerDecorated();
            if(!(inner is DigitNode))
                throw new InvalidOperationException("decorated does not have inner DigitNode");

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

        #region IDigitNode
        /// <summary>
        /// Returns the inner, undecorated digit node
        /// </summary>
        public DigitNode InnerDigitNode
        {
            get { return this.Inner as DigitNode; }
        }
        public virtual void SetValue(string symbol)
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            this.InnerDigitNode.SetValue(symbol); 
        }
        public virtual bool Add(string symbol)
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            var rv = this.InnerDigitNode.Add(symbol);
            return rv;
        }
        public virtual bool Subtract(string symbol)
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            var rv = this.InnerDigitNode.Subtract(symbol);
            return rv;
        }
        public virtual bool AddOne()
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            var rv = this.InnerDigitNode.AddOne();
            return rv;
        }
        public virtual bool SubtractOne()
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            var rv = this.InnerDigitNode.SubtractOne();
            return rv;
        }
        #endregion

       
    }
}
