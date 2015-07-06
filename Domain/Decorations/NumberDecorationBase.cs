using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.DataStructures;
using Arith.Decorating;
using Arith.Extensions;
using System.Runtime.Serialization;

namespace Arith.Domain.Decorations
{
    public interface INumberDecoration : INumber, IDecorationOf<INumber> { }

    public abstract class NumberDecorationBase : DecorationOfBase<INumber>, INumberDecoration
    {
       #region Ctor
        public NumberDecorationBase(INumber decorated)
            : base(decorated)
        {
        }
        #endregion

        #region ISerializable
        protected NumberDecorationBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        protected override void ISerializable_GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.ISerializable_GetObjectData(info, context);
        }
        #endregion

        #region Methods
        public override INumber This
        {
            get { return this; }
        }
        public string SymbolsText { get { return this.Decorated.SymbolsText; } }
        public virtual void SetValue(string number) { this.Decorated.SetValue(number); }
        public virtual void Add(string number) { this.Decorated.Add(number); }
        public virtual void Subtract(string number) { this.Decorated.Subtract(number); }
        public bool IsPositive { get { return this.Decorated.IsPositive; } }
        public NumeralSet NumberSystem { get { return this.Decorated.NumberSystem; } }

        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        public bool? Compare(string number) { return this.Decorated.Compare(number); }
        #endregion
    }
}
