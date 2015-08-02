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
    /// <summary>
    /// has precision, doesn't allow a number to grow larger than precision allows.
    /// 
    /// </summary>
    public interface IHasPrecision : INumericDecoration
    {
        Numeric DecimalPlaces { get; set; }
    }

    public class PrecisionNumericDecoration : NumericDecorationBase,
        IHasPrecision
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public PrecisionNumericDecoration(object decorated,
            INumeric decimalPlaces,
            string decorationName = null)
            : base(decorated, decorationName)
        {
            if (decimalPlaces == null)
                throw new ArgumentNullException("decimalPlaces");

            this.DecimalPlaces = decimalPlaces.GetInnermostNumeric();
        }
        #endregion

        #region Static
        public static PrecisionNumericDecoration New(object decorated,
            INumeric decimalPlaces,
            string decorationName = null)
        {
            return new PrecisionNumericDecoration(decorated, decimalPlaces, decorationName);
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
            return new PrecisionNumericDecoration(thing, this.DecimalPlaces, this.DecorationName);
        }
        #endregion

        #region Properties
        public Numeric DecimalPlaces { get; set; }
        #endregion

        #region INumeric Overrides
        public override void SetValue(INumeric number)
        {
            number.TruncateToDecimalPlaces(this.DecimalPlaces);
            base.SetValue(number);
        }
        #endregion
    }

    public static class PrecisionNumberDecorationExtensions
    {
        public static PrecisionNumericDecoration HasPrecision(this object decorated,
            INumeric decimalPlaces,
            string decorationName = null)
        {
            return PrecisionNumericDecoration.New(decorated, decimalPlaces, decorationName);
        }

        public static Numeric GetDecimalPlaces(this INumeric thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            if (thisNumber.IsEmpty())
                return thisNumber.GetCompatibleZero();

            var shifty = thisNumber.Clone().HasShift();
            var places = shifty.ShiftToZero();

            return places.GetInnermostNumeric();
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
                    thisNumber.DoWhileMutable(y =>
                    {
                        y.RemoveFirst();
                    });
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

            var precision = Numeric.New(set, "5").HasAddition();
            var num = Numeric.New(set, "123456789").HasPrecision(precision).HasShift();

            var counter = precision.Clone() as AddingNumericDecoration;
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
