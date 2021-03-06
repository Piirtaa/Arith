﻿using System;
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
    #region Step Data Structures
    public class AdditionStep
    {
        #region Ctor
        public AdditionStep(Numeric arg1, Numeric arg2)
        {
            this.Arg1 = arg1;
            this.Arg2 = arg2;
            this.CarryLine = arg1.GetCompatibleZero();

            //get the order of magnitudes
            List<INumeric> list = new List<INumeric>();
            if(arg1 != null)
                list.Add(arg1);

            if(arg2 != null)
                list.Add(arg2);

            list.g
        }
        #endregion

        #region Static Builder
        public static AdditionStep New(Numeric arg1, Numeric arg2)
        {
            return new AdditionStep(arg1, arg2);
        }
        #endregion

        #region Properties
        public Numeric Arg1 { get; private set; }
        public Numeric Arg2 { get; private set; }
        public Numeric CarryLine { get; private set;}
        public Numeric Result { get; private set; }
        #endregion
    }

    public class AddDigitOperationLine
    {
        #region Ctor
        public AddDigitOperationLine(Numeric number1, Numeric number2, Numeric orderOfMagnitude)
        {
            this.OrderOfMagnitude = orderOfMagnitude;
            var digit1 = number1.GetDigitAtMagnitude(orderOfMagnitude);
            var digit2 = number2.GetDigitAtMagnitude(orderOfMagnitude);

            var newDigit1 = number1.GetCompatibleNumber(digit1.Symbol);
            var newDigit2 = number2.GetCompatibleNumber(digit2.Symbol);
            var result = newDigit1.Clone();
            result.HasAddition().Add(newDigit2);
            
            this.Digit1 = digit1.Symbol;
            this.Digit2 = digit2.Symbol;
            this.ResultDigit = result.ZerothDigit.Value.Symbol;
            if(result.ZerothDigit.NextNode != null)
            {
                this.CarryDigit = result.ZerothDigit.NextDigit().Value.Symbol;
            }
        }
        #endregion

        #region Static Builder
        public static AddDigitOperationLine New(Numeric number1, Numeric number2, Numeric orderOfMagnitude)
        {
            return new AddDigitOperationLine(number1, number2, orderOfMagnitude);
        }
        #endregion

        #region Properties
        public Numeric OrderOfMagnitude { get; private set; }
        public string Digit1 { get; private set; }
        public string Digit2 { get; private set; }
        public string CarryDigit { get; private set; }
        public string ResultDigit { get; private set; }
        #endregion

        #region Calculated Properties
        public string DigitOperationDescription
        {
            get
            {
                var num1 = this.OrderOfMagnitude.GetNumberAtMagnitude(this.Digit1);
                var num2 = this.OrderOfMagnitude.GetNumberAtMagnitude(this.Digit2);
                var sum =num1.Clone();
                sum.HasAddition().Add(num2);

                var rv = string.Format("{0} + {1} = {2}", num1.GetSimpleFormat(),
                    num2.GetSimpleFormat(), sum.GetSimpleFormat());
                return rv;
            }
        }
        public string CarryDescription
        {
            get
            {
                if(string.IsNullOrEmpty(this.CarryDigit))
                    return string.Empty;

                var rv = string.Format("carries {0}", this.CarryDigit, null);
                return rv;
            }
        }
        #endregion
    }
    #endregion

    public interface IHasAdditionSteps : INumericDecoration
    {
        AdditionStep Add(INumeric numeric);
        AdditionStep Subtract(INumeric numeric);
    }

    public class AddingStepsNumericDecoration : NumericDecorationBase, IHasAddition
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

            bool isClone = false;
            var val = Add(this, numeric, out isClone);

            if (isClone)
                this.As<Numeric>().SetValue(val);
        }
        public void Subtract(INumeric numeric)
        {
            if (numeric == null)
                return;

            //if (!this.HasCompatibleNumberSystem(numeric))
            //    throw new InvalidOperationException("incompatible number system");


            numeric.As<Numeric>().SwitchSign();
            bool isClone = false;
            var val = Add(this, numeric, out isClone);
            numeric.As<Numeric>().SwitchSign(); //we don't want to return a modified arg

            if (isClone)
                this.As<Numeric>().SetValue(val);
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
        private static INumeric Add(INumeric number1, INumeric number2, out bool isReplace)
        {
            //the default value is false, as most cases don't need a clone operation
            isReplace = false;

            if (number1 == null)
            {
                isReplace = true;
                return number2;
            }
            if (number2 == null)
                return number1;

            INumeric rv = null;

            //determine the longer number
            bool? num1IsLonger = Numeric.AbsoluteValueCompare(number1.As<Numeric>(),
                number2.As<Numeric>());

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
                        isReplace = true;
                        var cloneNum2 = number2.Clone();
                        rv = Decrement(cloneNum2, number1);
                        rv.As<Numeric>().IsPositive = false;
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
                        isReplace = true;
                        rv = Decrement(number2.Clone(), number1);
                        rv.As<Numeric>().IsPositive = true;
                        break;
                }
            }
            else if (number1.IsPositive == false && number2.IsPositive == false)
            {
                rv = Increment(number1, number2);
                rv.As<Numeric>().IsPositive = false;
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
            var addNode1 = number1.ZerothDigit as DigitNode;
            var addNode2 = number2.ZerothDigit as DigitNode;

            //add after the decimal
            while (addNode2 != null)
            {
                addNode1.Add(addNode2.Value.Symbol);

                addNode2 = addNode2.NextNode as DigitNode;
                if (addNode2 != null)
                    addNode1 = addNode1.LoadNextDigit;
            }

            //add before the decimal
            addNode2 = number2.ZerothDigit.PreviousNode as DigitNode;

            if (addNode2 != null)
                addNode1 = (number1.ZerothDigit as DigitNode).LoadPreviousDigit;

            while (addNode2 != null)
            {
                addNode1.Value.Add(addNode2.Symbol);

                addNode2 = addNode2.PreviousNode as DigitNode;
                if (addNode2 != null)
                    addNode1 = addNode1.LoadPreviousDigit;
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
            var addNode1 = number1.ZerothDigit as DigitNode;
            var addNode2 = number2.ZerothDigit as DigitNode;

            //add after the decimal
            while (addNode2 != null)
            {
                addNode1.Subtract(addNode2.Symbol);

                addNode2 = addNode2.NextNode as DigitNode;
                if (addNode2 != null)
                    addNode1 = addNode1.LoadNextDigit;
            }

            //add before the decimal
            addNode2 = number2.ZerothDigit.PreviousNode as DigitNode;

            if (addNode2 != null)
                addNode1 = (number1.ZerothDigit as DigitNode).LoadPreviousDigit;

            while (addNode2 != null)
            {
                addNode1.Value.Subtract(addNode2.Symbol);

                addNode2 = addNode2.PreviousNode as DigitNode;
                if (addNode2 != null)
                    addNode1 = addNode1.LoadPreviousDigit;
            }
            return number1;
        }

        #endregion
    }

    public static class AddingNumericDecorationExtensions
    {
        public static AddingNumericDecoration HasAdditionSteps(this object number)
        {
            var decoration = number.ApplyDecorationIfNotPresent<AddingNumericDecoration>(x =>
            {
                return AddingNumericDecoration.New(number);
            });

            return decoration;
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

                var mag = numA.GetDigitMagnitude(node as DigitNode);
                Debug.WriteLine("digit order of mag " + mag.SymbolsText);

                var pos = numA.GetDigitPosition(node as DigitNode);
                Debug.WriteLine("digit pos " + pos.SymbolsText);
            }, (node, idx) =>
            {
                Debug.WriteLine("on number {0} digit {1} idx {2}", numA.SymbolsText,
    node.Value.Symbol, idx.SymbolsText);

                var mag = numA.GetDigitMagnitude(node as DigitNode);
                Debug.WriteLine("digit order of mag " + mag.SymbolsText);

                var pos = numA.GetDigitPosition(node as DigitNode);
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
