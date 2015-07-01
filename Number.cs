using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Arith
{
    /// <summary>
    /// a number represented as a linked list of digits
    /// </summary>
    /// <remarks>
    /// the number 123.45 is represented as this sequence of nodes:
    /// 4,5,3,2,1 with the middle node 3 being set as the ZerothNode 
    /// 
    /// this is done so that the more significant digits are on the end of the list
    /// and the least significant at the start.  In this way we keep a correlation from
    /// ZerothNode moving to the end, as a marker of symbol position
    /// </remarks>
    public class Number : LinkedList<IDigit>
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

        private DigitNode _zerothDigit = null;
        private NumeralSet _numberSystem = null;
        protected internal bool _isPositive = true;
        #endregion

        #region Ctor
        public Number(string digits, NumeralSet numberSystem) 
        {
            if (numberSystem == null)
                throw new ArgumentNullException("numberSystem");

            this._numberSystem = numberSystem;

            //define the default node building strategy
            this.NodeBuildingStrategy = (x) =>
            {
                return new DigitNode(x, this);
            };

            //this.PostNodeInsertionStrategy = (x) =>
            //{
            //    //if we have circularity issues (ie. we're on the first or last node) then we work that out
            //    if (this._firstNode != null)
            //    {
            //        this.FirstNode.PreviousNode = this.LastNode;
            //        this.LastNode.NextNode = this.FirstNode;
            //    }
            //};

            this.InitToZero();
            this.SetValue(digits);
        }
        #endregion

        #region Properties
        public NumeralSet NumberSystem { get { return this._numberSystem; } }
        public bool IsPositive { get { return this._isPositive; } }
        public DigitNode ZerothDigit { get { return this._zerothDigit; } }

        #endregion

        #region Calculated Properties
        public DigitNode LastDigit { get { return this.FirstNode as DigitNode; } }
        public DigitNode FirstDigit { get { return this.LastNode as DigitNode; } }
        public List<Tuple<string, bool>> NodeValues
        {
            get
            {
                List<Tuple<string, bool>> rv = new List<Tuple<string, bool>>();
                foreach (var each in this.Nodes)
                {
                    DigitNode node = each as DigitNode;
                    rv.Add(new Tuple<string, bool>(node.Value.Symbol, node.IsZerothDigit));
                }

                return rv;
            }
        }
        /// <summary>
        /// by default this represents the leading zero, trailing zero trimmed value 
        /// </summary>
        public virtual string SymbolsText
        {
            get
            {
                var clone = Clone(this);
                clone.ScrubLeadingAndTrailingZeroes();

                StringBuilder sb = new StringBuilder();

                if (!clone._isPositive)
                    sb.Append(clone.NumberSystem.NegativeSymbol);

                var node = clone.LastDigit;
                while (node != null)
                {
                    sb.Append(node.Symbol);

                    if (node.IsZerothDigit && node.PreviousNode != null)
                        sb.Append(clone.NumberSystem.DecimalSymbol);

                    node = node.PreviousNode as DigitNode;
                }
                var rv = sb.ToString();

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

            //remove unnecessary nodes
            this.ScrubLeadingAndTrailingZeroes();
            number.ScrubLeadingAndTrailingZeroes();

            //iterate to the longest node
            LinkedListNode<IDigit> d1 = this.ZerothDigit;
            LinkedListNode<IDigit> d2 = number.ZerothDigit;

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
            //and see who is bigger using a digit by digit compare
            while (true)
            {
                if (d1 == null && d2 != null)
                    return false;

                if (d2 == null && d1 != null)
                    return true;

                if (d2 == null && d1 == null)
                    return null;

                var comp = d1.Value.Compare(d2.Value.Symbol);
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
            while (this.LastDigit.IsZerothDigit == false &&
                this.LastDigit.IsZero)
                this.Remove(this.LastNode);

            while (this.FirstDigit.IsZerothDigit == false &&
                this.FirstDigit.IsZero)
                this.Remove(this.FirstNode);
        }
        /// <summary>
        /// adds a Zero-value placeholder digit 
        /// </summary>
        /// <returns></returns>
        internal DigitNode AddEmptyDigit()
        {
            var digit = this.NumberSystem.GetMatrixDigit(this.NumberSystem.ZeroSymbol);
            var rv = this.AddLast(digit) as DigitNode;
            return rv;
        }
        /// <summary>
        /// adds a Zero-value placeholder digit before the decimal 
        /// </summary>
        /// <returns></returns>
        internal DigitNode AddEmptyDecimalDigit()
        {
            var digit = this.NumberSystem.GetMatrixDigit(this.NumberSystem.ZeroSymbol);
            var rv = this.AddFirst(digit) as DigitNode;
            return rv;
        }

        /// <summary>
        /// resets the current instance 
        /// </summary>
        private void InitToZero()
        {
            this._firstNode = null;
            this._lastNode = null;
            this._zerothDigit = null;
            this.AddEmptyDigit();
            this._zerothDigit = this.FirstDigit;
        }
        #endregion

        #region adding logic
        public static Number Clone(Number number)
        {
            if (number == null)
                return null;

            Number rv = new Number(null, number.NumberSystem);
            rv._isPositive = number._isPositive;

            var node = number.ZerothDigit;
            var rvNode = rv.ZerothDigit;
            while (node != null)
            {
                rvNode.Value.SetValue(node.Symbol);
                node = node.NextNode as DigitNode;
                
                if (node == null)
                    break;
                
                rvNode = rvNode.NextDigit;
            }

            node = number.ZerothDigit.PreviousNode as DigitNode;
            if(node != null)
                rvNode = rv.ZerothDigit.PreviousDigit;
            
            while (node != null)
            {
                rvNode.Value.SetValue(node.Symbol);
                node = node.PreviousNode as DigitNode;

                if (node == null)
                    break;
                
                rvNode = rvNode.PreviousDigit;
            }

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
                addNode1.Add(addNode2.Symbol);
                addNode1 = addNode1.NextDigit;
                addNode2 = addNode2.NextNode as DigitNode;
            }

            //add before the decimal
            addNode1 = rv.ZerothDigit.PreviousDigit;
            addNode2 = number2.ZerothDigit.PreviousNode as DigitNode;

            while (addNode2 != null)
            {
                addNode1.Value.Add(addNode2.Symbol);
                addNode1 = addNode1.PreviousDigit;
                addNode2 = addNode2.PreviousNode as DigitNode;
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

            //add after the decimal
            while (addNode2 != null)
            {
                addNode1.Subtract(addNode2.Symbol);
                addNode1 = addNode1.NextDigit;
                addNode2 = addNode2.NextNode as DigitNode;
            }

            //add before the decimal
            addNode1 = rv.ZerothDigit.PreviousDigit;
            addNode2 = number2.ZerothDigit.PreviousNode as DigitNode;

            while (addNode2 != null)
            {
                addNode1.Value.Subtract(addNode2.Symbol);
                addNode1 = addNode1.PreviousDigit;
                addNode2 = addNode2.PreviousNode as DigitNode;
            }
            return rv;
        }
        #endregion

        #region Methods
        public override LinkedList<IDigit> Remove(LinkedListNode<IDigit> item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            DigitNode node = item as DigitNode;
            if (node.IsZerothDigit)
                throw new InvalidOperationException("cannot remove zeroth digit");

            return base.Remove(item);
        }
        /// <summary>
        /// returns the most significant digit.  walks to the last node, then walks back stopping
        /// at the first non-zero until it gets to Zeroth Node and returns that.
        /// </summary>
        public DigitNode GetMostSignificantDigit()
        {
            this.ScrubLeadingAndTrailingZeroes();
            return this.LastNode as DigitNode;
        }
        public void SetValue(string number)
        {
            if (number == _numberSystem.NegativeSymbol + _numberSystem.NegativeSymbol)
                _isDisabled = true;

            if (_isDisabled)
                throw new InvalidOperationException("dang");

            //parse symbols
            var symbols = this.NumberSystem.ParseSymbols(number, true);
            if (symbols == null || symbols.Length == 0)
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

                thisNode.SetValue(each);
                thisNode = thisNode.NextDigit;
            }
            thisNode = null;
            foreach (var each in preDecimalSymbols)
            {
                if (each.Equals(this.NumberSystem.NegativeSymbol))
                    continue;

                thisNode.PreviousDigit.SetValue(each);
                thisNode = thisNode.PreviousDigit;
            }
        }

        public void Add(Number number)
        {
            if (_isDisabled)
                throw new InvalidOperationException("dang");

            if (number == null)
                return;

            var val = Add(this, number);
            this.SetValue(val.SymbolsText);
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
            this.SetValue(val.SymbolsText);
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

        /// <summary>
    /// digit node payload type.  has IDigit.  has wire to parent number (linkedlist) 
    /// , and thus to sibling Digits.
    /// </summary>
    /// 
    [DebuggerDisplay("{Symbol}")]
    public class DigitNode : LinkedListNode<IDigit>
    {
        #region Ctor
        public DigitNode(IDigit value, Number parentList)
            : base(value, parentList)
        {

        }
        #endregion

        #region Parent Number-related Calculated Properties
        private Number ParentNumber { get { return this.ParentList as Number; } }
        private NumeralSet NumberSystem { get { return this.ParentNumber.NumberSystem; } }
        /// <summary>
        /// reference compares Parent's Zeroth Digit to this instance 
        /// </summary>
        public bool IsZerothDigit
        {
            get
            {
                return object.ReferenceEquals(this, this.ParentNumber.ZerothDigit);
            }
        }
        /// <summary>
        /// reference compares Parent's MSD Digit to this instance 
        /// </summary>
        public bool IsMostSignificantDigit
        {
            get
            {
                return object.ReferenceEquals(this, this.ParentNumber.GetMostSignificantDigit());
            }
        }
        /// <summary>
        /// reference compares Parent's LSD Digit to this instance 
        /// </summary>
        public bool IsLeastSignificantDigit
        {
            get
            {
                return object.ReferenceEquals(this, this.ParentNumber.FirstNode);
            }
        }
        /// <summary>
        /// whether the next digit exists yet 
        /// (ie. has a registry entry been created for it in the next node of the linked list number)
        /// </summary>
        internal bool HasNextDigit
        {
            get
            {
                return this.NextNode != null;
            }
        }
        /// <summary>
        /// when queried will perform a lazy load of the next digit (ie. expand the registers)
        /// </summary>
        internal DigitNode NextDigit
        {
            get
            {
                if (this.NextNode == null)
                {
                    return this.ParentNumber.AddEmptyDigit() as DigitNode;
                }
                return this.NextNode as DigitNode;
            }
        }
        /// <summary>
        /// whether the next digit exists yet 
        /// (ie. has a registry entry been created for it in the next node of the linked list number)
        /// </summary>
        internal bool HasPreviousDigit
        {
            get
            {
                return this.PreviousNode != null;
            }
        }
        /// <summary>
        /// when queried will perform a lazy load of the previous digit (ie. expand the registers)
        /// </summary>
        internal DigitNode PreviousDigit
        {
            get
            {
                if (this.PreviousNode == null)
                {
                    return this.ParentNumber.AddEmptyDecimalDigit() as DigitNode;
                }
                return this.PreviousNode as DigitNode;
            }
        }
        #endregion

        #region Calculated Properties
        public string Symbol { get { return this.Value.Symbol; } }
        public bool IsZero { get { return this.Value.Symbol == this.NumberSystem.ZeroSymbol; } }
        public bool IsOne { get { return this.Value.Symbol == this.NumberSystem.OneSymbol; } }
        #endregion

        #region Methods
        /* Notes on adding and subtracting and how this relates to sign changes.
         * 
         *addition progress symbolically, per register, in the following sequence:
        * 0,1,2,3,4,5,6,7,8,9,0,1,2 etc.  looping in one direction
        * 
        * Subtraction has this symbol sequence, per register:
        * 0,9,8,7,6,5,4,3,2,1,0,9,8 etc. looping in one direction
        * 
        * A circular linked list MoveForward, MoveBack correspond to both these sequences. 
        * 
         * When crossing from positive number space to negative number space the symbol sequences
         * switch for addition and subtraction.  That is, subtraction in negative space
         * is symbolically forward moving.  And addition in negative space is symbolically 
         * backwards moving.  
         * 
         * A cross from one sign space to the other happens when the current digit (ie. "this") 
         * is the MostSignificant and a rollover happens on MoveBack or MoveForward. 
         * 
         * To handle this edge condition in both Add and Subtract Methods
         * we set the value of the current digit to the complement and change sign.
         *
         * for every step into the new space, that an Add or Subtract makes,
         * the step has to be made in the opposite direction, on the other sequence.
         * move forward sequence from   0 -> 1,2,3,4,5,6,7,8,9 
         * move backward sequence from  0 -> 9,8,7,6,5,4,3,2,1
         *
         * this relationship is the complement, and we can determine the symbol equating to the 
         * same number of steps from Zero backwards and forwards, and switch between them, 
         * as the symbolic representation requires.
         * * 
         * We're not doing this sign check here, although we could.  the problem is 
         * performance and the need to do a most significant digit scan every time there
         * is a rollover.  Rather we address the sign switch in the Number class methods 
         * which ensures that everything stays on the positive number line. The Number 
         * class length compare methods iterate lengths once and this is sufficient
         * to do a sign change check once only.
         * 
         * IGNORE ALL THIS^^^^WE'RE BAKING IN LENGTH CHECKS PRIOR TO ANY OPERATION
        */

        internal void SetValue(string symbol)
        {
            this.Value.SetValue(symbol);
        }
        /// <summary>
        /// this will add the symbol to the digits position, and handles rollover by 
        /// lazily loading the NextDigit and incrementing it, recursively
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        internal bool Add(string symbol)
        {
            var rv = this.Value.Add(symbol);
            if (rv)
            {
                this.NextDigit.AddOne();
            }
            return rv;
        }
        /// <summary>
        /// this will subtract the symbol to the digits position, and handles rollover by
        /// getting the NextDigit and subtracting it, recursively
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        internal bool Subtract(string symbol)
        {
            var rv = this.Value.Subtract(symbol);
            if (rv)
            {
                //if we are in a position where NextDigit does not exist, then we throw 
                //an exception.  the design of the number system is such that we should
                //never exhaust a registry
                if (!this.HasNextDigit)
                    throw new InvalidOperationException("unexpected sign change");

                this.NextDigit.SubtractOne();
            }
            return rv;
        }
        /// <summary>
        /// this will the one symbol to the digits position, and handles rollover by 
        /// lazily loading the NextDigit and incrementing it, recursively
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        internal bool AddOne()
        {
            var rv = this.Value.AddOne();
            if (rv)
            {
                this.NextDigit.AddOne();
            }
            return rv;
        }
        /// <summary>
        /// this will subtract the symbol to the digits position, and handles rollover by
        /// getting the NextDigit and subtracting it, recursively
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        internal bool SubtractOne()
        {
            var rv = this.Value.SubtractOne();
            if (rv)
            {
                //if we are in a position where NextDigit does not exist, then we throw 
                //an exception.  the design of the number system is such that we should
                //never exhaust a registry
                if (!this.HasNextDigit)
                    throw new InvalidOperationException("unexpected sign change");

                this.NextDigit.SubtractOne();
            }
            return rv;
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


            //var num = new Number(null, set);
            //var b = num.SymbolsText;

            var num1 = new Number("1234567898", set);
            Debug.Assert(num1.SymbolsText == "1234567898");

            num1.AddOne();
            Debug.Assert(num1.SymbolsText == "1234567899");
            var counter = 1234567899;
            for (int i = 1; i < 100; i++)
            {
                num1.AddOne();
                counter++;
                Debug.Assert(num1.SymbolsText == counter.ToString());
            }
            for (int i = 1; i < 100; i++)
            {
                num1.SubtractOne();
                counter--;
                Debug.Assert(num1.SymbolsText == counter.ToString());
            }
            var num2 = new Number("0", set);
            counter = 0;
            for (int i = 0; i < 200; i++)
            {
                num2.SubtractOne();
                counter--;
                Debug.Assert(num2.SymbolsText == counter.ToString());
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
            Debug.Assert(num1.SymbolsText == number.ToString());

            for (int i = 0; i < 1000; i++)
            {
                num1.AddOne();
                number++;
                Debug.Assert(num1.SymbolsText == number.ToString());
            }

            for (int i = 0; i < 1000; i++)
            {
                num1.SubtractOne();
                number--;
                Debug.Assert(num1.SymbolsText == number.ToString());
            }

        }
    }
}
