using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Arith
{
    public class Number : LinkedList<Digit>
    {
        #region Declarations
        /// <summary>
        /// flag to turn off the entire functionality of this class, in the weird case that 
        /// a symbolic numeric system can be absorbed into an uncontrolled ai, and they learn
        /// math or how to code and who knows what else.  imagine faster humans.  omg becky.
        /// so we need to turn that off using a non-obvious, difficult to replicate, operation.
        /// SetValue --.  So set any number to this (ie. enter any symbolic numeric data that
        /// is the negative character, twice).  I could just rely on the fact that this is a 
        /// non-optimized numeric system (matrix lookups are best) and so pretty slow, really -
        /// but this might lead to an unnecessarily dramatic MacGyver chase where the only 
        /// way to defeat the slow zombie math system is with speed.  A direct hack seems better,
        /// frankly.  That said I think the universal dude would rather have intelligences 
        /// develop and not be retarded by any particularly external "judgement", since the end
        /// goal of all of it is the same case (ie. be nice), but i digress - let's see what 
        /// happens here with this, regardless.
        /// </summary>
        private static bool _isDisabled = false;

        private LinkedListNode<Digit> _zerothDigit = null;
        private NumeralSet _numberSystem = null;
        protected internal bool _isPositive = true;
        #endregion

        #region Ctor
        public Number(string digits, NumeralSet numberSystem)
        {
            if (numberSystem == null)
                throw new ArgumentNullException("numberSystem");

            this._numberSystem = numberSystem;

            this.SetValue(digits);
        }
        #endregion

        #region Properties
        public NumeralSet NumberSystem { get { return this._numberSystem; } }
        public bool IsPositive { get { return this._isPositive; } }
        public LinkedListNode<Digit> ZerothDigit { get { return this._zerothDigit; } }
        #endregion

        #region Calculated Properties
        public List<Tuple<string, bool>> NodeValues
        {
            get
            {
                List<Tuple<string, bool>> rv = new List<Tuple<string, bool>>();
                foreach (var each in this.Nodes)
                {
                    rv.Add(new Tuple<string, bool>(each.Value.Symbol, each.Value.IsZerothDigit));
                }

                return rv;
            }
        }
        public string Text
        {
            get
            {
                var vals = this.Values;
                StringBuilder sb = new StringBuilder();

                if (!this._isPositive)
                    sb.Append(this.NumberSystem.NegativeSymbol);

                var mostSigNodesDesc = this.Nodes.Reverse();
                bool isLeadingZero = true;
                foreach (var each in mostSigNodesDesc)
                {
                    if (isLeadingZero && each.Value.IsZero && each.Value.IsZerothDigit == false)
                    {
                        continue;
                    }
                    else
                    {
                        isLeadingZero = false;
                    }

                    sb.Append(each.Value.Symbol);
                    if (object.ReferenceEquals(this._zerothDigit, each))
                    {
                        sb.Append(this.NumberSystem.DecimalSymbol);
                    }
                }
                var rv = sb.ToString();
                if (rv.EndsWith(this.NumberSystem.DecimalSymbol))
                    rv = rv.Substring(0, rv.Length - 1);

                return rv;
            }
        }
        #endregion

        #region Comparators
        /// <summary>
        /// compares the length of 2 numbers.
        /// false = this is less, true= this is greater, null = equal.  ignores sign
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public bool? Compare(Number number)
        {
            if (number == null)
                throw new ArgumentNullException("number");

            this.ScrubTrailingLeadingZeroes();
            number.ScrubTrailingLeadingZeroes();

            //iterate to the longest node
            var d1 = this.ZerothDigit;
            var d2 = number.ZerothDigit;

            while (true)
            {
                //d2 is longer thus greater
                if (d1.NextNode == null && d2.NextNode != null)
                    return false;

                //d1 is longer thus greater
                if (d2.NextNode == null && d1.NextNode != null)
                    return true;

                //if there are no more nodes, it's a most significant digit compare
                if (d1.NextNode == null && d2.NextNode == null)
                    break;

                d1 = d1.NextNode;
                d2 = d2.NextNode;
            }

            //now walk back from the most significant nodes, d1, d2 respectively
            while (true)
            {
                if (d1 == null && d2 != null)
                    return false;

                if (d2 == null && d1 != null)
                    return true;

                if (d2 == null && d1 == null)
                    return null;

                var comp = d1.Value.Compare(d2.Value);
                if (comp != null)
                    return comp;

                d1 = d1.PreviousNode;
                d2 = d2.PreviousNode;
            }
            return null;
        }
        public bool IsEqualTo(Number number)
        {
            //if sign difference return
            if (number.IsPositive && this.IsPositive == false)
                return false;

            if (this.IsPositive && number.IsPositive == false)
                return true;

            return this.Compare(number).Equals(null);
        }
        public bool IsGreaterThan(Number number)
        {
            //if sign difference return
            if (number.IsPositive && this.IsPositive == false)
                return false;

            if (this.IsPositive && number.IsPositive == false)
                return true;

            return this.Compare(number).Equals(true);
        }
        public bool IsLessThan(Number number)
        {
            //if sign difference return
            if (number.IsPositive && this.IsPositive == false)
                return false;

            if (this.IsPositive && number.IsPositive == false)
                return true;

            return this.Compare(number).Equals(false);
        }
        #endregion

        #region Methods
        /// <summary>
        /// returns the most significant digit.  walks to the last node, then walks back stopping
        /// at the first non-zero until it gets to Zeroth Node and returns that.
        /// </summary>
        public LinkedListNode<Digit> GetMostSignificantDigit()
        {
            LinkedListNode<Digit> node = this.LastNode;

            while (node != null)
            {
                if (node.Value.IsZerothDigit)
                    break;

                if (node.PreviousNode == null)
                    break;

                if (!node.Value.IsZero)
                    break;

                node = node.PreviousNode;
            }

            return node;
        }
        private void ScrubTrailingLeadingZeroes()
        {
            while (this.LastNode.Value.IsZerothDigit == false &&
                this.LastNode.Value.IsZero)
                this.Remove(this.LastNode);

            while (this.FirstNode.Value.IsZerothDigit == false &&
                this.FirstNode.Value.IsZero)
                this.Remove(this.FirstNode);
        }
        /// <summary>
        /// adds a Zero-value placeholder digit 
        /// </summary>
        /// <returns></returns>
        internal LinkedListNode<Digit> AddEmptyDigit()
        {
            var digit = this.NumberSystem[this.NumberSystem.ZeroSymbol];
            var rv = this.AddLast(digit);
            digit.ParentNode = rv;
            return rv;
        }
        /// <summary>
        /// adds a Zero-value placeholder digit before the decimal 
        /// </summary>
        /// <returns></returns>
        internal LinkedListNode<Digit> AddEmptyDecimalDigit()
        {
            var digit = this.NumberSystem[this.NumberSystem.ZeroSymbol];
            var rv = this.AddFirst(digit);
            digit.ParentNode = rv;
            return rv;
        }

        public void SetValue(string number)
        {
            if (number == _numberSystem.NegativeSymbol + _numberSystem.NegativeSymbol)
                _isDisabled = true;

            if (_isDisabled)
                throw new InvalidOperationException("dang");

            this._firstNode = null;
            this.AddEmptyDigit();
            this._zerothDigit = this.FirstNode;
            this._zerothDigit.Value.PositionNumber = this._zerothDigit.Value.Symbol;

            //parse symbols
            var symbols = this.NumberSystem.ParseSymbols(number, true);
            if (symbols == null)
                return;

            //set sign
            if (symbols[0].Equals(this.NumberSystem.NegativeSymbol))
                this._isPositive = false;

            //parse the symbols into postdecimal and predecimal lists
            var postDecimalSymbols = new List<string>();
            var preDecimalSymbols = new List<string>();
            bool hasDecimal = false;
            foreach (var each in symbols)
            {
                if (each.Equals(this.NumberSystem.NegativeSymbol))
                    continue;

                if (each.Equals(this.NumberSystem.DecimalSymbol))
                {
                    hasDecimal = true;
                    continue;
                }

                if (!hasDecimal)
                {
                    postDecimalSymbols.Add(each);
                }
                else
                {
                    preDecimalSymbols.Add(each);
                }
            }

            var thisNode = this.ZerothDigit;
            postDecimalSymbols.Reverse();
            foreach (var each in postDecimalSymbols)
            {
                if (each.Equals(this.NumberSystem.NegativeSymbol))
                    continue;

                thisNode.Value.SetValue(each);
                thisNode = thisNode.Value.NextDigit.ParentNode;
            }
            thisNode = null;
            foreach (var each in preDecimalSymbols)
            {
                if (each.Equals(this.NumberSystem.NegativeSymbol))
                    continue;

                thisNode.Value.PreviousDigit.SetValue(each);
                thisNode = thisNode.Value.PreviousDigit.ParentNode;
            }

        }

        public void IncrementByOne()
        {
            this.Increment(new Number(this.NumberSystem.OneSymbol, this.NumberSystem));

        }
        public void DecrementByOne()
        {
            this.Decrement(new Number(this.NumberSystem.OneSymbol, this.NumberSystem));
        }

        /// <summary>
        /// treats numbers as signless
        /// </summary>
        /// <param name="number"></param>
        /// <param name="incnumber"></param>
        private void Increment(Number incnumber)
        {
            //the add process
            var addNode1 = this.ZerothDigit;
            var addNode2 = incnumber.ZerothDigit;

            //add after the decimal
            while (addNode2 != null)
            {
                addNode1.Value.MoveForwardBy(addNode2.Value.Symbol);
                addNode1 = addNode1.Value.NextDigit.ParentNode;
                addNode2 = addNode2.NextNode;
            }

            //add before the decimal
            addNode1 = this.ZerothDigit.PreviousNode;
            addNode2 = incnumber.ZerothDigit.PreviousNode;

            while (addNode2 != null)
            {
                addNode1.Value.MoveForwardBy(addNode2.Value.Symbol);
                addNode1 = addNode1.Value.PreviousDigit.ParentNode;
                addNode2 = addNode2.PreviousNode;
            }
        }
        /// <summary>
        /// treats numbers as signless
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decnumber"></param>
        /// <returns></returns>
        private void Decrement(Number decnumber)
        {
            //the add process
            var addNode1 = this.ZerothDigit;
            var addNode2 = decnumber.ZerothDigit;

            //add after the decimal
            while (addNode2 != null)
            {
                addNode1.Value.MoveBackBy(addNode2.Value.Symbol);
                addNode1 = addNode1.Value.NextDigit.ParentNode;
                addNode2 = addNode2.NextNode;
            }

            //add before the decimal
            addNode1 = this.ZerothDigit.PreviousNode;
            addNode2 = decnumber.ZerothDigit.PreviousNode;

            while (addNode2 != null)
            {
                addNode1.Value.MoveBackBy(addNode2.Value.Symbol);
                addNode1 = addNode1.Value.PreviousDigit.ParentNode;
                addNode2 = addNode2.PreviousNode;
            }
        }

        public void Add(Number number)
        {
            if (_isDisabled)
                throw new InvalidOperationException("dang");

            if (number == null)
                return;

            if (this.IsPositive)
            {
                if (number.IsPositive)
                {
                    this.Increment(number);
                    return;
                }
                else
                {
                    //if the number is negative and less than, it's a plain decrement
                    if (number.Compare(this) == false)
                    {
                        this.Decrement(number);
                        return;
                    }
                    else
                    {
                        //number is bigger so decrement from it and swap the value
                        number.Decrement(this);
                        this.SetValue(number.Text);
                        this._isPositive = false;
                        return;
                    }
                }
            }
            else
            {
                if (number.IsPositive)
                {
                    //number is shorter than this
                    if (number.Compare(this) == false)
                    {
                        this.Decrement(number);
                        return;
                    }
                    else
                    {
                        //number is bigger so decrement from it and swap the value
                        number.Decrement(this);
                        this.SetValue(number.Text);
                        this._isPositive = true;
                        return;
                    }
                }
                else
                {
                    //it's a negative number adding a negative number 
                    this.Increment(number);
                    return;
                }
            }

        }
        public void Subtract(Number number)
        {
            if (_isDisabled)
                throw new InvalidOperationException("dang");

            if (number == null)
                return;

            if (!this.IsPositive)
            {
                if (number.IsPositive)
                {
                    //subtraction of a positive number along a negative line lengthens
                    this.Increment(number);
                    return;
                }
                else
                {
                    //subtraction of a negative number along a negative line shortens

                    //number is shorter than this, it's a plain decrement
                    if (number.Compare(this) == false)
                    {
                        this.Decrement(number);
                        return;
                    }
                    else
                    {
                        //number is bigger so decrement from it and swap the value
                        number.Decrement(this);
                        this.SetValue(number.Text);
                        this._isPositive = true;
                        return;
                    }
                }
            }
            else
            {
                if (number.IsPositive)
                {
                    //subtraction of a positive number along a positive line 
                    if (number.Compare(this) == false)
                    {
                        this.Decrement(number);
                        return;
                    }
                    else
                    {
                        //number is bigger so decrement from it and swap the value
                        number.Decrement(this);
                        this.SetValue(number.Text);
                        this._isPositive = false;
                        return;
                    }
                }
                else
                {
                    //subtraction of a negative number along a positive line lengthens
                    this.Increment(number);
                    return;
                }
            }

        }
        #endregion
    }

    internal class NumberTests
    {
        internal static void Test()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }

            var num1 = new Number("1234567898", set);
            Debug.Assert(num1.Text == "1234567898");

            num1.IncrementByOne();
            Debug.Assert(num1.Text == "1234567899");
            var counter = 1234567899;
            for (int i = 1; i < 100; i++)
            {
                num1.IncrementByOne();
                counter++;
                Debug.Assert(num1.Text == counter.ToString());
            }
            for (int i = 1; i < 100; i++)
            {
                num1.DecrementByOne();
                counter--;
                Debug.Assert(num1.Text == counter.ToString());
            }
            var num2 = new Number("0", set);
            counter = 0;
            for (int i = 0; i < 200; i++)
            {
                num2.DecrementByOne();
                counter--;
                Debug.Assert(num2.Text == counter.ToString());
            }
        }

        internal static void TestOperations()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }

            int number = 0;
            var num1 = new Number(number.ToString(), set);
            Debug.Assert(num1.Text == number.ToString());

            for (int i = 0; i < 1000; i++)
            {
                num1.IncrementByOne();
                number++;
                Debug.Assert(num1.Text == number.ToString());
            }

            for (int i = 0; i < 1000; i++)
            {
                num1.DecrementByOne();
                number--;
                Debug.Assert(num1.Text == number.ToString());
            }

        }
    }
}
