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
        public NumericDecorationBase(object decorated)
            : base(decorated)
        {
            var inner = decorated.GetInnerDecorated();
            if(!(inner is Numeric))
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

        #region IDecoratedOf
        public INumeric DecoratedOf
        {
            get { return this.Decorated.AsBelow<INumeric>(false); }
        }
        #endregion

        #region INumeric
        /// <summary>
        /// Returns the inner, undecorated numeric
        /// </summary>
        public Numeric InnerNumeric
        {
            get { return this.Inner as Numeric; }
        }

        /*Note: the INumeric members that are "immutable of implementation" are those
         * that reflect the underlying Numeric structure, and are NOT marked as 
         * virtual, nor do they operate on the DecoratedOf (next INumeric down the
         * cake).
         * 
         * Those behavioural members that we want the cake to interact with, we
         * mark virtual and operate on DecoratedOf
         */ 

        public NumeralSet NumberSystem { get { return this.InnerNumeric.NumberSystem; } }
        public bool IsPositive { get { return this.InnerNumeric.IsPositive; } }
        public string SymbolsText { get { return this.InnerNumeric.SymbolsText; } }
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
        public IDigitNode ZerothDigit { get { return this.InnerNumeric.ZerothDigit; } }
        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        public bool? Compare(INumeric number) { return this.InnerNumeric.Compare(number); }
        /// <summary>
        /// duplicates the entire decoration chain
        /// </summary>
        /// <returns></returns>
        public INumeric Clone()
        {
            //get the Numeric and clone it
            var clone = this.InnerNumeric.Clone();

            var rv = this.CloneDecorationCake(clone);
            return rv as INumeric;
        }
        #endregion

        #region ILinkedList
        public Func<IDigit, ILinkedListNode<IDigit>> NodeBuildingStrategy
        {
            get
            {
                return this.InnerNumeric.NodeBuildingStrategy;
            }
            set
            {
                this.InnerNumeric.NodeBuildingStrategy = value;
            }
        }

        public ILinkedListNode<IDigit> FirstNode
        {
            get {return this.InnerNumeric.FirstNode; }
        }

        public ILinkedListNode<IDigit> LastNode
        {
            get { return this.InnerNumeric.LastNode ; }
        }

        public bool Contains(IDigit val)
        {
            return this.InnerNumeric.Contains(val);
        }

        public bool Contains(ILinkedListNode<IDigit> item)
        {
            return this.InnerNumeric.Contains(item);
        }

        public ILinkedListNode<IDigit> AddFirst(IDigit val)
        {
            return this.InnerNumeric.AddFirst(val);
        }

        public ILinkedListNode<IDigit> AddLast(IDigit val)
        {
            return this.InnerNumeric.AddLast(val);
        }

        public ILinkedListNode<IDigit> Insert(IDigit val, ILinkedListNode<IDigit> before, ILinkedListNode<IDigit> after)
        {
            return this.InnerNumeric.Insert(val, before, after);
        }

        public ILinkedListNode<IDigit> InsertNode(ILinkedListNode<IDigit> node, ILinkedListNode<IDigit> before, ILinkedListNode<IDigit> after)
        {
            return this.InnerNumeric.InsertNode(node, before, after);
        }

        public ILinkedList<IDigit> Remove(ILinkedListNode<IDigit> item)
        {
            return this.InnerNumeric.Remove(item);
        }
        #endregion
    }
}
