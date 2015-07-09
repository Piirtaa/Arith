using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Decorating;
using System.Runtime.Serialization;
using Arith.DataStructures;

namespace Arith.Domain.Decorations
{
    public interface IHasDivision : INumberDecoration
	{
		void Divide(string number, string numberOfDecimalPlaces); 
	}

    public class DividingNumberDecoration : NumberDecorationBase, IHasDivision
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public DividingNumberDecoration(INumber decorated)
            : base(decorated)
        {
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
        public override IDecorationOf<INumber> ApplyThisDecorationTo(INumber thing)
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

            //we use a cloned value of the dividend because it will be subtracted from at each step
            //and we don't want to change the passed in reference object - treat it as if it were a value type
            return divide(SymbolicNumber.Clone(dividend), divisor, product, toNumberOfDecimalPlaces);
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


}
