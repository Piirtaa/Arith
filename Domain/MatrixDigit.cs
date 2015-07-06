using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace Arith.Domain
{
    /// <summary>
    /// Implements IDigit using an arithmetic matrix generated from SymbolicDigits.
    /// SymbolicDigits are slow as they iterate, whereas this class uses lookup tables
    /// </summary>
    [DebuggerDisplay("{Symbol}")]
    public class MatrixDigit : IDigit
    {
        #region Declarations
        private string _symbol = null;
        private ArithmeticMatrix _matrix = null;
        #endregion

        #region Ctor
        public MatrixDigit(string symbol, NumeralSet numberSystem)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentNullException("symbol");
            this._symbol = symbol;
            this._matrix = numberSystem.Matrix;
        }
        #endregion

        #region IDigit
        /// <summary>
        /// the current numeral symbol
        /// </summary>
        public string Symbol { get { return this._symbol; } }
        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        /// <param name="digit"></param>
        /// <returns></returns>
        public bool? Compare(string symbol)
        {
            return this._matrix.Compare(this._symbol, symbol);
        }
        public bool Add(string symbol)
        {
            return this._matrix.Add(this._symbol, symbol, out this._symbol);
        }
        public bool Subtract(string symbol)
        {
            return this._matrix.Subtract(this._symbol, symbol, out this._symbol);
        }
        public bool AddOne()
        {
            return this._matrix.AddOne(this._symbol, out this._symbol);
        }
        public bool SubtractOne()
        {
            return this._matrix.SubtractOne(this._symbol, out this._symbol);
        }
        public void SetValue(string numeral)
        {
            this._symbol = numeral;
        }
        #endregion
    }



}
