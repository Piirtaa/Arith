using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Arith
{
    public class Number_old
    {
        #region Declarations
        private NumeralSet[] _registers = null;
        private bool _isPositive = true;
        #endregion

        #region Ctor
        public Number_old(NumeralSet set) : this(set.ZeroSymbol, set)
        {
        }

        public Number_old(string numberText, NumeralSet set)
        {
            if (set == null)
                throw new ArgumentNullException("symbol set");
            this.SymbolSet = set;
            this.SetNumber(numberText);
        }
        #endregion

        #region Properties
        public NumeralSet SymbolSet { get; private set; }
        public string Text
        {
            get
            {
                List<string> registerList = new List<string>();
                if (this._registers != null)
                    foreach (var each in _registers)
                        registerList.Add(each.CurrentSymbol);

                //now remove any most significant zeros on the end
                int mostSigZeroCount = 0;
                
                for (int i = registerList.Count - 1; i >= 1; i--)
                {
                    if (registerList[i] == this.SymbolSet.ZeroSymbol)
                    {
                        mostSigZeroCount++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (mostSigZeroCount > 0)
                    registerList.RemoveRange(registerList.Count - mostSigZeroCount, mostSigZeroCount);

                //we encode the number right to left, right being the least significant digits
                registerList.Reverse();

                if (this._isPositive)
                {
                    return string.Join("", registerList);
                }
                else
                {
                    return this.SymbolSet.NegativeSymbol + string.Join("", registerList);
                }
                
            }
        }
        public int Length { get { return this._registers.Length; } }
        public bool IsPositive { get { return this._isPositive; } }
        #endregion

        #region Methods
        private void SetNumber(string numberText)
        {
            string[] symbols = null;

            if (numberText.StartsWith(this.SymbolSet.NegativeSymbol))
            {
                this._isPositive = false;

                this.SymbolSet.ParseSymbols(numberText.Substring(this.SymbolSet.NegativeSymbol.Length));
            }
            else
            {
                this.SymbolSet.ParseSymbols(numberText);
            }

            if (symbols != null)
            {
                //init the array of registers and set them position by position
                this._registers = new NumeralSet[symbols.Length];
                for (int i = 0; i < symbols.Length; i++)
                {
                    this._registers[i] = this.SymbolSet.Clone();
                    this._registers[i].MoveAheadBy(symbols[i]);
                }
            }
        }
        private void ExpandRegistersTo(int capacity)
        {
            var oldLength = this.Length;
            if (capacity <= oldLength)
                return;

            Array.Resize(ref this._registers, capacity);
            for (int i = oldLength; i < this._registers.Length; i++)
            {
                //create a blank symbol in the new registers
                if (this._registers[i] == null)
                    this._registers[i] = this.SymbolSet.Clone();
            }
        }
        #endregion

        #region Add
        public Number_old Add(params string[] numbers)
        {
            if (numbers == null)
                return this;

            List<Number_old> items = new List<Number_old>();

            foreach (var each in numbers)
            {
                var num = new Number_old(each, this.SymbolSet);
                items.Add(num);
            }

            return this.Add(items.ToArray());
        }

        //public Number Add(params Number[] numbers)
        //{
        //    if (numbers == null)
        //        return this;

        //    Debug.WriteLine(string.Format("adding {0}", this.Text));
        //    foreach (var each in numbers)
        //        Debug.WriteLine(string.Format("to {0}", each.Text));

        //    //find longest number and iterate to that
        //    int maxLength = this.Length;

        //    foreach (var each in numbers)
        //        if (each.Length > maxLength)
        //            maxLength = each.Length;

        //    //add each number register by register
        //    for (int i = 0; i < maxLength; i++)
        //    {
        //        foreach (var each in numbers)
        //        {
        //            if (each == null)
        //                continue;

        //            if (each.Length < i)
        //                continue;

        //            this.AddToSingleRegister(this, each._registers[i].CurrentSymbol, i);
        //        }
        //    }
        //    return this;
        //}
        public Number_old Add(params Number_old[] numbers)
        {
            if (numbers == null)
                return this;

            Debug.Write(string.Format("{0} adding ", this.Text));
            foreach (var each in numbers)
                Debug.WriteLine(string.Format(" {0} ", each.Text));

            foreach (var each in numbers)
                this.AddSingle(each);

            return this;
        }
        public Number_old AddSingle(Number_old number)
        {
            if (number == null)
                return this;

            if (!number.IsPositive)
            {
                number._isPositive = true;
                return this.SubtractSingle(number);
            }

            Debug.WriteLine(string.Format("adding {0} to {1}",number.Text, this.Text));

            //find longest number and iterate to that
            int maxLength = number.Length;

            //add each number register by register
            for (int i = 0; i < maxLength; i++)
            {
                this.AddToSingleRegister(this, number._registers[i].CurrentSymbol, i);
            }
            return this;
        }
        private void AddToSingleRegister(Number_old number, string symbol, int pos)
        {
            if (number == null)
                return;

            if (symbol == null)
                return;

            //get some debug info to log
            var origValue = number.Text;

            number.ExpandRegistersTo(pos + 2);

            var origSymbol = number._registers[pos].CurrentSymbol;
            string origNextSymbol = number._registers[pos + 1].CurrentSymbol;

            //move ahead
            var rollover = number._registers[pos].MoveAheadBy(symbol);

            Debug.WriteLine("add. orig string {0}, pos {1}, orig symbol {2}, add symbol {3}, new symbol {4}", origValue, pos, origSymbol, symbol, number._registers[pos].CurrentSymbol);
            if (rollover)
            {
                Debug.WriteLine("carrying 1 to pos {0}, orig string {1}, orig symbol {2}", pos + 1, origValue, origNextSymbol);

                //recurse
                AddToSingleRegister(number, this.SymbolSet.OneSymbol, pos + 1);
            }
            Debug.WriteLine("add. orig string {0}, pos {1}, orig symbol {2}, add symbol {3}, new symbol {4}, new string {5}", origValue, pos, origSymbol, symbol, number._registers[pos].CurrentSymbol, number.Text);

        }
        #endregion

        #region Subtract
        public Number_old Subtract(params string[] numbers)
        {
            if (numbers == null)
                return this;

            List<Number_old> items = new List<Number_old>();

            foreach (var each in numbers)
            {
                var num = new Number_old(each, this.SymbolSet);
                items.Add(num);
            }

            return this.Add(items.ToArray());
        }

        public Number_old Subtract(params Number_old[] numbers)
        {
            if (numbers == null)
                return this;

            Debug.Write(string.Format("{0} subtracting ", this.Text));
            foreach (var each in numbers)
                Debug.WriteLine(string.Format(" {0} ", each.Text));

            foreach (var each in numbers)
                this.AddSingle(each);

            return this;
        }
        public Number_old SubtractSingle(Number_old number)
        {
            if (number == null)
                return this;

            if (!number.IsPositive)
            {
                number._isPositive = true;
                return this.AddSingle(number);
            }

            Debug.WriteLine(string.Format("subtracting {0} from {1}", number.Text, this.Text));


            //find longest number and iterate to that
            int maxLength = number.Length;

            //add each number register by register
            for (int i = 0; i < maxLength; i++)
            {
                this.SubtractFromSingleRegister(this, number._registers[i].CurrentSymbol, i);
            }
            return this;
        }
        private void SubtractFromSingleRegister(Number_old number, string symbol, int pos)
        {
            if (number == null)
                return;

            if (symbol == null)
                return;

            //get some debug info to log
            var origValue = number.Text;

            var origSymbol = number._registers[pos].CurrentSymbol;
            string origNextSymbol = number._registers[pos + 1].CurrentSymbol;

            //move back
            var rollover = number._registers[pos].MoveBehindBy(symbol);

            Debug.WriteLine("subtract. orig string {0}, pos {1}, orig symbol {2}, subtract symbol {3}, new symbol {4}", origValue, pos, origSymbol, symbol, number._registers[pos].CurrentSymbol);
            if (rollover)
            {
                Debug.WriteLine("carrying -1 to pos {0}, orig string {1}, orig symbol {2}", pos + 1, origValue, origNextSymbol);

                //sign change check
                //if the position is 0 and the 1st position has a zero symbol, we're rolling over to negative

                //recurse
                SubtractFromSingleRegister(number, this.SymbolSet.OneSymbol, pos + 1);
            }
            Debug.WriteLine("subtract. orig string {0}, pos {1}, orig symbol {2}, subtract symbol {3}, new symbol {4}, new string {5}", origValue, pos, origSymbol, symbol, number._registers[pos].CurrentSymbol, number.Text);

        }
        #endregion
    }
}
