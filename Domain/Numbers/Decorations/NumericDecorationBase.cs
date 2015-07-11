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
    public interface INumericDecoration : INumeric, IDecorationOf<INumeric> { }

    public abstract class NumericDecorationBase : DecorationOfBase<INumeric>,
        INumericDecoration
    {
        #region Ctor
        public NumericDecorationBase(INumeric decorated)
            : base(decorated)
        {
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
        public override INumeric This
        {
            get { return this; }
        }  
        public NumeralSet NumberSystem { get { return this.Decorated.NumberSystem; } }
        public bool IsPositive { get { return this.Decorated.IsPositive; } }    
        public string SymbolsText { get { return this.Decorated.SymbolsText; } }
        public virtual void SetValue(string number) 
        {
            if (!this.IsDecorationEnabled)
                throw new InvalidOperationException("decoration disabled");

            this.Decorated.SetValue(number); 
        }

        public IDigitNode ZerothDigit { get { return this.Decorated.ZerothDigit; } }
        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        public bool? Compare(INumeric number) { return this.Decorated.Compare(number); }
        /// <summary>
        /// duplicates the entire decoration chain
        /// </summary>
        /// <returns></returns>
        public INumeric Clone()
        {
            var clone = this.Decorated.Clone();

            return this.ApplyThisDecorationTo(clone) as INumeric;
        }
        #endregion
    }
}
