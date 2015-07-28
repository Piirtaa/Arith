using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Decorating;
using System.Runtime.Serialization;
using Arith.DataStructures;
using System.Diagnostics;
using Arith.Domain.Digits;
using Arith.DataStructures.Decorations;

namespace Arith.Domain.Numbers.Decorations
{
    /// <summary>
    /// note we require a precision decoration to perform division (else we run into endless loops
    /// on repeating numbers)
    /// </summary>
    public interface IHasDivision : INumericDecoration, IIsA<IHasPrecision>
    {
        void Divide(string number);
    }

    public class DividingNumericDecoration : NumericDecorationBase, IHasDivision
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public DividingNumericDecoration(object decorated)
            : base(decorated)
        {
        }
        #endregion

        #region Static
        public static DividingNumericDecoration New(object decorated)
        {
            return new DividingNumericDecoration(decorated);
        }
        #endregion

        #region ISerializable
        protected DividingNumericDecoration(SerializationInfo info, StreamingContext context)
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
            return new DividingNumericDecoration(thing);
        }
        #endregion

        #region IHasDivision
        public void Divide(string number)
        {
            lock (this._stateLock)
            {
                var rv = DivideWithPrecision(
                    this.InnerNumeric,
                    this.GetCompatibleNumber(number),
                    this.As<IHasPrecision>(false).DecimalPlaces as Numeric);

                this.DecoratedOf.SetValue(rv);
            }
        }
        #endregion

        /// <summary>
        /// does a divide without changing the dividend
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <param name="toNumberOfDecimalPlaces"></param>
        /// <returns></returns>
        public static Numeric DivideWithPrecision(INumeric dividend, INumeric divisor,
            INumeric toNumberOfDecimalPlaces)
        {
            if (dividend == null)
                throw new ArgumentNullException("dividend");

            if (divisor == null)
                throw new ArgumentNullException("divisor");

            if (toNumberOfDecimalPlaces == null)
                throw new ArgumentNullException("toNumberOfDecimalPlaces");


            Debug.WriteLine("dividing. {0} / {1} to {2} places", dividend.SymbolsText,
             divisor.SymbolsText,
             toNumberOfDecimalPlaces.SymbolsText);

            //build sum
            var product = dividend.GetCompatibleEmpty().HasAddition();
            product.InnerNumeric.IsPositive = dividend.IsPositive;

            //shift dividend and divisor to zero, to eliminate clarity with decimal shift handling
            //we'll shift back at the end.  this way we're only dividing whole numbers.
            //also makes the code cleaner
            var dividendShifts = dividend.ShiftToZero();
            var divisorShifts = divisor.ShiftToZero();

            //we use a cloned value of the dividend because it will be subtracted from at each step
            //and we don't want to change the passed in reference object - treat it as if it were a value type
            recursiveDivideStep(
                dividend.Clone() as Numeric,
                divisor.GetInnerNumeric(),
                product.InnerNumeric,
                toNumberOfDecimalPlaces.GetInnerNumeric());

            //shift back
            product.HasShift().ShiftLeft(dividendShifts).ShiftLeft(divisorShifts);

            Debug.WriteLine("dividing. {0} / {1} to {2} places ={3}", dividend.SymbolsText,
             divisor.SymbolsText,
             toNumberOfDecimalPlaces.SymbolsText,
             product.SymbolsText);

            return product.InnerNumeric;
        }


        #region Dividing Steps
        /*
        * The division process follows the "Long-hand Arithmetic" approach.  We maintain
         * a dividend (the number we are dividing into), a result (the output of the division),
         * and a divisor (the number we are dividing by).  It's a recursive process wherein the
         * dividend is mutated at each step and the product is updated by the result of the step.
         * To check against infinite recursion we limit the product to a number of decimal places.
         * 
        *   the recursive process is as follows:
         *   -is the divisor greater than the dividend?
         *      -yes.  increase magnitude of dividend.  add least significant zero digit to product.
         *              recurse.
         *      -no.   subtract divisor from dividend until divisor is greater than dividend.
         *              count how many times this operation took place, and add these digits
         *              as least significant digits on the product.  
         *              
         *          -is the dividend zero?
         *            -yes.  return product
         *            -no. recurse  
         *          
        */
        /// <summary>
        /// recursive division step
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <param name="product"></param>
        /// <param name="toNumberOfDecimalPlaces"></param>
        /// <returns></returns>
        private static void recursiveDivideStep(Numeric dividend, Numeric divisor,
            Numeric product, Numeric toNumberOfDecimalPlaces)
        {
            Numeric zero = dividend.GetCompatibleZero();

            //VALIDATIONS-----------------
            //divide by zero check
            if (divisor.IsEqualTo(zero))
                throw new InvalidOperationException("divide by zero");

            //decimal place check
            if (toNumberOfDecimalPlaces != null)
            {
                var prodDecPlaces = product.GetDecimalPlaces();
                if (prodDecPlaces.IsGreaterThan(toNumberOfDecimalPlaces))
                {
                    //truncate and return
                    product.TruncateToDecimalPlaces(toNumberOfDecimalPlaces);
                    return;
                }
            }

            //log step
            Debug.WriteLine("division step begins. {0} / {1} to {2} places. current product={3}", dividend.SymbolsText,
                divisor.SymbolsText,
                toNumberOfDecimalPlaces.SymbolsText,
                product.SymbolsText);

            //STEP 1
            //shift the dividend
            if (shiftDividendToGreaterThanDivisorStep(dividend, divisor, product))
                Debug.WriteLine("division shift step. {0} / {1} to {2} places. current product={3}", dividend.SymbolsText,
     divisor.SymbolsText,
     toNumberOfDecimalPlaces.SymbolsText,
     product.SymbolsText);

            //get the the nearest, larger divided segment that is less than an order of mag
            //greater than the divisor
            Numeric orderOfMag = null;
            var dividendSegment = getDividendSegmentLargerThanDivisor(dividend, divisor, out orderOfMag);

            Debug.WriteLine("nearest larger segment={0}  magnitude {1}", 
                dividendSegment.SymbolsText,
    orderOfMag.SymbolsText);

            //divide this segment number by the dividend
            Numeric remainder = null;
            Numeric subtracted = null;
            var count = divisionByIteratedSubtraction(dividendSegment, divisor, out subtracted, out remainder);

            Debug.WriteLine("division segment calc step. {0} / {1} = {2} . remainder={3} . subtracted={4}",
                dividendSegment.SymbolsText,
                divisor.SymbolsText,
                count.SymbolsText,
                remainder.SymbolsText,
                subtracted.SymbolsText);

            //validate this count is less than the max digit. ie. it's one symbol
            if (!count.FirstNode.IsLast())
                throw new InvalidOperationException("count exceeds order of magnitude");

            //STEP 2
            //perform partial divide operation
            //subtract the count * divisor * shift by orderOfMag greater(right)
            //this value to subtract should be less than or equal to the dividend segment shifted by order of mag
            //record the num of times in the product - append lsd
            //change the dividend to be the remainder and recurse
            subtracted.ShiftRight(orderOfMag);
            var oldDividend = dividend.SymbolsText;
            dividend.HasAddition().Subtract(subtracted);
            product.AddLeastSignificantDigit(count.FirstDigit.Value.Symbol);

            Debug.WriteLine("dividend subtraction step. {0} - {1} = {2}, dividing {3} times",
       oldDividend,
       subtracted.SymbolsText,
       dividend.SymbolsText,
       count.SymbolsText);

            Debug.WriteLine("division step complete. {0} / {1} to {2} places. current product={3}", dividend.SymbolsText,
    divisor.SymbolsText,
    toNumberOfDecimalPlaces.SymbolsText,
    product.SymbolsText);

            //STEP 3 
            //recurse if the dividend is greater than zero
            if (dividend.IsGreaterThan(zero))
            {
                recursiveDivideStep(dividend, divisor, product, toNumberOfDecimalPlaces);
            }
        }
        /// <summary>
        /// tests if the dividend is less than the divisor and needs to be shifted right until it is
        /// greater
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        private static bool shiftDividendToGreaterThanDivisorStep(Numeric dividend, Numeric divisor,
    Numeric product)
        {
            //if the divisor is less than or equal to the dividend, we do no shifting
            if (divisor.IsLessThanOrEqual(dividend))
                return false;

            while (divisor.IsGreaterThan(dividend))
            {
                dividend.HasShift().ShiftRight();
                product.HasShift().ShiftRight();//apply same shift to the product
            }

            return true;
        }
        /// <summary>
        /// most from MSD inwards, finds the smallest number that is greater than the divisor,
        /// and returns the multiple and the number of shifts right (eg. order of magnitude of 
        /// the MSD digits)
        /// 
        /// Example:
        /// 17 into 1234
        /// would return a 123, and an order of mag of 1
        /// 
        /// 17 into 1700
        /// would return 17, and an order of mag of 2
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <param name="numShiftsRights"></param>
        /// <returns></returns>
        private static Numeric getDividendSegmentLargerThanDivisor(Numeric dividend,
            Numeric divisor, out Numeric orderOfMagnitude)
        {
            //get MSD's that are greater than divisor
            //eg.  7 into 120
            //      returns 1 - as 7 goes into 12 1 time
            //      and returns shifts of 1
            Numeric orderOfMag = null;
            Numeric portionNumber = null;
            dividend.IterateMSDs((portion, mag) =>
            {
                if (divisor.IsLessThanOrEqual(portion))
                {
                    portionNumber = portion;
                    orderOfMag = mag;
                    return true;
                }
                return false;
            });

            orderOfMagnitude = orderOfMag;
            return portionNumber;
        }
        /// <summary>
        /// removes the divisor from the dividend until the dividend is smaller than the divisor.
        /// requires that the divisor is less than or equal to dividend to begin with
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        private static Numeric divisionByIteratedSubtraction(Numeric dividend,
            Numeric divisor, out Numeric subtracted, out Numeric remainder)
        {
            var count = dividend.GetCompatibleZero().HasAddition();
            var total = dividend.GetCompatibleZero().HasAddition();

            var subtracty = dividend.Clone().HasAddition();
            while (subtracty.IsGreaterThanOrEqual(divisor))
            {
                subtracty.Subtract(divisor);
                total.Add(divisor);
                count.AddOne();
            }
            remainder = subtracty.InnerNumeric;
            subtracted = total.InnerNumeric;
            return count.InnerNumeric;
        }

        #endregion
    }

    public static class DividingNumberDecorationExtensions
    {
        public static DividingNumericDecoration HasDivision(this object number, Numeric decimalPlaces)
        {
            var decoration = number.ApplyDecorationIfNotPresent<DividingNumericDecoration>(x =>
            {
                //note the precision decoration injection
                return DividingNumericDecoration.New(number.HasPrecision(decimalPlaces).Outer);
            });
            //update the precision to passed arg
            decoration.AsBelow<PrecisionNumericDecoration>(true).DecimalPlaces = decimalPlaces;

            return decoration;
        }

        /// <summary>
        /// walks the most sig digits of a number until the filter function returns true.
        /// The filter args are the number portion and its order of magnitude
        /// eg. for a number 1234 the iterations would look like this
        /// 1, order of mag = 3.  1 * 10^3 = 1000
        /// 12, order of mag = 2.  12 * 10^2 = 1200
        /// 123, order of mag = 1.  123 * 10^1 = 1230
        /// 1234, order of mag = 0.  1234 * 10^0 = 1234
        /// </summary>
        /// <param name="numeric"></param>
        /// <param name="filter"></param>
        public static void IterateMSDs(this INumeric numeric,
            Func<Numeric, Numeric, bool> filter)
        {
            numeric.Filter(node =>
            {
                //first get the number portion
                IDigitNode dNode = node as IDigitNode;
                var trimNum = numeric.Trim(dNode, true);

                //then get the number order of magnitude
                var mag = numeric.GetDigitMagnitude(dNode);

                return filter(trimNum, mag);
            }, false);
        }
    }



    public class DividingNumberTests
    {
        public static void Test()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }

            var bigNum = Numeric.New(set, "1234567890.246");

            //bigNum.Iterate(digit =>
            //{
            //    var trim = bigNum.Trim(digit as DigitNode, true);
            //    //Debug.WriteLine("on digit={0} trim to msd={1}", digit.Value.Symbol, trim.SymbolsText);

            //    var trim2 = bigNum.Trim(digit as DigitNode, false);
            //    //Debug.WriteLine("on digit={0} trim to lsd={1}", digit.Value.Symbol, trim2.SymbolsText);

            //}, true);

            //bigNum.IterateMSDs((portion, mag) =>
            //{
            //    //Debug.WriteLine("portion={0}, mag={1}", portion.SymbolsText, mag.SymbolsText);
            //    return false;
            //});

            int topLimit = 100;
            var precision = Numeric.New(set, set.OneSymbol);

            for (int x = 1; x < topLimit; x++)
            {
                for (int y = 1; y < topLimit; y++)
                {
                    var num1 = Numeric.New(set, x.ToString()).HasDivision(precision);

                    decimal res = (decimal)x / (decimal)y;
                    num1.Divide(y.ToString());

                    Debug.Assert(num1.SymbolsText == res.ToString("#.#"));
                }
            }


        }

    }
}
