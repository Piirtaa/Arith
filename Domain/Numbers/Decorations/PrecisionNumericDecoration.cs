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

    public class PrecisionNumericDecoration : NumericDecorationBase, 
        IHasPrecision, IIsA<HookedLinkedListDecoration<IDigit>>
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public PrecisionNumericDecoration(object decorated, INumeric decimalPlaces)
            : base(decorated)
        {
            if (decimalPlaces == null)
                throw new ArgumentNullException("decimalPlaces");

            this.DecimalPlaces = decimalPlaces;

            var hookDecoration = this.AsBelow<HookedLinkedListDecoration<IDigit>>(false);
            hookDecoration.AppendPostMutateStrategy((x) =>
            {
                //now ensure we don't have more than the specified decimal places
                this.InnerNumeric.TruncateToDecimalPlaces(this.DecimalPlaces);
            });


        }
        #endregion

        #region Static
        public static PrecisionNumericDecoration New(object decorated, INumeric decimalPlaces)
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
            return new PrecisionNumericDecoration(thing, this.DecimalPlaces);
        }
        #endregion

        #region Properties
        public INumeric DecimalPlaces { get; set; }
        #endregion
    }

    public static class PrecisionNumberDecorationExtensions
    {
        public static PrecisionNumericDecoration HasPrecision(this object decorated, 
            INumeric decimalPlaces)
        {
            var decoration = decorated.ApplyDecorationIfNotPresent<PrecisionNumericDecoration>(x =>
            {
                //note the hooking injection
                //When decorating inline, return the outermost
                return PrecisionNumericDecoration.New(decorated.HasHooks<IDigit>().Outer, decimalPlaces);
            });

            //update precision with passed in value
            decoration.DecimalPlaces = decimalPlaces;

            return decoration;
        }

        public static Numeric GetDecimalPlaces(this INumeric thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            var shifty = thisNumber.Clone().HasShift();
            var places = shifty.ShiftToZero();

            return places.GetInnerNumeric();
        }
        public static void TruncateToDecimalPlaces(this INumeric thisNumber,
            Numeric places)
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

                addy.PerformThisManyTimes(x =>
                {
                    thisNumber.Remove(thisNumber.FirstNode);
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

            var precision = new Numeric(set, "5").HasAddition();
            var num = new Numeric(set, "123456789").HasShift().HasPrecision(precision);
            
            var counter = precision.Clone() as AddingNumericDecoration ;
            counter.PerformThisManyTimes(x =>
            {
                var shifty = num.As<IHasShift>(false);
                shifty.ShiftLeft();
            });
            Debug.Assert(num.SymbolsText == "1234.56789");
            num.As<IHasShift>(false).ShiftLeft();

            //the precision is maintained to 5 digits
            Debug.Assert(num.SymbolsText == "123.45678");


        }

    }
}
