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
    /// <summary>
    /// defines numeric decoration as NOT constraining decorated to being INumeric,
    /// only that decorated has INumeric somewhere in its cake.  Also, the innermost
    /// decorated MUST be an instance of Numeric - it's to enforce "naturalness" aka
    /// its nature is known and understood - has an anti-corruption quality.
    /// </summary>
    public interface INumericDecoration : INumeric, IDecoration,
        IDecorationOf<INumeric>
    { }

    public abstract class NumericDecorationBase : DecorationBase,
        INumericDecoration
    {
        #region Ctor
        public NumericDecorationBase(object decorated, 
            string decorationName = null)
            : base(decorated, decorationName)
        {
            var inner = decorated.GetInnerDecorated();
            if (!(inner is Numeric))
                throw new InvalidOperationException("decorated does not have inner Numeric");

        }
        #endregion

        #region ISerializable
        protected NumericDecorationBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        protected override void ISerializable_GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.ISerializable_GetObjectData(info, context);
        }
        #endregion

        #region Properties
        public INumeric DecoratedOf
        {
            get
            {
                var rv = this.Decorated.AsBelow<INumeric>(false);
                return rv;
            }
        }
        /// <summary>
        /// Returns the lower numeric
        /// </summary>
        public Numeric InnermostNumeric
        {
            get { return this.AsInnermost<Numeric>(false); }
        }
        #endregion

        #region INumeric
        /*Note: the INumeric members that are "immutable of implementation" are those
         * that reflect the underlying Numeric structure, and are NOT marked as 
         * virtual, nor do they operate on the DecoratedOf (next INumeric down the
         * cake).
         * 
         * Those behavioural members that we want the cake to interact with, we
         * mark virtual and operate on DecoratedOf
         */

        public NumeralSet NumberSystem { get { return this.InnermostNumeric.NumberSystem; } }
        public bool IsPositive { get { return this.InnermostNumeric.IsPositive; } }
        public string SymbolsText { get { return this.InnermostNumeric.SymbolsText; } }
        public virtual void SetValue(string number)
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            this.DecoratedOf.SetValue(number);
        }
        public virtual void SetValue(INumeric number)
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            this.DecoratedOf.SetValue(number);
        }
        public IDigitNode ZerothDigit { get { return this.InnermostNumeric.ZerothDigit; } }
        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        public bool? Compare(INumeric number) { return this.InnermostNumeric.Compare(number); }
        /// <summary>
        /// duplicates the entire decoration chain
        /// </summary>
        /// <returns></returns>
        public INumeric Clone()
        {
            //get the Numeric and clone it
            var clone = this.InnermostNumeric.Clone();

            var rv = this.CloneDecorationCake(clone);
            return rv as INumeric;
        }
        #endregion

        #region ILinkedList
        public ILinkedListNode<IDigit> FirstNode
        {
            get { return this.InnermostNumeric.FirstNode; }
        }

        public ILinkedListNode<IDigit> LastNode
        {
            get { return this.InnermostNumeric.LastNode; }
        }

        public bool Contains(IDigit val)
        {
            return this.InnermostNumeric.Contains(val);
        }

        public bool Contains(ILinkedListNode<IDigit> item)
        {
            return this.InnermostNumeric.Contains(item);
        }

        //public ILinkedListNode<IDigit> AddFirst(IDigit val)
        //{
        //    return this.InnermostNumeric.AddFirst(val);
        //}

        //public ILinkedListNode<IDigit> AddLast(IDigit val)
        //{
        //    return this.InnermostNumeric.AddLast(val);
        //}

        //public ILinkedListNode<IDigit> Insert(IDigit val, ILinkedListNode<IDigit> before, ILinkedListNode<IDigit> after)
        //{
        //    return this.InnermostNumeric.Insert(val, before, after);
        //}

        //public ILinkedListNode<IDigit> InsertNode(ILinkedListNode<IDigit> node, ILinkedListNode<IDigit> before, ILinkedListNode<IDigit> after)
        //{
        //    return this.InnermostNumeric.InsertNode(node, before, after);
        //}

        //public ILinkedList<IDigit> Remove(ILinkedListNode<IDigit> item)
        //{
        //    return this.InnermostNumeric.Remove(item);
        //}
        #endregion
    }
}
