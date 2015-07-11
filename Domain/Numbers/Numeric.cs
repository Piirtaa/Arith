﻿using System;
using System.Linq;
using System.Text;
using Arith.Domain.Digits;
using System.Diagnostics;
using Arith.DataStructures;

namespace Arith.Domain.Numbers
{
    /// <summary>
    /// puts numeric semantics around a linked list of digits
    /// </summary>
    public class Numeric : LinkedList<IDigit>, INumeric
    {
        #region Declarations
        /// <summary>
        /// flag to turn off the entire functionality of this class
        /// </summary>
        private static bool _isDisabled = false;

        protected DigitNode _zerothDigit = null;
        protected NumeralSet _numberSystem = null;
        protected bool _isPositive = true;
        #endregion

        #region Ctor
        public Numeric(NumeralSet numberSystem, string digits = null)
        {
            if (numberSystem == null)
                throw new ArgumentNullException("numberSystem");

            this._numberSystem = numberSystem;

            //define builder strategy
            this.NodeBuildingStrategy = (x) =>
            {
                return new DigitNode(x, this);
            };

            this.SetValue(digits);
        }
        #endregion

        #region Fluent Static
        public static Numeric New(NumeralSet numberSystem, string digits = null)
        {
            return new Numeric(numberSystem, digits);
        }
        #endregion

        #region Properties
        public DigitNode ZerothDigit { get { return this._zerothDigit; } }
        public DigitNode LastDigit { get { return this._lastNode as DigitNode; } }
        public DigitNode FirstDigit { get { return this._firstNode as DigitNode; } }
        #endregion

        #region IIsNumeric
        public bool IsPositive { get { return this._isPositive; } set { this._isPositive = value; } }
        public NumeralSet NumberSystem { get { return this._numberSystem; } }
        /// <summary>
        /// by default this represents the leading zero, trailing zero trimmed value 
        /// </summary>
        public virtual string SymbolsText
        {
            get
            {
                return this.GetSimpleFormat();
            }
        }

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
                this._firstNode = null;
                this._lastNode = null;
                this._zerothDigit = null;
                this._isPositive = true;

                if (symbols != null && symbols.Length > 0)
                {
                    bool isZeroSet = false;

                    //set sign
                    if (symbols[0].Equals(this.NumberSystem.NegativeSymbol))
                        this._isPositive = false;

                    DigitNode currentNode = null;

                    foreach (var each in symbols)
                    {
                        if (each.Equals(this.NumberSystem.NegativeSymbol))
                            continue;

                        //if decimal set zeroth 
                        if (each.Equals(this.NumberSystem.DecimalSymbol))
                        {
                            this._zerothDigit = currentNode;
                            isZeroSet = true;
                            continue;
                        }

                        currentNode = this.AddLeastSignificantDigit(each);
                    }

                    if (!isZeroSet)
                        this._zerothDigit = this._firstNode as DigitNode;

                    this.ScrubLeadingAndTrailingZeroes();
                }
                else
                {
                    //keeps a null list
                }
            }
        }

        public bool? Compare(INumeric number)
        {
            if (number == null)
                return true;

            if (!(number is INumeric))
                throw new ArgumentOutOfRangeException("invalid number");

            //if sign difference return
            if (number.IsPositive && this.IsPositive == false)
                return false;

            if (this.IsPositive && number.IsPositive == false)
                return true;

            return AbsoluteValueCompare(this, number as Numeric);
        }

        public INumeric Clone()
        {
            Numeric rv = new Numeric(this.NumberSystem, null);
            rv._isPositive = this._isPositive;

            this.Iterate((node) =>
            {
                DigitNode dNode = node as DigitNode;
                var newNode = rv.AddLeastSignificantDigit(dNode.Symbol);
                if (dNode.IsZerothDigit())
                {
                    rv._zerothDigit = newNode;
                }
            }, false);

            return rv;
        }
        #endregion

        #region Overrides
        public override ILinkedList<IDigit> Remove(ILinkedListNode<IDigit> item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            DigitNode node = item as DigitNode;
            if (node.IsZerothDigit())
            {
                //reset zeroth to next
                if (node.NextNode == null)
                    throw new InvalidOperationException("cannot remove zeroth digit");

                this._zerothDigit = node.NextNode as DigitNode;
            }
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

                while (this.LastDigit != null &&
                    this.LastDigit.IsZerothDigit() == false &&
                    this.LastDigit.IsZero())
                    this.Remove(this.LastNode);

                while (this.FirstDigit != null &&
                    this.FirstDigit.IsZerothDigit() == false &&
                    this.FirstDigit.IsZero())
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
        public void SetValue(Numeric number)
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
                if (dNode.IsZerothDigit())
                {
                    number._zerothDigit = newNode;
                }
            }, false);

            this.ScrubLeadingAndTrailingZeroes();
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// compares the length of 2 numbers. ALWAYS does a zero trim on all args.
        /// false = this is less, true= this is greater, null = equal.  ignores sign
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool? AbsoluteValueCompare(Numeric thisNumber, Numeric number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            if (number == null)
                throw new ArgumentNullException("number");

            //validate the same number systems
            if (!thisNumber.NumberSystem.IsCompatible(number.NumberSystem))
                throw new InvalidOperationException("incompatible number systems");

            thisNumber.ScrubLeadingAndTrailingZeroes();
            number.ScrubLeadingAndTrailingZeroes();

            //iterate to the most significant digit starting at Zeroth
            ILinkedListNode<IDigit> d1 = thisNumber.ZerothDigit;
            ILinkedListNode<IDigit> d2 = number.ZerothDigit;

            bool? thisIsLarger = null;
            while (true)
            {
                //d2 is longer thus greater
                if (d1.NextNode == null && d2.NextNode != null)
                {
                    thisIsLarger = false;
                    break;
                }
                //d1 is longer thus greater
                if (d2.NextNode == null && d1.NextNode != null)
                {
                    thisIsLarger = true;
                    break;
                }
                //if there are no more nodes, it's a most significant digit compare
                if (d1.NextNode == null && d2.NextNode == null)
                {
                    thisIsLarger = null;
                    break;
                }
                d1 = d1.NextNode;
                d2 = d2.NextNode;
            }

            if (thisIsLarger != null)
                return thisIsLarger;

            //they have the same whole number length, so we need a node by node value compare
            //now walk back from the most significant nodes, d1, d2 respectively
            //and see who is bigger using a digit by digit compare
            while (true)
            {
                if (d1 == null && d2 != null)
                {
                    thisIsLarger = false;
                    break;
                }

                if (d2 == null && d1 != null)
                {
                    thisIsLarger = true;
                    break;
                }

                if (d2 == null && d1 == null)
                {
                    thisIsLarger = null;
                    break;
                }
                var comp = d1.Value.Compare(d2.Value.Symbol);
                if (comp != null)
                {
                    thisIsLarger = comp;
                    break;
                }
                d1 = d1.PreviousNode;
                d2 = d2.PreviousNode;
            }
            return thisIsLarger;
        }

        #endregion
    }


}
