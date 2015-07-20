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
                    //add each to itself, each2 times
                    var total =  Numeric.New(this.NumberSystem, each).HasAddition();
                    var counter = Numeric.New(this.NumberSystem, each2).HasAddition();
                    counter.CountdownToZero(c =>
                    {
                        total.Add(total);
                    });

                    //register it
                    this._multMap.Add(each, each2, total.InnerNumeric);
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
            var val = this._multMap.Get(digit1, digit2);

            var rv = val.Clone().HasHooks<IDigit>().HasShift();
            var counter = digit1Pos.HasAddition();
            counter.CountdownToZero((c) =>
            {
                rv.ShiftLeft();
            });
            counter = digit2Pos.HasAddition();
            counter.CountdownToZero((c) =>
            {
                rv.ShiftLeft();
            });
            return rv.InnerNumeric;
        }
        #endregion

        #region IHasMultiplication
        public void Multiply(string number)
        {
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
                var arg = Numeric.New(this.NumberSystem, number).HasHooks<IDigit>().HasShift();
                var thisNum = this.InnerNumeric.Clone().HasHooks<IDigit>().HasShift();
                var sum = Numeric.New(this.NumberSystem, this.NumberSystem.ZeroSymbol).HasHooks<IDigit>().HasShift().HasAddition();

                //shift both arg and thisnum to zero, to eliminate clarity with decimal shift handling
                //we'll shift back at the end.  this way we're only multiplying whole numbers.
                //also makes the code cleaner

                var argShifts = arg.ShiftToZero();
                var thisNumShifts = thisNum.ShiftToZero();

                this.InnerNumeric.ZoneIterateWithIndex((thisDigit, thisIdx) =>
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

                this.InnerNumeric.SetValue(sum);
            }
        }
        #endregion
    }

    public static class MultiplyingNumberDecorationExtensions
    {
        public static MultiplyingNumericDecoration HasMultiplication(this INumeric number)
        {
            return MultiplyingNumericDecoration.New(number);
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

            int topLimit= 100;
            for (int x = 0; x < topLimit; x++)
            {
                for (int y = 0; y < topLimit; y++)
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
