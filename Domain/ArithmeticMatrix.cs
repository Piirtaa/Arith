using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.DataStructures;

namespace Arith.Domain
{
    /// <summary>
    /// for a given number set generates the addition, subtraction and compare tables
    /// for a digit.  
    /// </summary>
    public class ArithmeticMatrix
    {
        #region Declarations
        private NumeralSet _numeralSet = null;

        private SquareLookup<Tuple<string, bool>> _addMap = null;
        private SquareLookup<Tuple<string, bool>> _subtractMap = null;
        private SquareLookup<bool?> _compareMap = null;
        #endregion

        #region Ctor
        public ArithmeticMatrix(NumeralSet numeralSet)
        {
            if (numeralSet == null)
                throw new ArgumentNullException("numeralSet");
            this._numeralSet = numeralSet;

            var keys = this._numeralSet.SymbolSet.Values;
            this._addMap = new SquareLookup<Tuple<string, bool>>(keys);
            this._subtractMap = new SquareLookup<Tuple<string, bool>>(keys);
            this._compareMap = new SquareLookup<bool?>(keys);

            foreach (var key1 in keys)
            {
                foreach (var key2 in keys)
                {
                    var dig1 = numeralSet.GetSymbolicDigit(key1);
                    var rollover = dig1.Add(key2);
                    this._addMap.Add(key1, key2, new Tuple<string, bool>(dig1.Symbol, rollover));

                    dig1 = numeralSet.GetSymbolicDigit(key1);
                    rollover = dig1.Subtract(key2);
                    this._subtractMap.Add(key1, key2, new Tuple<string, bool>(dig1.Symbol, rollover));

                    dig1 = numeralSet.GetSymbolicDigit(key1);
                    var compare = dig1.Compare(key2);
                    this._compareMap.Add(key1, key2, compare);
                }
            }

        }
        #endregion

        #region Methods
        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        /// <param name="digit"></param>
        /// <returns></returns>
        public bool? Compare(string symbol1, string symbol2)
        {
            return this._compareMap.Get(symbol1, symbol2);
        }
        public bool Add(string symbol1, string symbol2, out string symbol)
        {
            var item = this._addMap.Get(symbol1, symbol2);
            symbol = item.Item1;
            return item.Item2;
        }
        public bool Subtract(string symbol1, string symbol2, out string symbol)
        {
            var item = this._subtractMap.Get(symbol1, symbol2);
            symbol = item.Item1;
            return item.Item2;
        }
        public bool AddOne(string symbol1, out string symbol)
        {
            return this.Add(symbol1, this._numeralSet.OneSymbol, out symbol);
        }
        public bool SubtractOne(string symbol1, out string symbol)
        {
            return this.Subtract(symbol1, this._numeralSet.OneSymbol, out symbol);
        }
        #endregion
    }
}
