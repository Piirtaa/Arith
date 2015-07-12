using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Decorating;
using System.Runtime.Serialization;
using Arith.DataStructures;
using System.Diagnostics;
using Arith.DataStructures.Decorations;
using Arith.Domain.Digits;

namespace Arith.Domain.Numbers.Decorations
{
    public interface IHasPrecision : INumericDecoration
    {
        INumeric DecimalPlaces { get; set; }
    }

    public class PrecisionNumericDecoration : NumericDecorationBase, IHasPrecision
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public PrecisionNumericDecoration(INumeric decorated, INumeric decimalPlaces)
            : base(decorated.HasHooks())
        {
            //note that we've decorated hooks on Decorated above ^^

            if (decimalPlaces == null)
                throw new ArgumentNullException("decimalPlaces");

            this.DecimalPlaces = decimalPlaces;

            var hookDecoration = this.AsBelow<IHasHooks<IDigit>>(false);
            hookDecoration.PostMutateStrategy = (x) =>
            {
                //now ensure we don't have more than the specified decimal places
                this.ThisNumeric.TruncateToDecimalPlaces(this.DecimalPlaces);
            };


        }
        #endregion

        #region Static
        public static PrecisionNumericDecoration New(INumeric decorated, INumeric decimalPlaces)
        {
            return new PrecisionNumericDecoration(decorated, decimalPlaces);
        }
        #endregion

        #region ISerializable
        protected PrecisionNumericDecoration(SerializationInfo info, StreamingContext context)
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
        public override IDecoration ApplyThisDecorationTo(object thing)
        {
            return new PrecisionNumericDecoration(thing as INumeric, this.DecimalPlaces);
        }
        #endregion

        #region Properties
        public INumeric DecimalPlaces { get; set; }
        #endregion
    }

    public static class PrecisionNumberDecorationExtensions
    {
        public static PrecisionNumericDecoration HasPrecision(this INumeric decorated, INumeric decimalPlaces)
        {
            return PrecisionNumericDecoration.New(decorated, decimalPlaces);
        }
        public static INumeric GetDecimalPlaces(this INumeric thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            var shifty = thisNumber.Clone().HasShift();
            var places = shifty.ShiftToZero();

            return places;
        }
        public static void TruncateToDecimalPlaces(this INumeric thisNumber,
            INumeric places)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            if (places == null)
                throw new ArgumentNullException("places");

            var currPlaces = thisNumber.GetDecimalPlaces();
            if (currPlaces.IsGreaterThan(places))
            {
                var addy = currPlaces.HasAddition();
                addy.Subtract(places);

                addy.CountdownToZero(x =>
                {
                    thisNumber.Remove(thisNumber.LastNode);
                });
            }
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


            ////var num = new Number(null, set);
            ////var b = num.SymbolsText;

            //Number num = new Number("123456789", set);
            //var precision = new Number("5", set);
            //var cake = num.HasPrecision(precision);

            //precision.SymbolicNumber.CountdownToZero(x =>
            //{
            //    cake.SymbolicNumber.ShiftLeft();
            //});
            //Debug.Assert(cake.SymbolsText == "1234.56789");
            //cake.SymbolicNumber.ShiftLeft();
            //Debug.Assert(cake.SymbolsText == "123.45678");


        }

    }
}
