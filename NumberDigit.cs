using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Arith
{
    /// <summary>
    /// digit node payload type.  has IDigit.  has wire to parent number (linkedlist) 
    /// , and thus to sibling Digits.
    /// </summary>
    /// 
    [DebuggerDisplay("{Symbol}")]
    public class NumberDigit
    {
        #region Declarations
        private IDigit _digit = null;
        private NumeralSet _numberSystem = null;
        #endregion

        #region Ctor
        /// <summary>
        /// implementation ctor
        /// </summary>
        /// <param name="digit"></param>
        public NumberDigit(IDigit digit, NumeralSet numberSystem)
        {
            if (digit == null)
                throw new ArgumentNullException("digit");
            this._digit = digit;

            if (numberSystem == null)
                throw new ArgumentNullException("numberSystem");
            this._numberSystem = numberSystem;
        }
        /// <summary>
        /// defaults to matrixdigit implementation
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="numberSet"></param>
        public NumberDigit(string symbol, NumeralSet numberSystem)
        {
            this._digit = new MatrixDigit(symbol, numberSystem);
            this._numberSystem = numberSystem;
        }
        #endregion

        #region Properties
        /// <summary>
        /// the IDigit implementation
        /// </summary>
        public IDigit Digit { get { return this._digit; } }
        /// <summary>
        /// backreference to the linkedlistnode that contains this digit, wherein the linkedlist of
        /// digits is the datastructure containing the number
        /// </summary>
        protected internal LinkedListNode<NumberDigit> DigitNode { get; set; }
        #endregion

        #region Parent Number-related Calculated Properties
        protected bool IsWiredToParent { get { return this.DigitNode != null; } }
        protected Number ParentNumber
        {
            get
            {
                return this.DigitNode.ParentList as Number;
            }
        }
        /// <summary>
        /// reference compares Parent's Zeroth Digit to this instance 
        /// </summary>
        public bool IsZerothDigit
        {
            get
            {
                return object.ReferenceEquals(this.DigitNode, this.ParentNumber.ZerothDigit);
            }
        }
        /// <summary>
        /// reference compares Parent's MSD Digit to this instance 
        /// </summary>
        public bool IsMostSignificantDigit
        {
            get
            {
                return object.ReferenceEquals(this.DigitNode, this.ParentNumber.GetMostSignificantDigit());
            }
        }
        /// <summary>
        /// reference compares Parent's LSD Digit to this instance 
        /// </summary>
        public bool IsLeastSignificantDigit
        {
            get
            {
                return object.ReferenceEquals(this.DigitNode, this.ParentNumber.FirstNode);
            }
        }
        /// <summary>
        /// whether the next digit exists yet 
        /// (ie. has a registry entry been created for it in the next node of the linked list number)
        /// </summary>
        public bool HasNextDigit
        {
            get
            {
                if (!this.IsWiredToParent)
                    return false;

                return this.DigitNode.NextNode != null;
            }
        }
        /// <summary>
        /// when queried will perform a lazy load of the next digit (ie. expand the registers)
        /// </summary>
        public NumberDigit NextDigit
        {
            get
            {
                if (!this.IsWiredToParent)
                    return null;

                if (this.DigitNode.NextNode == null)
                {
                    return this.ParentNumber.AddEmptyDigit().Value;
                }
                return this.DigitNode.NextNode.Value;
            }
        }
        /// <summary>
        /// whether the next digit exists yet 
        /// (ie. has a registry entry been created for it in the next node of the linked list number)
        /// </summary>
        public bool HasPreviousDigit
        {
            get
            {
                if (!this.IsWiredToParent)
                    return false;

                return this.DigitNode.PreviousNode != null;
            }
        }
        /// <summary>
        /// when queried will perform a lazy load of the previous digit (ie. expand the registers)
        /// </summary>
        public NumberDigit PreviousDigit
        {
            get
            {
                if (!this.IsWiredToParent)
                    return null;

                if (this.DigitNode.PreviousNode == null)
                {
                    return this.ParentNumber.AddEmptyDecimalDigit().Value;
                }
                return this.DigitNode.PreviousNode.Value;
            }
        }
        #endregion

        #region Calculated Properties
        public string Symbol { get { return this.Digit.Symbol; } }
        public bool IsZero { get { return this._digit.Symbol == this._numberSystem.ZeroSymbol; } }
        public bool IsOne { get { return this._digit.Symbol == this._numberSystem.OneSymbol; } }
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

        /// <summary>
        /// this will add the symbol to the digits position, and handles rollover by 
        /// lazily loading the NextDigit and incrementing it, recursively
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        internal bool Add(string symbol)
        {
            var rv = this._digit.Add(symbol);
            if (rv && this.IsWiredToParent)
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
            var rv = this._digit.Subtract(symbol);
            if (rv && this.IsWiredToParent)
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
            var rv = this._digit.AddOne();
            if (rv && this.IsWiredToParent)
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
            var rv = this._digit.SubtractOne();
            if (rv && this.IsWiredToParent)
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
}
