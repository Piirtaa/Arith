using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Arith.DataStructures;
using Arith.Decorating;
using Arith.Domain.Digits;

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
        public AddingNumericDecoration(INumeric decorated)
            : base(decorated)
        {
        }
        #endregion

        #region Static
        public static AddingNumericDecoration New(INumeric decorated)
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
            return new AddingNumericDecoration(thing as INumeric);
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

            if(addNode2 != null)
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
        public static AddingNumericDecoration HasAddition(this INumeric number)
        {
            //if there's already an addition decoration, return that
            var decoration = number.As<AddingNumericDecoration>(true);
            if(decoration == null)
                decoration = AddingNumericDecoration.New(number);

            return decoration;
        }

        public static void AddOne(this IHasAddition thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            thisNumber.Add(thisNumber.GetCompatibleOne());
        }
        public static void SubtractOne(this IHasAddition thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            thisNumber.Subtract(thisNumber.GetCompatibleOne());
        }

        /// <summary>
        /// performs the action for as many times as the number
        /// </summary>
        /// <param name="number"></param>
        /// <param name="action"></param>
        public static void CountdownToZero(this IHasAddition number, Action<INumeric> action)
        {
            if (number == null) return;
            if (action == null) throw new ArgumentNullException("action");

            if (!number.IsPositive)
                throw new ArgumentOutOfRangeException("number must be positive");

            var zero = number.GetCompatibleZero();

            AddingNumericDecoration num = (number as AddingNumericDecoration).Clone() as AddingNumericDecoration;
            while (num.IsGreaterThan(zero))
            {
                action(num);
                num.SubtractOne();
            }
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
                }
            }


        }

    }
}
