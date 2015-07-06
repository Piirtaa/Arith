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
            return thisNumber.Compare(number).Equals(null);
        }
        public static bool IsGreaterThan(this ISymbolicNumber thisNumber, ISymbolicNumber number)
        {
            return thisNumber.Compare(number).Equals(true);
        }
        public static bool IsLessThan(this ISymbolicNumber thisNumber, ISymbolicNumber number)
        {
            return thisNumber.Compare(number).Equals(false);
        }
    }
}
