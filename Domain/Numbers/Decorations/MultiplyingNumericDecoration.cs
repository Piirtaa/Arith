using System;
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
    public interface IHasMultiplication : INumericDecoration
    {
        void Multiply(string number);
    }

    public class MultiplyingNumericDecoration : NumericDecorationBase, IHasMultiplication
    {
        #region Declarations
        private readonly object _stateLock = new object();
        private SquareLookup<Numeric> _multMap = null;
        #endregion

        #region Ctor
        public MultiplyingNumericDecoration(object decorated)
            : base(decorated)
        {
            this.InitMap();
        }
        #endregion

        #region Static
        public static MultiplyingNumericDecoration New(object decorated)
        {
            return new MultiplyingNumericDecoration(decorated);
        }
        #endregion

        #region ISerializable
        protected MultiplyingNumericDecoration(SerializationInfo info, StreamingContext context)
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
            return new MultiplyingNumericDecoration(thing);
        }
        #endregion

        #region Helpers
        private void InitMap()
        {
            var keys = this.NumberSystem.Symbols;
            this._multMap = new SquareLookup<Numeric>(keys);

            foreach (string each in keys)
            {
                foreach (string each2 in keys)
                {
                    //Debug.WriteLine("creating entry for {0} x {1}", each, each2);

                    //add each to itself, each2 times
                    var eachNum = Numeric.New(this.NumberSystem, each);
                    var counter = Numeric.New(this.NumberSystem, each2).HasAddition();
                    var total = eachNum.GetCompatibleZero().HasAddition();

                    counter.PerformThisManyTimes(c =>
                    {
                        //Debug.WriteLine("interim total for {0} x {1} = {2} ", each, each2, total.SymbolsText);
                        total.Add(eachNum);
                    });

                    //register it
                    this._multMap.Add(each, each2, total.InnerNumeric);
                    //Debug.WriteLine("creating entry for {0} x {1} = {2}", each, each2, total.InnerNumeric.SymbolsText);
                }
            }
        }
        /// <summary>
        /// multiplies 2 digits given their positions
        /// </summary>
        /// <param name="digit1"></param>
        /// <param name="digit2"></param>
        /// <param name="digit1Pos"></param>
        /// <param name="digit2Pos"></param>
        /// <returns></returns>
        private Numeric MultiplyDigits(string digit1, string digit2,
            Numeric digit1Pos, Numeric digit2Pos)
        {
            Debug.WriteLine("multiplying digits {0} and {1} at positions {2} and {3}",
                digit1, digit2, digit1Pos.SymbolsText, digit2Pos.SymbolsText);

            var val = this._multMap.Get(digit1, digit2);
            Debug.WriteLine("{0} * {1} = {2}",
                    digit1, digit2, val.SymbolsText);

            var rv = val.Clone().HasShift();
            var number1 = Numeric.New(this.NumberSystem, digit1).HasShift();
            var number2 = Numeric.New(this.NumberSystem, digit2).HasShift();

            var counter = digit1Pos.HasAddition();
            rv.ShiftRight(counter);
            number1.ShiftRight(counter);

            counter = digit2Pos.HasAddition();
            rv.ShiftRight(counter);
            number2.ShiftRight(counter);

            Debug.WriteLine("equivalent to {0} * {1} = {2}",
                    number1.SymbolsText, number2.SymbolsText, rv.SymbolsText);
            return rv.InnerNumeric;
        }
        #endregion

        #region IHasMultiplication
        public void Multiply(string number)
        {
            Debug.WriteLine("multiplying {0} * {1}", this.SymbolsText, number);

            /*
             * Xn, Xn-1,...,X1, X0
             * Yn, Yn-1,...,Y1, Y0
             * 
             * To multiply this process is followed:
             * 
             * foreach X digit
             *  foreach Y digit
             *      multiply digits to get a Number
             *      shift left X position + Y position times
             *          (eg. Y0 * Xn-1, shift left n-1 times)
             *      add to total
             * 
             */

            lock (this._stateLock)
            {
                var arg = Numeric.New(this.NumberSystem, number).HasShift();
                var thisNum = this.InnerNumeric.Clone().HasShift();
                var sum = Numeric.New(this.NumberSystem, this.NumberSystem.ZeroSymbol).HasShift().HasAddition();

                //shift both arg and thisnum to zero, to eliminate clarity with decimal shift handling
                //we'll shift back at the end.  this way we're only multiplying whole numbers.
                //also makes the code cleaner

                var argShifts = arg.ShiftToZero();
                var thisNumShifts = thisNum.ShiftToZero();

                this.ZoneIterateWithIndex((thisDigit, thisIdx) =>
                {
                    arg.ZoneIterateWithIndex((argDigit, argIdx) =>
                    {
                        var digitProduct = this.MultiplyDigits(argDigit.Value.Symbol,
                            thisDigit.Value.Symbol, argIdx, thisIdx);

                        sum.Add(digitProduct);
                    }, (argDigit, argIdx) =>
                    {
                        //no operation will happen here because we've shifted to zero
                    });
                }, (thisDigit, thisIdx) =>
                {
                    //no operation will happen here because we've shifted to zero
                });

                //shift back
                var shifty = sum.AsBelow<IHasShift>(false);
                shifty.ShiftLeft(argShifts).ShiftLeft(thisNumShifts);

                this.DecoratedOf.SetValue(sum);
            }
        }
        #endregion
    }

    public static class MultiplyingNumberDecorationExtensions
    {
        public static MultiplyingNumericDecoration HasMultiplication(this object number)
        {
            var decoration = number.ApplyDecorationIfNotPresent<MultiplyingNumericDecoration>(x =>
            {
                return MultiplyingNumericDecoration.New(number);
            });

            return decoration;
        }
    }


    public class MultiplyingNumberTests
    {
        public static void Test()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }

            int topLimit = 100;
            for (int x = 1; x < topLimit; x++)
            {
                for (int y = 1; y < topLimit; y++)
                {
                    var num1 = Numeric.New(set, x.ToString()).HasMultiplication();

                    int res = x * y;
                    num1.Multiply(y.ToString());

                    Debug.Assert(num1.SymbolsText == res.ToString());
                }
            }


        }
    }
}
