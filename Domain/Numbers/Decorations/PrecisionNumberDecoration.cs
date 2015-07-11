using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Decorating;
using System.Runtime.Serialization;
using Arith.DataStructures;
using System.Diagnostics;

namespace Arith.Domain.Numbers.Decorations
{
    public interface IHasPrecision : INumericDecoration
    {
        INumeric DecimalPlaces { get; set; }
    }

    public class PrecisionNumberDecoration : NumericDecorationBase, IHasPrecision
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public PrecisionNumberDecoration(INumeric decorated, INumeric decimalPlaces)
            : base(decorated)
        {
            if (decimalPlaces == null)
                throw new ArgumentNullException("decimalPlaces");

            this.DecimalPlaces = decimalPlaces;

            this.SymbolicNumber.PostMutateStrategy = (x) =>
            {
                //now ensure we don't have more than the specified decimal places
                this.SymbolicNumber.TruncateToDecimalPlaces(this.DecimalPlaces.SymbolicNumber);
            };


        }
        #endregion

        #region Static
        public static PrecisionNumberDecoration New(INumeric decorated, INumeric decimalPlaces)
        {
            return new PrecisionNumberDecoration(decorated, decimalPlaces);
        }
        #endregion

        #region ISerializable
        protected PrecisionNumberDecoration(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        /// <summary>
        /// since we don't want to expose ISerializable concerns publicly, we use a virtual protected
        /// helper function that does the actual implementation of ISerializable, and is called by the
        /// explicit interface implementation of GetObjectData.  This is the method to be overridden in 
        /// derived classes.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected override void ISerializable_GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.ISerializable_GetObjectData(info, context);
        }
        #endregion

        #region Overrides
        public override IDecorationOf<INumeric> ApplyThisDecorationTo(INumeric thing)
        {
            return new PrecisionNumberDecoration(thing, this.DecimalPlaces);
        }
        #endregion

        #region Properties
        public INumeric DecimalPlaces { get; set; }
        #endregion

    }

    public static class PrecisionNumberDecorationExtensions
    {
        public static PrecisionNumberDecoration HasPrecision(this INumeric decorated, INumeric decimalPlaces)
        {
            return PrecisionNumberDecoration.New(decorated, decimalPlaces);
        }
    }


    public class PrecisionNumberTests
    {
        public static void Test()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }


            //var num = new Number(null, set);
            //var b = num.SymbolsText;

            Number num = new Number("123456789", set);
            var precision = new Number("5", set);
            var cake = num.HasPrecision(precision);

            precision.SymbolicNumber.CountdownToZero(x =>
            {
                cake.SymbolicNumber.ShiftLeft();
            });
            Debug.Assert(cake.SymbolsText == "1234.56789");
            cake.SymbolicNumber.ShiftLeft();
            Debug.Assert(cake.SymbolsText == "123.45678");


        }

    }
}
