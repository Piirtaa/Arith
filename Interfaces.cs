using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Domain;

namespace Arith
{
 
    public interface INumber
    {
        /// <summary>
        /// the number in symbolic form
        /// </summary>
        string SymbolsText { get; }
        void SetValue(string number);
        void Add(string number);
        void Subtract(string number);
        bool IsPositive { get; }
        NumeralSet NumberSystem { get; }

        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        bool? Compare(string number);
    }

    public static class NumberExtensions
    {
        public static bool IsEqualTo(this Number thisNumber, string number)
        {
            return thisNumber.Compare(number).Equals(null);
        }
        public static bool IsGreaterThan(this Number thisNumber, string number)
        {
            return thisNumber.Compare(number).Equals(true);
        }
        public static bool IsLessThan(this Number thisNumber, string number)
        {
            return thisNumber.Compare(number).Equals(false);
        }
    }
}
