using System;

using System.Linq;
using System.Text;
using System.Diagnostics;
using Arith.DataStructures;
using Arith.DataStructures.Decorations;
using Arith.Decorating;

namespace Arith.Domain.Digits
{
    /// <summary>
    /// contains an set of numerals.  Eg. a decimal numeral set would have 0-9.
    /// a binary numeral set would have 0 to 1.
    /// </summary>
    [DebuggerDisplay("{Text}")]
    public class NumeralSet
    {
        #region Declarations
        private CircularLinkedList<string> _symbols = new CircularLinkedList<string>();
        private string _negativeSymbol = null;
        private string _decimalSymbol = null;
        #endregion

        #region Ctor
        public NumeralSet(string decimalSymbol, string negativeSymbol, params string[] symbols)
        {
            if (string.IsNullOrEmpty(negativeSymbol))
                throw new ArgumentNullException("negativeSymbol");
            _negativeSymbol = negativeSymbol;

            if (string.IsNullOrEmpty(decimalSymbol))
                throw new ArgumentNullException("decimalSymbol");
            _decimalSymbol = decimalSymbol;

            if (symbols != null)
                foreach (string each in symbols)
                    this.AddSymbolToSet(each);

            this.Matrix = new ArithmeticMatrix(this);
        }
        #endregion

        #region Properties
        public CircularLinkedList<string> SymbolSet { get { return this._symbols; } }
        public string NegativeSymbol { get { return this._negativeSymbol; } }
        public string DecimalSymbol { get { return this._decimalSymbol; } }
        public ArithmeticMatrix Matrix { get; private set; }
        #endregion

        #region Calculated Properties
        public string[] Symbols
        {
            get
            {
                return this.SymbolSet.As<LinkedList<string>>().Values;
            }
        }
        public string Text
        {
            get
            {
                return string.Join(",", this.SymbolSet.As<LinkedList<string>>().Values);
            }
        }
        public string ZeroSymbol
        {
            get
            {
                if (this.SymbolSet.FirstNode == null)
                    return null;

                return this.SymbolSet.FirstNode.NodeValue;
            }
        }
        public string OneSymbol
        {
            get
            {
                if (this.SymbolSet.FirstNode == null || this.SymbolSet.FirstNode.NextNode == null)
                    return null;

                return this.SymbolSet.FirstNode.NextNode.NodeValue;
            }
        }
        #endregion

        #region Methods
        public bool IsCompatible(NumeralSet set)
        {
            if (set == null)
                return false;

            if (!this.DecimalSymbol.Equals(set.DecimalSymbol))
                return false;

            if (!this.NegativeSymbol.Equals(set.NegativeSymbol))
                return false;

            var thisNode = this.SymbolSet.FirstNode;
            var setNode = set.SymbolSet.FirstNode;
            while (thisNode != null && setNode != null)
            {
                if (!thisNode.NodeValue.Equals(setNode.NodeValue))
                    return false;

                thisNode = thisNode.NextNode;
                setNode = setNode.NextNode;

                if (thisNode != null && setNode == null)
                    return false;

                if (thisNode == null && setNode != null)
                    return false;
            }

            return true;
        }
        public SymbolicDigit GetSymbolicDigit(string symbol)
        {
            var node = this.SymbolSet.Filter((i) => { return i.NodeValue.Equals(symbol); }, true) as ICircularLinkedListNode<string>;
            return new SymbolicDigit(node);
        }
        public MatrixDigit GetMatrixDigit(string symbol)
        {
            return new MatrixDigit(symbol, this);
        }
        public ICircularLinkedListNode<string> GetComplement(ICircularLinkedListNode<string> symbol)
        {
            return symbol.GetListComplement();
        }
        public NumeralSet AddSymbolToSet(string symbol)
        {
            this.ValidateNewSymbol(symbol);
            this._symbols.AddLast(symbol);

            //rebuild the matrix
            this.Matrix = new ArithmeticMatrix(this);

            //fluent return
            return this;
        }
        protected void ValidateNewSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentOutOfRangeException("symbol");

            //validate negative and decimal reservations
            if (symbol.Contains(this._negativeSymbol) ||
            this._negativeSymbol.Contains(symbol))
            {
                throw new InvalidOperationException("symbol taken");
            }

            if (symbol.Contains(this._decimalSymbol) ||
                this._decimalSymbol.Contains(symbol))
            {
                throw new InvalidOperationException("symbol taken");
            }
            //validate other reservations
            var match = this._symbols.Filter((x) =>
            {
                if (symbol.Contains(x.NodeValue) ||
                x.NodeValue.Contains(symbol))
                {
                    return true;
                }

                return false;
            }, true);

            if(match != null)
            {
                throw new InvalidOperationException("symbol taken");
            }
        }
        #endregion

        #region Parsing
        /// <summary>
        /// given some text, identifies each unique symbol in that text and returns as an array.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string[] ParseSymbols(string text, bool parseLeftToRight = false, bool skipNonSymbols = true)
        {
            if (text == null)
                return null;

            string remaining = text;

            System.Collections.Generic.List<string> rv = new System.Collections.Generic.List<string>();

            while (!string.IsNullOrEmpty(remaining))
            {
                string symbol = null;
                var success = this.ParseNextSymbol(remaining, out remaining, out symbol, parseLeftToRight);
                if (!success && skipNonSymbols)
                    continue;
                rv.Add(symbol);
            }

            return rv.ToArray();
        }
        private bool ParseNextSymbol(string text, out string newText, out string symbol, bool seekLeftToRight = false)
        {
            string newTextOUT = text;
            string symbolOUT = null;
            bool isSuccess = false;

            var values = this.SymbolSet.As<LinkedList<string>>().Values.ToList();
            values.Add(this.DecimalSymbol);
            values.Add(this.NegativeSymbol);

            foreach (var each in values)
            {
                if (seekLeftToRight)
                {
                    if (text.StartsWith(each))
                    {
                        symbolOUT = each;
                        newTextOUT = text.Substring(symbolOUT.Length);
                        isSuccess = true;
                        break;
                    }
                }
                else
                {
                    if (text.EndsWith(each))
                    {
                        symbolOUT = each;
                        newTextOUT = text.Substring(0, text.Length - symbolOUT.Length);
                        isSuccess = true;
                        break;
                    }
                }
            }

            //if we can't find a symbol we truncate by 1 character
            if (!isSuccess)
            {
                if (seekLeftToRight)
                {
                    newTextOUT = text.Substring(1);
                    symbolOUT = text.Substring(0, 1);
                }
                else
                {
                    newTextOUT = text.Substring(0, text.Length - 1);
                    symbolOUT = text.Substring(text.Length - 1, 1);
                }
            }
            newText = newTextOUT;
            symbol = symbolOUT;
            return isSuccess;
        }
        #endregion
    }


    public class NumeralSetTests
    {
        public static void Test()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }

            Debug.Assert(set.DecimalSymbol == ".");
            Debug.Assert(set.NegativeSymbol == "-");
            Debug.Assert(set.ZeroSymbol == "0");
            Debug.Assert(set.OneSymbol == "1");

            var parse1 = set.ParseSymbols("1234567890", true, true);
            Debug.Assert(parse1.Length == 10);
            Debug.Assert(parse1[0] == "1");
            Debug.Assert(parse1[1] == "2");
            Debug.Assert(parse1[2] == "3");
            Debug.Assert(parse1[3] == "4");
            Debug.Assert(parse1[4] == "5");
            Debug.Assert(parse1[5] == "6");
            Debug.Assert(parse1[6] == "7");
            Debug.Assert(parse1[7] == "8");
            Debug.Assert(parse1[8] == "9");
            Debug.Assert(parse1[9] == "0");

            var parse2 = set.ParseSymbols("-1234567890.123", true, true);
            Debug.Assert(parse2[0] == "-");
            Debug.Assert(parse2[1] == "1");
            Debug.Assert(parse2[2] == "2");
            Debug.Assert(parse2[3] == "3");
            Debug.Assert(parse2[4] == "4");
            Debug.Assert(parse2[5] == "5");
            Debug.Assert(parse2[6] == "6");
            Debug.Assert(parse2[7] == "7");
            Debug.Assert(parse2[8] == "8");
            Debug.Assert(parse2[9] == "9");
            Debug.Assert(parse2[10] == "0");
            Debug.Assert(parse2[11] == ".");
            Debug.Assert(parse2[12] == "1");
            Debug.Assert(parse2[13] == "2");
            Debug.Assert(parse2[14] == "3");


            var parse3 = set.ParseSymbols("-1234567890.123", false, true);
            Debug.Assert(parse3[14] == "-");
            Debug.Assert(parse3[13] == "1");
            Debug.Assert(parse3[12] == "2");
            Debug.Assert(parse3[11] == "3");
            Debug.Assert(parse3[10] == "4");
            Debug.Assert(parse3[9] == "5");
            Debug.Assert(parse3[8] == "6");
            Debug.Assert(parse3[7] == "7");
            Debug.Assert(parse3[6] == "8");
            Debug.Assert(parse3[5] == "9");
            Debug.Assert(parse3[4] == "0");
            Debug.Assert(parse3[3] == ".");
            Debug.Assert(parse3[2] == "1");
            Debug.Assert(parse3[1] == "2");
            Debug.Assert(parse3[0] == "3");

            try
            {
                var parse4 = set.ParseSymbols("x-1234567890.123", false, false);
            }
            catch { }

            var digit = set.GetSymbolicDigit("0");
            Debug.Assert(digit.Symbol.Equals(set.ZeroSymbol));
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
            s = digit.Subtract("1");
            Debug.Assert(digit.Symbol == "9");
            Debug.Assert(s);

        }
    }
}
