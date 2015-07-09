using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arith.Domain
{
    /// <summary>
    /// describes the core digit info
    /// </summary>
    public interface IDigit
    {
        string Symbol { get; }

        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        bool? Compare(string symbol);
        bool Add(string symbol);
        bool Subtract(string symbol);
        bool AddOne();
        bool SubtractOne();
        void SetValue(string symbol);
    }

    public static class IDigitExtensions
    {
        public static bool IsEqualTo(this IDigit thisDigit, string symbol)
        {
            return thisDigit.Compare(symbol).Equals(null);
        }
        public static bool IsGreaterThan(this IDigit thisDigit, string symbol)
        {
            return thisDigit.Compare(symbol).Equals(true);
        }
        public static bool IsLessThan(this IDigit thisDigit, string symbol)
        {
            return thisDigit.Compare(symbol).Equals(false);
        }
    }

    public interface ISymbolicNumber
    {
        /// <summary>
        /// the number in symbolic form
        /// </summary>
        string SymbolsText { get; }
        void SetValue(string number);
        void Add(ISymbolicNumber number);
        void Subtract(ISymbolicNumber number);
        bool IsPositive { get; }
        NumeralSet NumberSystem { get; }

        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        bool? Compare(ISymbolicNumber number);
    }

    public static class SymbolicNumberExtensions
    {
        public static bool IsEqualTo(this ISymbolicNumber thisNumber, ISymbolicNumber number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return thisNumber.Compare(number).Equals(null);
        }
        public static bool IsGreaterThan(this ISymbolicNumber thisNumber, ISymbolicNumber number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return thisNumber.Compare(number).Equals(true);
        }
        public static bool IsLessThan(this ISymbolicNumber thisNumber, ISymbolicNumber number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return thisNumber.Compare(number).Equals(false);
        }
        public static void AddOne(this ISymbolicNumber thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            thisNumber.Add(new SymbolicNumber(thisNumber.NumberSystem.OneSymbol, thisNumber.NumberSystem));
        }
        public static void SubtractOne(this ISymbolicNumber thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            thisNumber.Subtract(new SymbolicNumber(thisNumber.NumberSystem.OneSymbol, thisNumber.NumberSystem));
        }
        /// <summary>
        /// fluent
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <param name="numShifts"></param>
        /// <returns></returns>
        public static SymbolicNumber ShiftLeft(this SymbolicNumber thisNumber, SymbolicNumber numShifts)
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
        /// fluent
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <param name="numShifts"></param>
        /// <returns></returns>
        public static SymbolicNumber ShiftRight(this SymbolicNumber thisNumber, SymbolicNumber numShifts)
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
        /// always a left shift. 
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <returns></returns>
        public static SymbolicNumber ShiftToZero(this SymbolicNumber thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            SymbolicNumber counter = new SymbolicNumber(thisNumber.NumberSystem.ZeroSymbol, thisNumber.NumberSystem);
            while (thisNumber.ZerothDigit.HasPreviousDigit)
            {
                thisNumber.ShiftLeft();
                counter.AddOne();
            }
            return counter;
        }
        /// <summary>
        /// performs the action for as many times as the number
        /// </summary>
        /// <param name="number"></param>
        /// <param name="action"></param>
        public static void CountdownToZero(this ISymbolicNumber number, Action<ISymbolicNumber> action)
        {
            if (number == null) return;
            if (action == null) throw new ArgumentNullException("action");

            if (!number.IsPositive)
                throw new ArgumentOutOfRangeException("number must be positive");

            var zero = new SymbolicNumber(number.NumberSystem.ZeroSymbol, number.NumberSystem);
            
            var num = SymbolicNumber.Clone(number as SymbolicNumber);
            while (num.IsGreaterThan(zero))
            {
                action(num);
                number.SubtractOne();
            }
        }

        public static SymbolicNumber GetDecimalPlaces(this SymbolicNumber thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            var places = thisNumber.ShiftToZero();
            thisNumber.ShiftRight(places);

            return places;
        }
        public static void TruncateToDecimalPlaces(this SymbolicNumber thisNumber, 
            SymbolicNumber places)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            if (places == null)
                throw new ArgumentNullException("places");

            var currPlaces = thisNumber.GetDecimalPlaces();
            if (currPlaces.IsGreaterThan(places))
            {
                currPlaces.Subtract(places);

                currPlaces.CountdownToZero(x =>
                {
                    thisNumber.Remove(thisNumber.LastNode);
                });
            }
        }
    }
}
