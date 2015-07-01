using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arith
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
}
