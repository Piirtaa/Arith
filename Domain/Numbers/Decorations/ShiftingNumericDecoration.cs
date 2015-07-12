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
    public interface IHasShift : INumericDecoration
    {
        /// <summary>
        /// moves the decimal to the right (towards LSD) - an order of magnitude increase.
        /// if the node doesn't exist, it is added.
        /// </summary>
        void ShiftLeft();

        /// <summary>
        /// moves the decimal to the left (toward MSD) - an order of magnitude decrease.
        /// if the node doesn't exist, it is added.
        /// </summary>
        void ShiftRight();
    }

    public class ShiftNumericDecoration : NumericDecorationBase, IHasShift
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public ShiftNumericDecoration(INumeric decorated)
            : base(decorated)
        {
        }
        #endregion

        #region Static
        public static ShiftNumericDecoration New(INumeric decorated)
        {
            return new ShiftNumericDecoration(decorated);
        }
        #endregion

        #region ISerializable
        protected ShiftNumericDecoration(SerializationInfo info, StreamingContext context)
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
        public override IDecoration ApplyThisDecorationTo(INumeric thing)
        {
            return new ShiftNumericDecoration(thing);
        }
        #endregion

        #region Methods
        /// <summary>
        /// moves the decimal to the right (towards LSD) - an order of magnitude increase.
        /// if the LSD rightmost position doesn't exist, it is added.
        /// </summary>
        public void ShiftRight()
        {
            lock (this._stateLock)
            {
                var node = this.ZerothDigit.PreviousNode as DigitNode;
                if (node == null)
                {
                    node = this.ThisNumeric.AddLeastSignificantZeroDigit();
                }

                //move the decimal
                this.ThisNumeric.ZerothDigit = node;
            }
        }
        /// <summary>
        /// moves the decimal to the left (toward MSD) - an order of magnitude decrease.
        /// if the leftmost(MSD) position doesn't exist, it is added.
        /// </summary>
        public void ShiftLeft()
        {
            lock (this._stateLock)
            {
                var node = this.ZerothDigit.NextNode as DigitNode;
                if (node == null)
                {
                    node = this.ThisNumeric.AddMostSignificantZeroDigit();
                }

                //move the decimal
                this.ThisNumeric.ZerothDigit = node;
            }
        }
        #endregion
    }

    public static class ShiftNumberDecorationExtensions
    {
        public static ShiftNumericDecoration HasShift(this INumeric decorated)
        {
            return ShiftNumericDecoration.New(decorated);
        }

        /// <summary>
        /// fluent.  makes larger by numShifts orders of magnitude
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <param name="numShifts"></param>
        /// <returns></returns>
        public static IHasShift ShiftLeft(this IHasShift thisNumber, IHasAddition numShifts)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            if (numShifts == null)
                throw new ArgumentNullException("numShifts");

            numShifts.CountdownToZero(c =>
            {
                thisNumber.ShiftLeft();
            });
            return thisNumber;
        }
        /// <summary>
        /// fluent.  makes smaller by numShifts orders of magnitude
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <param name="numShifts"></param>
        /// <returns></returns>
        public static IHasShift ShiftRight(this IHasShift thisNumber, IHasAddition numShifts)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            if (numShifts == null)
                throw new ArgumentNullException("numShifts");

            numShifts.CountdownToZero(c =>
            {
                thisNumber.ShiftRight();
            });
            return thisNumber;
        }
        /// <summary>
        /// shifts the number so that it has no decimal digits.  returns the shift length. 
        /// always a right shift. 
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <returns></returns>
        public static IHasAddition ShiftToZero(this IHasShift thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            var counter = thisNumber.GetCompatibleZero().HasAddition();
            while (thisNumber.ZerothDigit.HasPreviousDigit())
            {
                thisNumber.ShiftRight();
                counter.AddOne();
            }
            return counter;
        }

    }


    public class ShiftNumberTests
    {
        public static void Test()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }

            Numeric num = new Numeric(set, "123456789");
            var shiftNum = num.HasShift();

            shiftNum.ShiftToZero();
            Debug.Assert(num.SymbolsText == "123456789");

            shiftNum.ShiftLeft();
            Debug.Assert(num.SymbolsText == "1234567890");

            shiftNum.ShiftLeft();
            Debug.Assert(num.SymbolsText == "12345678900");

            shiftNum.ShiftRight();
            Debug.Assert(num.SymbolsText == "1234567890");

            shiftNum.ShiftRight();
            Debug.Assert(num.SymbolsText == "123456789");

            for (int i = 0; i < 20; i++)
            {
                var oldNum = shiftNum.Clone();
                shiftNum.ShiftRight();
                Debug.Assert(oldNum.IsGreaterThan(shiftNum));
            }
        }

    }
}
