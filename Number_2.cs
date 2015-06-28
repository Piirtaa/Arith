using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arith
{
    /// <summary>
    /// describes an instance of a symbol at a position in a number
    /// </summary>
    public class Number 
    {
        #region Declarations
        private Number_old _parentNumber = null;
        private CircularLinkedListNode<string> _currentSymbol = null;
        private CircularLinkedListNode<string> _position = 
        #endregion

        #region Ctor
        public Number(Number_old parentNumber, string symbol)
        {
            if (parentNumber == null)
                throw new ArgumentNullException("number");

            this._parentNumber = parentNumber;
            
            if (!this._parentNumber.SymbolSet.Contains(symbol))
                throw new ArgumentOutOfRangeException("invalid symbol");

            this._currentSymbol = this._parentNumber.SymbolSet[symbol];
        }
        #endregion

        #region Properties
        public string CurrentSymbol
        {
            get
            {
                return this._currentSymbol.Value;
            }
        }
        #endregion

        #region Methods
        public void Reset()
        {
            this._currentSymbol = this._currentSymbol.FirstNode;
        }
        /// <summary>
        /// moves to the next symbol.  if at the end of the line, returns true and resets index to 0
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            return this._currentSymbol.MoveForward(out this._currentSymbol);
        }
        /// <summary>
        /// moves to the last symbol.  if at 0, returns true and resets index to the end of the line
        /// </summary>
        /// <returns></returns>
        public bool MoveBack()
        {
            return this._currentSymbol.MoveBack(out this._currentSymbol);
        }
        public bool MoveAheadBy(string symbol)
        {
            return this._currentSymbol.MoveForward(out this._currentSymbol);
        }
        public bool MoveBehindBy(string symbol)
        {
            return this._currentSymbol.MoveForward(out this._currentSymbol);
        }
        #endregion
    }
}
