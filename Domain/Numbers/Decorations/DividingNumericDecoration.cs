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
                var rv = DivideWithPrecision(this.Numeric,
                    new Numeric(number, this.NumberSystem),
                    new Numeric(numberOfDecimalPlaces, this.NumberSystem));

                this.Numeric.SetValue(rv);
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
        public static Numeric DivideWithPrecision(Numeric dividend, Numeric divisor,
            Numeric toNumberOfDecimalPlaces)
        {
            if (dividend == null)
                throw new ArgumentNullException("dividend");

            if (divisor == null)
                throw new ArgumentNullException("divisor");

            if (toNumberOfDecimalPlaces == null)
                throw new ArgumentNullException("toNumberOfDecimalPlaces");

            //build sum
            var product = dividend.GetCompatibleZero().HasAddition();
            product.InnerNumeric.IsPositive = dividend.IsPositive;

            //shift dividend and divisor to zero, to eliminate clarity with decimal shift handling
            //we'll shift back at the end.  this way we're only dividing whole numbers.
            //also makes the code cleaner
            var dividendShifts = dividend.HasHooks<IDigit>().HasShift().ShiftToZero();
            var divisorShifts = divisor.HasHooks<IDigit>().HasShift().ShiftToZero();

            //we use a cloned value of the dividend because it will be subtracted from at each step
            //and we don't want to change the passed in reference object - treat it as if it were a value type
            var rv = divide(Numeric.Clone(dividend), divisor, product, toNumberOfDecimalPlaces);

            //shift back
            rv.ShiftLeft(dividendShifts).ShiftLeft(divisorShifts);

            return rv;
        }
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
        /// tests if the dividend is less than the divisor and needs to be shifted right until it is
        /// greater
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        private static bool shiftDividendToGreaterThanDivisor(Numeric dividend, Numeric divisor,
    Numeric product)
        {
            //if the divisor is less than or equal to the dividend, we do no shifting
            if (!divisor.IsGreaterThanOrEqual(dividend))
                return false;

            while(!divisor.IsGreaterThanOrEqual(dividend))
            {
                dividend.HasHooks<IDigit>().HasShift().ShiftRight();
                product.HasHooks<IDigit>().HasShift().ShiftRight();//apply same shift to the product
            }

            return true;
        }
        /// <summary>
        /// most from MSD inwards, finds the smallest number that is greater than the divisor,
        /// and returns the multiple and the number of shifts right (eg. order of magnitude of 
        /// the MSD digits)
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <param name="numShiftsRights"></param>
        /// <returns></returns>
        private static Numeric getMostSignificantDigits(Numeric dividend, Numeric divisor, out Numeric numShiftsRights)
        {
            //get MSD's that are greater than divisor
            //eg.  7 into 120
            //      returns 1 - as 7 goes into 12 1 time
            //      and returns shifts of 1

            var portion = dividend.GetCompatibleZero();

            dividend.Filter(digitnode=>{

                

            }, false);
        }
        private static Numeric calcDivisorMultiple(Numeric dividend, Numeric divisor)
        {
            //get MSD's that are greater than 
        }
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
            
            //divide by zero check
            if (divisor.IsEqualTo(zero))
                throw new InvalidOperationException("divide by zero");

            //decimal place check
            if (toNumberOfDecimalPlaces != null)
            {
                if (product.GetDecimalPlaces().IsGreaterThan(toNumberOfDecimalPlaces))
                {
                    //truncate and return
                    product.TruncateToDecimalPlaces(toNumberOfDecimalPlaces);
                    return;
                }
            }
            
            //shift the dividend
            shiftDividendToGreaterThanDivisor(dividend, divisor, product);

                //find out how many times divisor fits into the dividend
                Numeric counter = new Numeric(product.NumberSystem.ZeroSymbol, product.NumberSystem);
                while (dividend.IsGreaterThan(divisor)) 
                {
                    dividend.Subtract(divisor);
                    counter.AddOne();
                }

                //record the count as least significant digits
                counter.Iterate(digit =>
                {
                    DigitNode dNode = digit as DigitNode;
                    product.AddLeastSignificantDigit(dNode.Symbol);
                }, false);


                //recurse if the dividend is greater than zero
                if (dividend.IsGreaterThan(zero))
                {
                    divide(dividend, divisor, product, toNumberOfDecimalPlaces);
                }
                else
                {
                    //we're done!
                    return product;
                }
            }
            return product;
        
    }

    public static class DividingNumberDecorationExtensions
    {
        public static DividingNumericDecoration HasDivision(this object number)
        {
            return DividingNumericDecoration.New(number);
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
        public static void IterateMSDs(this Numeric numeric,
            Func<INumeric, INumeric, bool> filter)
        {
            numeric.Filter(node =>
            {
                //first get the number portion
                DigitNode dNode = node as DigitNode;
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

            int topLimit = 10000000;
            for (int x = 0; x < topLimit; x++)
            {
                for (int y = 0; y < topLimit; y++)
                {
                    var num1 = Numeric.New(set, x.ToString() ).HasMultiplication();

                    int res = x * y;
                    num1.Multiply(y.ToString());

                    Debug.Assert(num1.SymbolsText == res.ToString());
                }
            }


        }

    }
}
