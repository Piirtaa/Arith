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
    /// only that decorated has INumeric somewhere in its cake
    /// </summary>
    public interface INumericDecoration : INumeric, IDecoration { }

    public abstract class NumericDecorationBase : DecorationBase,
        INumericDecoration
    {
        #region Ctor
        public NumericDecorationBase(object decorated)
            : base(decorated)
        {
            if (!decorated.Is<Numeric>())
                throw new InvalidOperationException("decorated does not have a Numeric");
                

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

        #region Methods
        public Numeric ThisNumeric
        {
            get { return this.Inner as Numeric; }
        }
        public NumeralSet NumberSystem { get { return this.ThisNumeric.NumberSystem; } }
        public bool IsPositive { get { return this.ThisNumeric.IsPositive; } }
        public string SymbolsText { get { return this.ThisNumeric.SymbolsText; } }
        public virtual void SetValue(string number) 
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            this.ThisNumeric.SetValue(number); 
        }

        public IDigitNode ZerothDigit { get { return this.ThisNumeric.ZerothDigit; } }
        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        public bool? Compare(INumeric number) { return this.ThisNumeric.Compare(number); }
        /// <summary>
        /// duplicates the entire decoration chain
        /// </summary>
        /// <returns></returns>
        public INumeric Clone()
        {
            //get the Numeric and clone it
            var clone = this.ThisNumeric.Clone();

            var rv = this.CloneDecorationCake(clone);
            return rv as INumeric;
        }
        #endregion
    }
}
