using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Arith.DataStructures;

namespace Arith.Domain
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
    /// 
    [DebuggerDisplay("{SymbolsText}")]
    public class SymbolicNumber : Arith.DataStructures.LinkedList<IDigit>, ISymbolicNumber
    {
        #region Declarations
        /// <summary>
        /// flag to turn off the entire functionality of this class
        /// </summary>
        private static bool _isDisabled = false;

        private DigitNode _zerothDigit = null;
        private NumeralSet _numberSystem = null;
        protected internal bool _isPositive = true;
        #endregion

        #region Ctor
        public SymbolicNumber(string digits, NumeralSet numberSystem)
        {
            if (numberSystem == null)
                throw new ArgumentNullException("numberSystem");

            this._numberSystem = numberSystem;

            //define the default node building strategy
            this.NodeBuildingStrategy = (x) =>
            {
                return new DigitNode(x, this);
            };

            this.PostNodeInsertionStrategy = (x) =>
            {
                Debug.WriteLine("post insert on " + x.Value.Symbol + " producing " + this.SymbolsText);

                DigitNode node = x as DigitNode;

                //set zeroth digit if it hasn't been
                if (this.ZerothDigit == null)
                    this._zerothDigit = node;
            };

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
        public DigitNode LastDigit { get { return this.LastNode as DigitNode; } }
        public DigitNode FirstDigit { get { return this.FirstNode as DigitNode; } }
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
                StringBuilder sb = new StringBuilder();
                if (!this._isPositive)
                    sb.Append(this.NumberSystem.NegativeSymbol);

                bool isLeadingZero = true;
                var mostSigNodesDesc = this.Nodes.Reverse();
                foreach (var each in mostSigNodesDesc)
                {
                    DigitNode node = each as DigitNode;
                    //ignore leading zeroes
                    if (isLeadingZero && node.IsZero && node.IsZerothDigit == false)
                    {
                        continue;
                    }
                    else
                    {
                        isLeadingZero = false;
                    }

                    sb.Append(node.Symbol);
                    if (node.IsZerothDigit)
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
        public bool? Compare(ISymbolicNumber number)
        {
            if (number == null)
                return true;

            if (!(number is SymbolicNumber))
                throw new ArgumentOutOfRangeException("invalid number");

            //if sign difference return
            if (number.IsPositive && this.IsPositive == false)
                return false;

            if (this.IsPositive && number.IsPositive == false)
                return true;

            return LengthCompare(this, number as SymbolicNumber);
        }
        #endregion

        #region Overrides
        public override Arith.DataStructures.LinkedList<IDigit> Remove(Arith.DataStructures.LinkedListNode<IDigit> item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            DigitNode node = item as DigitNode;
            if (node.IsZerothDigit)
                throw new InvalidOperationException("cannot remove zeroth digit");

            return base.Remove(item);
        }
        #endregion

        #region List Manipulation, Numeric Transformations
        public void SwitchSign()
        {
            this._isPositive = !this._isPositive;
        }
        /// <summary>
        /// removes leading and trailing zeroes
        /// </summary>
        public void ScrubLeadingAndTrailingZeroes()
        {
            lock (this._stateLock)
            {
                while (this.LastDigit.IsZerothDigit == false &&
                    this.LastDigit.IsZero)
                    this.Remove(this.LastNode);

                while (this.FirstDigit.IsZerothDigit == false &&
                    this.FirstDigit.IsZero)
                    this.Remove(this.FirstNode);
            }
        }
        /// <summary>
        /// adds a digit at the end of the list
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public DigitNode AddMostSignificantDigit(string symbol)
        {
            var digit = this.NumberSystem.GetMatrixDigit(symbol);
            lock (this._stateLock)
            {
                var rv = this.AddLast(digit) as DigitNode;
                return rv;
            }
        }
        /// <summary>
        /// adds a Zero digit at the end of the list
        /// </summary>
        /// <returns></returns>
        public DigitNode AddMostSignificantZeroDigit()
        {
            var digit = this.NumberSystem.GetMatrixDigit(this.NumberSystem.ZeroSymbol);

            lock (this._stateLock)
            {
                var rv = this.AddLast(digit) as DigitNode;
                return rv;
            }
        }
        /// <summary>
        /// adds a Zero digit at the start of the list
        /// </summary>
        /// <returns></returns>
        public DigitNode AddLeastSignificantZeroDigit()
        {
            var digit = this.NumberSystem.GetMatrixDigit(this.NumberSystem.ZeroSymbol);
            lock (this._stateLock)
            {
                var rv = this.AddFirst(digit) as DigitNode;
                return rv;
            }
        }
        /// <summary>
        /// adds a digit at the start of the list
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public DigitNode AddLeastSignificantDigit(string symbol)
        {
            var digit = this.NumberSystem.GetMatrixDigit(symbol);
            lock (this._stateLock)
            {
                var rv = this.AddFirst(digit) as DigitNode;
                return rv;
            }
        }
        /// <summary>
        /// moves the decimal to the right (towards LSD) - an order of magnitude increase.
        /// if the LSD rightmost position doesn't exist, it is added.
        /// </summary>
        public void ShiftRight()
        {
            lock (this._stateLock)
            {
                var node = this.ZerothDigit.PreviousNode as DigitNode;
                if (node == null)
                {
                    node = this.AddLeastSignificantZeroDigit();
                }

                //move the decimal
                this._zerothDigit = node;
            }
        }
        /// <summary>
        /// moves the decimal to the left (toward MSD) - an order of magnitude decrease.
        /// if the leftmost(MSD) position doesn't exist, it is added.
        /// </summary>
        public void ShiftLeft()
        {
            lock (this._stateLock)
            {
                var node = this.ZerothDigit.NextNode as DigitNode;
                if (node == null)
                {
                    node = this.AddMostSignificantZeroDigit();
                }

                //move the decimal
                this._zerothDigit = node;
            }
        }
        #endregion

        #region List Walking
        /// <summary>
        /// iterates towards MSD starting at Zeroth, performing postZeroAction.  iterates
        /// towards LSD starting at node prior to Zeroth, performing preZeroAction 
        /// </summary>
        /// <param name="postZeroAction"></param>
        /// <param name="preZeroAction"></param>
        public void IterateFromZeroth(Action<DigitNode, SymbolicNumber> postZeroAction, Action<DigitNode, SymbolicNumber> preZeroAction)
        {

            //the add process
            var node = this.ZerothDigit;

            SymbolicNumber index = new SymbolicNumber(this.NumberSystem.ZeroSymbol, this.NumberSystem);
            while (node != null)
            {
                postZeroAction(node, index);
                index.AddOne();
                node = node.NextNode as DigitNode;
            }

            node = this.ZerothDigit.PreviousNode as DigitNode;
            
            index = new SymbolicNumber(this.NumberSystem.OneSymbol, this.NumberSystem);
            while (node != null)
            {  
                preZeroAction(node, index);
                index.AddOne();

                node = node.PreviousNode as DigitNode;
            }
        }
        #endregion

        #region INumber Methods
        //public void SetValue(string number)
        //{
        //    if (number == _numberSystem.NegativeSymbol + _numberSystem.NegativeSymbol)
        //        _isDisabled = true;

        //    if (_isDisabled)
        //        throw new InvalidOperationException("dang");

        //    //parse symbols
        //    var symbols = this.NumberSystem.ParseSymbols(number, true);
        //    if (symbols == null || symbols.Length == 0)
        //        return;

        //    //set sign
        //    if (symbols[0].Equals(this.NumberSystem.NegativeSymbol))
        //        this._isPositive = false;

        //    //parse the symbols into postdecimal and predecimal lists
        //    var postDecimalSymbols = new List<string>();
        //    var preDecimalSymbols = new List<string>();
        //    bool hasDecimal = false;
        //    foreach (var each in symbols)
        //    {
        //        if (each.Equals(this.NumberSystem.NegativeSymbol))
        //            continue;

        //        if (each.Equals(this.NumberSystem.DecimalSymbol))
        //        {
        //            hasDecimal = true;
        //            continue;
        //        }

        //        if (!hasDecimal)
        //        {
        //            postDecimalSymbols.Add(each);
        //        }
        //        else
        //        {
        //            preDecimalSymbols.Add(each);
        //        }
        //    }

        //    this._firstNode = null;
        //    this._lastNode = null;
        //    this._zerothDigit = null;

        //    //iterate thru those lists and set the values
        //    //reverse so we're adding digits from the decimal outwards, and then from the decimal inwards
        //    postDecimalSymbols.Reverse();
        //    foreach (var each in postDecimalSymbols)
        //    {
        //        if (each.Equals(this.NumberSystem.NegativeSymbol))
        //            continue;
        //        this.AddMostSignificantDigit(each);
        //    }
        //    foreach (var each in preDecimalSymbols)
        //    {
        //        if (each.Equals(this.NumberSystem.NegativeSymbol))
        //            continue;

        //        this.AddLeastSignificantDigit(each);
        //    }

        //    this.ScrubLeadingAndTrailingZeroes();
        //}
        public void SetValue(string number)
        {
            if (number == _numberSystem.NegativeSymbol + _numberSystem.NegativeSymbol)
                _isDisabled = true;

            if (_isDisabled)
                throw new InvalidOperationException("dang");

            //parse symbols
            var symbols = this.NumberSystem.ParseSymbols(number, true);
            
            lock (this._stateLock)
            {
                //set sign
                if (symbols[0].Equals(this.NumberSystem.NegativeSymbol))
                    this._isPositive = false;

                this._firstNode = null;
                this._lastNode = null;
                this._zerothDigit = null;

                DigitNode currentNode = null;
                //parse the symbols into postdecimal and predecimal lists
                foreach (var each in symbols)
                {
                    if (each.Equals(this.NumberSystem.NegativeSymbol))
                        continue;

                    //if decimal set zeroth 
                    if (each.Equals(this.NumberSystem.DecimalSymbol))
                    {
                        this._zerothDigit = currentNode;
                        continue;
                    }

                    currentNode = this.AddLeastSignificantDigit(each);
                }
            }
            this.ScrubLeadingAndTrailingZeroes();
        }
        public void SetValue(SymbolicNumber number)
        {
            if (number == null)
                throw new ArgumentNullException("number");

            this._firstNode = null;
            this._lastNode = null;
            this._zerothDigit = null;
            this._numberSystem = number.NumberSystem;
            this._isPositive = number._isPositive;

            number.Iterate((node) =>
            {
                DigitNode dNode = node as DigitNode;
                var newNode = this.AddLeastSignificantDigit(dNode.Symbol);
                if (dNode.IsZerothDigit)
                {
                    number._zerothDigit = newNode;
                }
            }, false);

            this.ScrubLeadingAndTrailingZeroes();
        }
        public void Add(ISymbolicNumber number)
        {
            if (number == null)
                return;

            if (!(number is SymbolicNumber))
                throw new ArgumentOutOfRangeException("invalid number");

            bool isClone = false;
            var val = Add(this, number as SymbolicNumber, out isClone);

            if(isClone)
                this.SetValue(val);
        }
        public void Subtract(ISymbolicNumber number)
        {
            if (number == null)
                return;

            if (!(number is SymbolicNumber))
                throw new ArgumentOutOfRangeException("invalid number");

            SymbolicNumber symNumber = number as SymbolicNumber;
            symNumber.SwitchSign();
            bool isClone = false;
            var val = Add(this, symNumber, out isClone);
            symNumber.SwitchSign();

            if(isClone)
                this.SetValue(val);
        }

        #endregion

        #region Helpers
        /// <summary>
        /// returns the most significant digit.  walks to the last node, then walks back stopping
        /// at the first non-zero until it gets to Zeroth Node and returns that.
        /// </summary>
        public DigitNode GetMostSignificantDigit()
        {
            this.ScrubLeadingAndTrailingZeroes();
            return this.LastNode as DigitNode;
        }
        /// <summary>
        /// resets the current instance 
        /// </summary>
        private void InitToZero()
        {
            this._firstNode = null;
            this._lastNode = null;
            this._zerothDigit = null;
            this.AddMostSignificantZeroDigit();
            this._zerothDigit = this.FirstDigit;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// compares the length of 2 numbers. ALWAYS does a zero trim on all args.
        /// false = this is less, true= this is greater, null = equal.  ignores sign
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool? LengthCompare(SymbolicNumber thisNumber, SymbolicNumber number)
        {
            if (number == null)
                throw new ArgumentNullException("number");

            thisNumber.ScrubLeadingAndTrailingZeroes();
            number.ScrubLeadingAndTrailingZeroes();

            //iterate to the longest node
            Arith.DataStructures.LinkedListNode<IDigit> d1 = thisNumber.ZerothDigit;
            Arith.DataStructures.LinkedListNode<IDigit> d2 = number.ZerothDigit;

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
        public static SymbolicNumber Clone(SymbolicNumber number)
        {
            if (number == null)
                return null;

            SymbolicNumber rv = new SymbolicNumber(null, number.NumberSystem);
            rv._isPositive = number._isPositive;

            number.Iterate((node) =>
            {
                DigitNode dNode = node as DigitNode;
                var newNode = rv.AddLeastSignificantDigit(dNode.Symbol);
                if (dNode.IsZerothDigit)
                {
                    number._zerothDigit = newNode;
                }
            }, false);

            return rv;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="number1"></param>
        /// <param name="number2"></param>
        /// <param name="setValueIndicated">this is true if the return value is a clone and not the 
        /// same instance as number1</param>
        /// <returns></returns>
        private static SymbolicNumber Add(SymbolicNumber number1, SymbolicNumber number2, out bool isReturnCloned)
        {
            if (_isDisabled)
                throw new InvalidOperationException("dang");

            //the default value is false, as most cases don't need a clone operation
            isReturnCloned = false;

            if (number1 == null)
            {
                isReturnCloned = true;
                return number2;
            }
            if (number2 == null)
                return number1;

            SymbolicNumber rv = null;

            //determine the longer number
            bool? num1IsLonger = LengthCompare(number1, number2);

            //note: below we clone the 2nd arg if it is being modified as part of the operation
            //we don't clone the 1st arg as it is 
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
                        isReturnCloned = true;
                        rv = Decrement(Clone(number2), number1);
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
                        isReturnCloned = true;
                        rv = Decrement(Clone(number2), number1);
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
        private static SymbolicNumber Increment(SymbolicNumber number1, SymbolicNumber number2)
        {
            if (number1 == null)
                throw new ArgumentNullException("number1");

            if (number2 == null)
                throw new ArgumentNullException("number2");

            //the add process
            var addNode1 = number1.ZerothDigit;
            var addNode2 = number2.ZerothDigit;

            //add after the decimal
            while (addNode2 != null)
            {
                addNode1.Add(addNode2.Symbol);

                addNode2 = addNode2.NextNode as DigitNode;
                if (addNode2 != null)
                    addNode1 = addNode1.NextDigit;
            }

            //add before the decimal
            addNode1 = number1.ZerothDigit.PreviousDigit;
            addNode2 = number2.ZerothDigit.PreviousNode as DigitNode;

            while (addNode2 != null)
            {
                addNode1.Value.Add(addNode2.Symbol);

                addNode2 = addNode2.PreviousNode as DigitNode;
                if (addNode2 != null)
                    addNode1 = addNode1.PreviousDigit;
            }
            return number1;
        }
        /// <summary>
        /// treats numbers as signless.  assumes number1 is longer than number 2
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decnumber"></param>
        /// <returns></returns>
        private static SymbolicNumber Decrement(SymbolicNumber number1, SymbolicNumber number2)
        {
            if (number1 == null)
                throw new ArgumentNullException("number1");

            if (number2 == null)
                throw new ArgumentNullException("number2");

            //the add process
            var addNode1 = number1.ZerothDigit;
            var addNode2 = number2.ZerothDigit;

            //add after the decimal
            while (addNode2 != null)
            {
                addNode1.Subtract(addNode2.Symbol);

                addNode2 = addNode2.NextNode as DigitNode;
                if (addNode2 != null)
                    addNode1 = addNode1.NextDigit;
            }

            //add before the decimal
            addNode1 = number1.ZerothDigit.PreviousDigit;
            addNode2 = number2.ZerothDigit.PreviousNode as DigitNode;

            while (addNode2 != null)
            {
                addNode1.Value.Subtract(addNode2.Symbol);

                addNode2 = addNode2.PreviousNode as DigitNode;
                if (addNode2 != null)
                    addNode1 = addNode1.PreviousDigit;
            }
            return number1;
        }
        #endregion
    }

    /// <summary>
    /// digit node payload type.  has IDigit.  has wire to parent number (linkedlist) 
    /// , and thus to sibling Digits.
    /// </summary>
    [DebuggerDisplay("{Symbol}")]
    public class DigitNode : Arith.DataStructures.LinkedListNode<IDigit>
    {
        #region Ctor
        public DigitNode(IDigit value, SymbolicNumber parentList)
            : base(value, parentList)
        {

        }
        #endregion

        #region Parent Number-related Calculated Properties
        public SymbolicNumber ParentNumber { get { return this.ParentList as SymbolicNumber; } }
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
                    return this.ParentNumber.AddMostSignificantZeroDigit() as DigitNode;
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
                    return this.ParentNumber.AddLeastSignificantZeroDigit() as DigitNode;
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

    internal class SymbolicNumberTests
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

            var num1 = new SymbolicNumber("123456789", set);
            var f = num1.FirstDigit;
            var l = num1.LastDigit;

            Debug.Assert(num1.SymbolsText == "123456789");

            num1.AddOne();
            Debug.Assert(num1.SymbolsText == "123456790");
            var counter = 123456790;
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
            var num2 = new SymbolicNumber("0", set);
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
            var num1 = new SymbolicNumber(number.ToString(), set);
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
