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
    public interface IHasDivision : INumericDecoration
	{
		void Divide(string number, string numberOfDecimalPlaces); 
	}

    public class DividingNumberDecoration : NumericDecorationBase, IHasDivision
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public DividingNumberDecoration(INumeric decorated)
            : base(decorated)
        {
        }
        #endregion

        #region Static
        public static DividingNumberDecoration New(INumeric decorated)
        {
            return new DividingNumberDecoration(decorated);
        }
        #endregion

        #region ISerializable
        protected DividingNumberDecoration(SerializationInfo info, StreamingContext context)
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
            return new MultiplyingNumberDecoration(thing);
        }
        #endregion

        #region IHasDivision
        public void Divide(string number, string numberOfDecimalPlaces)
        {
            lock (this._stateLock)
            {
                var rv = DivideWithPrecision(this.SymbolicNumber,
                    new SymbolicNumber(number, this.NumberSystem),
                    new SymbolicNumber(numberOfDecimalPlaces, this.NumberSystem));

                this.SymbolicNumber.SetValue(rv);
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
        public static SymbolicNumber DivideWithPrecision(SymbolicNumber dividend, SymbolicNumber divisor,
            SymbolicNumber toNumberOfDecimalPlaces)
        {
            var product = new SymbolicNumber(dividend.NumberSystem.ZeroSymbol, dividend.NumberSystem);
            product._isPositive = dividend.IsPositive;

            //shift dividend and divisor to zero, to eliminate clarity with decimal shift handling
            //we'll shift back at the end.  this way we're only dividing whole numbers.
            //also makes the code cleaner

            var dividendShifts = dividend.ShiftToZero();
            var divisorShifts = divisor.ShiftToZero();

            //we use a cloned value of the dividend because it will be subtracted from at each step
            //and we don't want to change the passed in reference object - treat it as if it were a value type
            var rv = divide(SymbolicNumber.Clone(dividend), divisor, product, toNumberOfDecimalPlaces);

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
        private static SymbolicNumber divide(SymbolicNumber dividend, SymbolicNumber divisor, 
            SymbolicNumber product, SymbolicNumber toNumberOfDecimalPlaces)
        {
            SymbolicNumber zero = new SymbolicNumber(product.NumberSystem.ZeroSymbol, product.NumberSystem);

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
                    return product;
                }
            }
            
            if (divisor.IsGreaterThan(dividend))
            {
                //can't divide a smaller number into a digit, 
                //so we have to add an order of magnitude to the dividend 
                //to make it greater than the divisor - a shift right.
                dividend.ShiftRight();

                //record this shift in the product as shift right in the product
                product.ShiftRight();

                //recurse
                divide(dividend, divisor, product, toNumberOfDecimalPlaces);
            }
            else
            {
                //find out how many times divisor fits into the dividend
                SymbolicNumber counter = new SymbolicNumber(product.NumberSystem.ZeroSymbol, product.NumberSystem);
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
    }

    public static class DividingNumberDecorationExtensions
    {
        public static DividingNumberDecoration HasDivision(this INumeric number)
        {
            return DividingNumberDecoration.New(number);
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
                    var num1 = Number.New(x.ToString(), set).HasMultiplication();

                    int res = x * y;
                    num1.Multiply(y.ToString());

                    Debug.Assert(num1.SymbolsText == res.ToString());
                }
            }


        }

    }
}
