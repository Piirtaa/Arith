using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Arith.DataStructures;
using Arith.Decorating;
using Arith.Domain.Digits;
using Arith.Extensions;

namespace Arith.Domain.Numbers.Decorations
{
    /*A note about this decoration.  We prefer to use INumeric instead of the concrete Numeric type
     * in the implementation, because it is a common interface between all decorations.
     * That said, we assume the underlying INumeric type to be a concrete Numeric.  Thus we
     * also assume DigitNode is the node type.  The concrete Numeric instance can be
     * accessed via the .As<Numeric>() "decorative cast" to find the appropriate instance.
     * Or it can be accessed through the DigitNode node instances, via the backref property.  
     * Either way we can ensure a reference to the core.
       * 
     */

    public interface IHasAddition : INumericDecoration
    {
        void Add(INumeric numeric);
        void Subtract(INumeric numeric);
    }

    public class AddingNumericDecoration : NumericDecorationBase, IHasAddition
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public AddingNumericDecoration(object decorated)
            : base(decorated)
        {
        }
        #endregion

        #region Static
        public static AddingNumericDecoration New(object decorated)
        {
            return new AddingNumericDecoration(decorated);
        }
        #endregion

        #region ISerializable
        protected AddingNumericDecoration(SerializationInfo info, StreamingContext context)
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
            return new AddingNumericDecoration(thing);
        }
        #endregion

        #region IHasAddition
        public void Add(INumeric numeric)
        {
            if (numeric == null)
                return;

            //if (!this.HasCompatibleNumberSystem(numeric))
            //    throw new InvalidOperationException("incompatible number system");
            var num2 = numeric.GetInnerNumeric().Clone();
            var num1 = this.GetInnerNumeric().Clone();
            var val = Add(num1, num2);
            this.DecoratedOf.SetValue(val);
        }
        public void Subtract(INumeric numeric)
        {
            if (numeric == null)
                return;

            //if (!this.HasCompatibleNumberSystem(numeric))
            //    throw new InvalidOperationException("incompatible number system");
            var num2 = numeric.GetInnerNumeric().Clone() as Numeric;
            num2.SwitchSign();
            var num1 = this.GetInnerNumeric().Clone();
            var val = Add(num1, num2);
            this.DecoratedOf.SetValue(val);
        }
        #endregion

        #region Static Helpers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="number1"></param>
        /// <param name="number2"></param>
        /// <param name="setValueIndicated">this is true if the return value is a clone and not the 
        /// same instance as number1</param>
        /// <returns></returns>
        private static INumeric Add(INumeric number1, INumeric number2)
        {
            if (number1 == null)
            {
                return number2;
            }
            if (number2 == null)
                return number1;

            INumeric rv = null;

            //determine the longer number
            bool? num1IsLonger = Numeric.AbsoluteValueCompare(number1.GetInnerNumeric(),
                number2.GetInnerNumeric());

            //note: below we clone the 2nd arg if it is being modified as part of the operation
            //we don't clone the 1st arg as it is 
            if (number1.IsPositive && number2.IsPositive)
            {
                rv = Increment(number1, number2);
            }
            else if (number1.IsPositive && number2.IsPositive == false)
            {
                switch (num1IsLonger)
                {
                    case null:
                        rv = Decrement(number1, number2);
                        break;
                    case true:
                        rv = Decrement(number1, number2);
                        break;
                    case false:
                        rv = Decrement(number2, number1);
                        rv.GetInnerNumeric().IsPositive = false;
                        break;
                }
            }
            else if (number1.IsPositive == false && number2.IsPositive)
            {
                switch (num1IsLonger)
                {
                    case null:
                        rv = Decrement(number1, number2);
                        break;
                    case true:
                        rv = Decrement(number1, number2);
                        break;
                    case false:
                        rv = Decrement(number2, number1);
                        rv.GetInnerNumeric().IsPositive = true;
                        break;
                }
            }
            else if (number1.IsPositive == false && number2.IsPositive == false)
            {
                rv = Increment(number1, number2);
                rv.GetInnerNumeric().IsPositive = false;
            }

            return rv;
        }
        /// <summary>
        /// treats numbers as signless, and increase the value of the host number by the amount
        /// </summary>
        /// <param name="number"></param>
        /// <param name="number2"></param>
        private static INumeric Increment(INumeric number1, INumeric number2)
        {
            if (number1 == null)
                throw new ArgumentNullException("number1");

            if (number2 == null)
                throw new ArgumentNullException("number2");

            //the add process
            var addNode1 = number1.ZerothDigit;
            var addNode2 = number2.ZerothDigit;

            //add after the decimal
            while (addNode2 != null)
            {
                addNode1.Add(addNode2.Value.Symbol);

                addNode2 = addNode2.NextNode as IDigitNode;
                if (addNode2 != null)
                    addNode1 = addNode1.LoadNextDigit();
            }

            //add before the decimal
            addNode2 = number2.ZerothDigit.PreviousDigit();

            if (addNode2 != null)
                addNode1 = (number1.ZerothDigit).LoadPreviousDigit();

            while (addNode2 != null)
            {
                addNode1.Value.Add(addNode2.Value.Symbol);

                addNode2 = addNode2.PreviousDigit();
                if (addNode2 != null)
                    addNode1 = addNode1.LoadPreviousDigit();
            }
            return number1;
        }
        /// <summary>
        /// treats numbers as signless.  assumes number1 is longer than number 2
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decnumber"></param>
        /// <returns></returns>
        private static INumeric Decrement(INumeric number1, INumeric number2)
        {
            if (number1 == null)
                throw new ArgumentNullException("number1");

            if (number2 == null)
                throw new ArgumentNullException("number2");

            //the add process
            var addNode1 = number1.ZerothDigit;
            var addNode2 = number2.ZerothDigit;

            //add after the decimal
            while (addNode2 != null)
            {
                addNode1.Subtract(addNode2.Value.Symbol);

                addNode2 = addNode2.NextDigit();
                if (addNode2 != null)
                    addNode1 = addNode1.LoadNextDigit();
            }

            //add before the decimal
            addNode2 = number2.ZerothDigit.PreviousDigit();

            if (addNode2 != null)
                addNode1 = number1.ZerothDigit.LoadPreviousDigit();

            while (addNode2 != null)
            {
                addNode1.Value.Subtract(addNode2.Value.Symbol);

                addNode2 = addNode2.PreviousDigit();
                if (addNode2 != null)
                    addNode1 = addNode1.LoadPreviousDigit();
            }
            return number1;
        }

        #endregion
    }

    public static class AddingNumericDecorationExtensions
    {
        public static AddingNumericDecoration HasAddition(this object number)
        {
            var decoration = number.ApplyDecorationIfNotPresent<AddingNumericDecoration>(x =>
            {
                return AddingNumericDecoration.New(number);
            });

            return decoration;
        }

        public static void AddOne(this INumeric thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            thisNumber.HasAddition().Add(thisNumber.GetCompatibleOne());
        }
        public static void SubtractOne(this INumeric thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            thisNumber.HasAddition().Subtract(thisNumber.GetCompatibleOne());
        }

        /// <summary>
        /// performs the action for as many times as the number
        /// </summary>
        /// <param name="number"></param>
        /// <param name="action"></param>
        public static void PerformThisManyTimes(this INumeric number,
            Action<INumeric> action)
        {
            if (number == null) return;
            if (action == null) throw new ArgumentNullException("action");

            if (!number.IsPositive)
                throw new ArgumentOutOfRangeException("number must be positive");

            var zero = number.GetCompatibleZero();

            AddingNumericDecoration num = number.GetInnerNumeric().Clone().HasAddition();
            while (num.IsGreaterThan(zero))
            {
                action(num);
                num.SubtractOne();
            }
        }

        /// <summary>
        /// returns the lengths of the number
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <param name="wholeNumberLength"></param>
        /// <param name="decimalLength"></param>
        public static void GetNumericLengths(this INumeric thisNumber,
            out Numeric wholeNumberLength,
            out Numeric decimalLength)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            var counter1 = thisNumber.GetCompatibleZero().HasAddition();
            var counter2 = thisNumber.GetCompatibleZero().HasAddition();
            thisNumber.ZoneIterate((node) =>
            {
                counter1.AddOne();
            }, (node) =>
            {
                counter2.AddOne();
            }, false);

            wholeNumberLength = counter1.InnerNumeric;
            decimalLength = counter2.InnerNumeric;
        }

        /// <summary>
        /// returns the position of the node.  zero is the first node.  irrespective of
        /// zeroth node/decimal place
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static Numeric GetDigitPosition(this INumeric thisNumber,
            IDigitNode digit)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            var pos = thisNumber.GetCompatibleZero().HasAddition();

            thisNumber.Filter((node) =>
            {
                if (object.ReferenceEquals(digit, node))
                    return true;

                pos.AddOne();
                return false;
            }, true);

            return pos.InnerNumeric;
        }

        /// <summary>
        /// 0 is Zeroth, positive values are post-decimal digits, negative values are
        /// pre-decimal digits
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        public static Numeric GetDigitMagnitude(this INumeric thisNumber,
            IDigitNode digit)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            Numeric digitIdx = null;
            thisNumber.ZoneIterateWithIndex((node, idx) =>
            {
                if (object.ReferenceEquals(digit, node))
                    digitIdx = idx;
            }, (node, idx) =>
            {
                if (object.ReferenceEquals(digit, node))
                    digitIdx = idx;

            });

            return digitIdx;
        }

        /// <summary>
        /// zone iterates but includes the index as an additional strategy argument.
        /// 0 is Zeroth, positive values are post-decimal digits, negative values are
        /// pre-decimal digits
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <param name="postZeroAction"></param>
        /// <param name="preZeroAction"></param>
        /// <param name="towardsZero"></param>
        public static void ZoneIterateWithIndex(this INumeric thisNumber,
                Action<IDigitNode, Numeric> postZeroAction,
                Action<IDigitNode, Numeric> preZeroAction)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            var counter1 = thisNumber.GetCompatibleZero().HasAddition();
            var counter2 = thisNumber.GetCompatibleOne().HasAddition();
            counter2.InnerNumeric.SwitchSign();

            //note we iterate from zeroth outwards to msd to facilitate counting the index 
            thisNumber.ZoneIterate((node) =>
            {
                postZeroAction(node, counter1.InnerNumeric.Clone() as Numeric);
                counter1.AddOne();
            }, (node) =>
            {
                preZeroAction(node, counter2.InnerNumeric.Clone() as Numeric);
                counter2.SubtractOne();
            }, false);
        }

        /// <summary>
        /// creates a number using a digit and an order of magnitude
        /// </summary>
        /// <param name="orderOfMag"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        public static Numeric GetNumberAtMagnitude(this Numeric orderOfMag, string digit)
        {
            if (orderOfMag == null)
                throw new ArgumentNullException("orderOfMag");

            Numeric rv = orderOfMag.GetCompatibleNumber(digit);

            if (orderOfMag.IsEqualTo(orderOfMag.GetCompatibleZero()))
                return rv;

            if (orderOfMag.IsPositive)
            {
                rv.ShiftRight(orderOfMag);
            }
            else
            {
                var o = orderOfMag.Clone() as Numeric;
                o.SwitchSign();
                rv.ShiftLeft(o);
            }

            return rv;
        }

        /// <summary>
        /// creates a number using a digit and an order of magnitude
        /// </summary>
        /// <param name="orderOfMag"></param>
        /// <param name="digit"></param>
        /// <returns></returns>
        public static IDigitNode GetDigitAtMagnitude(this INumeric thisNumber,
            Numeric orderOfMag)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            if (orderOfMag == null)
                throw new ArgumentNullException("orderOfMag");

            if (orderOfMag.IsEqualTo(orderOfMag.GetCompatibleZero()))
                return thisNumber.ZerothDigit;

            IDigitNode rv = null;

            if (orderOfMag.IsPositive)
            {
                rv = thisNumber.ZerothDigit;
                orderOfMag.PerformThisManyTimes(x =>
                {
                    if (rv != null)
                        rv = rv.NextDigit();
                });
            }
            else
            {
                rv = thisNumber.ZerothDigit;
                orderOfMag.GetNegativeOf().PerformThisManyTimes(x =>
                {
                    if (rv != null)
                        rv = rv.PreviousDigit();
                });
            }

            return rv;
        }


        public static void SetDigitAtMagnitude(this INumeric thisNumber,
     Numeric orderOfMag, string digit)
        {
            var dig = thisNumber.GetDigitAtMagnitude(orderOfMag);
            dig.SetValue(digit);
        }
    }


    public class AddingNumberTests
    {
        public static void Test()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }

            var numA = Numeric.New(set, "1234567890.246");
            numA.ZoneIterateWithIndex((node, idx) =>
            {
                Debug.WriteLine("on number {0} digit {1} idx {2}", numA.SymbolsText,
                    node.Value.Symbol, idx.SymbolsText);

                var mag = numA.GetDigitMagnitude(node);
                Debug.WriteLine("digit order of mag " + mag.SymbolsText);

                var pos = numA.GetDigitPosition(node);
                Debug.WriteLine("digit pos " + pos.SymbolsText);
            }, (node, idx) =>
            {
                Debug.WriteLine("on number {0} digit {1} idx {2}", numA.SymbolsText,
    node.Value.Symbol, idx.SymbolsText);

                var mag = numA.GetDigitMagnitude(node);
                Debug.WriteLine("digit order of mag " + mag.SymbolsText);

                var pos = numA.GetDigitPosition(node);
                Debug.WriteLine("digit pos " + pos.SymbolsText);
            });


            int topLimit = 100;
            for (int x = 0; x < topLimit; x++)
            {
                for (int y = 0; y < topLimit; y++)
                {
                    var num1 = Numeric.New(set, x.ToString()).HasAddition();
                    var num2 = Numeric.New(set, y.ToString()).HasAddition();
                    int res = x + y;
                    num1.Add(num2);

                    Debug.Assert(num1.SymbolsText == res.ToString());
                    Debug.WriteLine("{0} + {1} = {2}", x, y, res);

                    num1.Subtract(num2);
                    int res2 = x;
                    Debug.Assert(num1.SymbolsText == res2.ToString());

                    num2.Subtract(num1);
                    int res3 = y - x;
                    Debug.Assert(num2.SymbolsText == res3.ToString());
                    Debug.WriteLine("{0} - {1} = {2}", y, x, res3);

                }
            }




        }

    }
}
