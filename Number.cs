using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Arith
{
    [DebuggerDisplay("{Text}")]
    public class Number : LinkedList<NumberDigit>
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

        private LinkedListNode<NumberDigit> _zerothDigit = null;
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
        public LinkedListNode<NumberDigit> ZerothDigit { get { return this._zerothDigit; } }
        #endregion

        #region Calculated Properties
        public List<Tuple<string, bool>> NodeValues
        {
            get
            {
                List<Tuple<string, bool>> rv = new List<Tuple<string, bool>>();
                foreach (var each in this.Nodes)
                {
                    rv.Add(new Tuple<string, bool>(each.Value.Digit.Symbol, each.Value.IsZerothDigit));
                }

                return rv;
            }
        }
        /// <summary>
        /// by default this represents the leading zero, trailing zero trimmed value 
        /// </summary>
        public virtual string Text
        {
            get
            {
                var vals = this.Values;
                StringBuilder sb = new StringBuilder();

                this.ScrubLeadingAndTrailingZeroes();

                if (!this._isPositive)
                    sb.Append(this.NumberSystem.NegativeSymbol);

                var mostSigNodesDesc = this.Nodes.Reverse();
                foreach (var each in mostSigNodesDesc)
                {
                    sb.Append(each.Value.Digit.Symbol);
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

            this.ScrubLeadingAndTrailingZeroes();
            number.ScrubLeadingAndTrailingZeroes();

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

                var comp = d1.Value.Digit.Compare(d2.Value.Digit.Symbol);
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

        #region Helpers
        private void SwitchSign()
        {
            this._isPositive = !this._isPositive;
        }
        private void ScrubLeadingAndTrailingZeroes()
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
        internal LinkedListNode<NumberDigit> AddEmptyDigit()
        {
            var digit = new NumberDigit(this.NumberSystem.ZeroSymbol, this.NumberSystem);
            var rv = this.AddLast(digit);
            digit.DigitNode = rv;
            return rv;
        }
        /// <summary>
        /// adds a Zero-value placeholder digit before the decimal 
        /// </summary>
        /// <returns></returns>
        internal LinkedListNode<NumberDigit> AddEmptyDecimalDigit()
        {
            var digit = new NumberDigit(this.NumberSystem.ZeroSymbol, this.NumberSystem);
            var rv = this.AddFirst(digit);
            digit.DigitNode = rv;
            return rv;
        }

        #endregion

        #region adding logic
        public static void HydrateFrom(Number numberToHydrate, Number number)
        {
            numberToHydrate._isPositive = number._isPositive;

            numberToHydrate._firstNode = null;
            numberToHydrate.AddEmptyDigit();
            numberToHydrate._zerothDigit = numberToHydrate.FirstNode;

            var currentDigit = numberToHydrate.FirstNode.Value;
            var node = number.ZerothDigit;
            while (node != null)
            {
                currentDigit.Digit.SetValue(node.Value.Digit.Symbol);
                currentDigit = currentDigit.NextDigit;
                node = node.NextNode;
            }

            currentDigit = numberToHydrate.FirstNode.Value;
            node = number.ZerothDigit.PreviousNode;
            while (node != null)
            {
                currentDigit = currentDigit.PreviousDigit;
                currentDigit.Digit.SetValue(node.Value.Digit.Symbol);
                node = node.PreviousNode;
            }
        }
        public static Number Clone(Number number)
        {
            var rv = new Number(null, number.NumberSystem);
            HydrateFrom(rv, number);
            return rv;
        }
        private static Number Add(Number number1, Number number2)
        {
            if (_isDisabled)
                throw new InvalidOperationException("dang");

            if (number1 == null)
                return number2;

            if (number2 == null)
                return number1;

            Number rv = null;

            //determine the longer number
            bool? num1IsLonger = number1.Compare(number2);

            if (number1.IsPositive && number2.IsPositive)
            {
                rv = Increment(number1, number2);
            }
            else if (number1.IsPositive && number2.IsPositive == false)
            {
                switch (num1IsLonger)
                {
                    case null:
                        rv = Decrement(number1, number2);
                        break;
                    case true:
                        rv = Decrement(number1, number2);
                        break;
                    case false:
                        rv = Decrement(number2, number1);
                        rv._isPositive = false;
                        break;
                }
            }
            else if (number1.IsPositive == false && number2.IsPositive)
            {
                switch (num1IsLonger)
                {
                    case null:
                        rv = Decrement(number1, number2);
                        break;
                    case true:
                        rv = Decrement(number1, number2);
                        break;
                    case false:
                        rv = Decrement(number2, number1);
                        rv._isPositive = true;
                        break;
                }
            }
            else if (number1.IsPositive == false && number2.IsPositive == false)
            {
                rv = Increment(number1, number2);
                rv._isPositive = false;
            }

            return rv;
        }
        /// <summary>
        /// treats numbers as signless, and increase the value of the host number by the amount
        /// </summary>
        /// <param name="number"></param>
        /// <param name="number2"></param>
        private static Number Increment(Number number1, Number number2)
        {
            if (number1 == null)
                throw new ArgumentNullException("number1");

            if (number2 == null)
                throw new ArgumentNullException("number2");

            Number rv = Clone(number1);

            //the add process
            var addNode1 = rv.ZerothDigit;
            var addNode2 = number2.ZerothDigit;

            //add after the decimal
            while (addNode2 != null)
            {
                addNode1.Value.Add(addNode2.Value.Digit.Symbol);
                addNode1 = addNode1.Value.NextDigit.DigitNode;
                addNode2 = addNode2.NextNode;
            }

            //add before the decimal
            addNode1 = rv.ZerothDigit.Value.PreviousDigit.DigitNode;
            addNode2 = number2.ZerothDigit.PreviousNode;

            while (addNode2 != null)
            {
                addNode1.Value.Add(addNode2.Value.Digit.Symbol);
                addNode1 = addNode1.Value.PreviousDigit.DigitNode;
                addNode2 = addNode2.PreviousNode;
            }
            return rv;
        }
        /// <summary>
        /// treats numbers as signless.  assumes number1 is longer than number 2
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decnumber"></param>
        /// <returns></returns>
        private static Number Decrement(Number number1, Number number2)
        {
            if (number1 == null)
                throw new ArgumentNullException("number1");

            if (number2 == null)
                throw new ArgumentNullException("number2");

            Number rv = Clone(number1);

            //the add process
            var addNode1 = rv.ZerothDigit;
            var addNode2 = number2.ZerothDigit;

            //subtract after the decimal
            while (addNode2 != null)
            {
                addNode1.Value.Subtract(addNode2.Value.Digit.Symbol);
                addNode1 = addNode1.Value.NextDigit.DigitNode;
                addNode2 = addNode2.NextNode;
            }

            //subtract before the decimal
            addNode1 = rv.ZerothDigit.PreviousNode;
            addNode2 = number2.ZerothDigit.PreviousNode;

            while (addNode2 != null)
            {
                addNode1.Value.Subtract(addNode2.Value.Digit.Symbol);
                addNode1 = addNode1.Value.PreviousDigit.DigitNode;
                addNode2 = addNode2.PreviousNode;
            }
            return rv;
        }
        #endregion

        #region Methods
        /// <summary>
        /// returns the most significant digit.  walks to the last node, then walks back stopping
        /// at the first non-zero until it gets to Zeroth Node and returns that.
        /// </summary>
        public LinkedListNode<NumberDigit> GetMostSignificantDigit()
        {
            this.ScrubLeadingAndTrailingZeroes();
            return this.LastNode;
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

            //iterate thru those lists and set the values
            var thisNode = this.ZerothDigit;
            //reverse so we're adding digits from the decimal outwards, and then from the decimal inwards
            postDecimalSymbols.Reverse();
            foreach (var each in postDecimalSymbols)
            {
                if (each.Equals(this.NumberSystem.NegativeSymbol))
                    continue;

                thisNode.Value.Digit.SetValue(each);
                thisNode = thisNode.Value.NextDigit.DigitNode;
            }
            thisNode = null;
            foreach (var each in preDecimalSymbols)
            {
                if (each.Equals(this.NumberSystem.NegativeSymbol))
                    continue;

                thisNode.Value.PreviousDigit.Digit.SetValue(each);
                thisNode = thisNode.Value.PreviousDigit.DigitNode;
            }
        }

        public void Add(Number number)
        {
            if (_isDisabled)
                throw new InvalidOperationException("dang");

            if (number == null)
                return;

            var val = Add(this, number);
            HydrateFrom(this, val);
        }
        public void Subtract(Number number)
        {
            if (_isDisabled)
                throw new InvalidOperationException("dang");

            if (number == null)
                return;
            var negNumber = Clone(number);
            negNumber.SwitchSign();

            var val = Add(this, negNumber);
            HydrateFrom(this, val);
        }
        public void AddOne()
        {
            this.Add(new Number(this.NumberSystem.OneSymbol, this.NumberSystem));
        }
        public void SubtractOne()
        {
            this.Subtract(new Number(this.NumberSystem.OneSymbol, this.NumberSystem));
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

            num1.AddOne();
            Debug.Assert(num1.Text == "1234567899");
            var counter = 1234567899;
            for (int i = 1; i < 100; i++)
            {
                num1.AddOne();
                counter++;
                Debug.Assert(num1.Text == counter.ToString());
            }
            for (int i = 1; i < 100; i++)
            {
                num1.SubtractOne();
                counter--;
                Debug.Assert(num1.Text == counter.ToString());
            }
            var num2 = new Number("0", set);
            counter = 0;
            for (int i = 0; i < 200; i++)
            {
                num2.SubtractOne();
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
                num1.AddOne();
                number++;
                Debug.Assert(num1.Text == number.ToString());
            }

            for (int i = 0; i < 1000; i++)
            {
                num1.SubtractOne();
                number--;
                Debug.Assert(num1.Text == number.ToString());
            }

        }
    }
}
