using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Arith
{

    /// <summary>
    /// implements IDigit using a circular symbol list
    /// </summary>
    [DebuggerDisplay("{Symbol}")]
    public class SymbolicDigit : IDigit
    {
        #region Declarations

        private CircularLinkedListNode<string> _numeral = null;

        /// <summary>
        /// the symbol that when "added" to the current symbol produces the last symbol in the set
        /// </summary>
        private CircularLinkedListNode<string> _complementNumeral = null;
        #endregion

        #region Ctor
        public SymbolicDigit(CircularLinkedListNode<string> numeral)
        {
            if (numeral == null)
                throw new ArgumentNullException("numeral");
            this._numeral = numeral;

            this._complementNumeral = numeral.GetListComplement();
        }
        #endregion

        #region Hidden Properties
        /// <summary>
        /// for the numeral set this digit belongs to, returns the zeroth(first) numeral 
        /// </summary>
        protected CircularLinkedListNode<string> ZeroNumeral { get { return this._numeral.ParentList.FirstNode as CircularLinkedListNode<string>; } }
        #endregion

        #region IDigit
        /// <summary>
        /// the current numeral symbol
        /// </summary>
        public string Symbol { get { return this._numeral.Value; } }
        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        /// <param name="digit"></param>
        /// <returns></returns>
        public bool? Compare(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentNullException("symbol");

            if (symbol.Equals(this.Symbol))
                return null;

            var numeral = this.ZeroNumeral;

            //find which digit comes first thru simple iteration
            while (numeral != null)
            {
                if (numeral.Value.Equals(this.Symbol))
                    return true;

                if (numeral.Value.Equals(symbol))
                    return false;

                if (numeral.IsLast)
                    throw new InvalidOperationException("bad digit compare");

                numeral = numeral.NextNode as CircularLinkedListNode<string>;
            }
            return null;
        }
        public bool Add(string symbol)
        {
            var rv = this._numeral.MoveForwardBy(symbol, out _numeral);
            //if (rv && this.NextDigit != null)
            //    this.NextDigit.MoveForward();
            return rv;
        }
        public bool Subtract(string symbol)
        {
            var rv = this._numeral.MoveBackBy(symbol, out _numeral);
            //if (rv && this.NextDigit != null)
            //    this.NextDigit.MoveBack();
            return rv;
        }
        public bool AddOne()
        {
            var rv = this._numeral.MoveForward(out _numeral);
            //if (rv && this.NextDigit != null)
            //{
            //    this.NextDigit.MoveForward();
            //}
            return rv;
        }
        public bool SubtractOne()
        {
            var rv = this._numeral.MoveBack(out _numeral);
            //if (rv && this.NextDigit != null)
            //{
            //    this.NextDigit.MoveBack();
            //}
            return rv;
        }
        public void SetValue(string numeral)
        {
            this._numeral = this._numeral.FindNodeByValue(numeral);
        }
        #endregion
    }


    internal class SymbolicDigitTests
    {
        internal static void Test()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }


            var digit = set.GetSymbolicDigit("0");
            Debug.Assert(digit.Symbol == "0");
            var s = digit.AddOne();
            Debug.Assert(!s);
            Debug.Assert(digit.Symbol == "1");
            digit.AddOne();
            Debug.Assert(digit.Symbol == "2");
            s = digit.Add("9");
            Debug.Assert(digit.Symbol == "1");
            Debug.Assert(s);
            digit.Subtract("1");
            Debug.Assert(digit.Symbol == "0");
            s = digit.SubtractOne();
            Debug.Assert(digit.Symbol == "9");
            Debug.Assert(s);

        }
    }
}
