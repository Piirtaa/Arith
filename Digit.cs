using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arith
{
    /// <summary>
    /// a digit contains a circular number value, the numeral, and itself is aggregated
    /// in a Number linked list.
    /// </summary>
    public class Digit
    {
        #region Declarations
        private CircularLinkedListNode<string> _numeral = null;

        /// <summary>
        /// the symbol that when "added" to the current symbol produces the last symbol in the set
        /// </summary>
        private CircularLinkedListNode<string> _complementNumeral = null; 
        #endregion

        #region Ctor
        public Digit(CircularLinkedListNode<string> numeral)
        {
            if (numeral == null)
                throw new ArgumentNullException("numeral");
            this._numeral = numeral;

            this._complementNumeral = numeral.GetListComplement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// the current numeral symbol
        /// </summary>
        public string Symbol { get { return this._numeral.Value; } }
        /// <summary>
        /// basically unusued placeholder value to keep the number symbols for position of this digit
        /// using the same number system
        /// </summary>
        public string PositionNumber { get; set; }


        #endregion

        #region Hidden Properties
        /// <summary>
        /// for the numeral set this digit belongs to, returns the zeroth(first) numeral 
        /// </summary>
        protected CircularLinkedListNode<string> ZeroNumeral { get { return this._numeral.ParentList.FirstNode as CircularLinkedListNode<string>; } }

        protected internal LinkedListNode<Digit> ParentNode { get; set; }

        protected Number ParentNumber
        {
            get
            {
                return this.ParentNode.ParentList as Number;
            }
        }
        #endregion

        #region Calculated Properties
        /// <summary>
        /// is it the Zero Numeral
        /// </summary>
        public bool IsZero
        {
            get
            {
                return this._numeral.IsFirst;
            }
        }
        public bool IsZerothDigit
        {
            get
            {
                return object.ReferenceEquals(this.ParentNode, this.ParentNumber.ZerothDigit);
            }
        }
        public bool IsMostSignificantDigit
        {
            get
            {
                return object.ReferenceEquals(this.ParentNode, this.ParentNumber.GetMostSignificantDigit());
            }
        }
        public bool IsLeastSignificantDigit
        {
            get
            {
                return object.ReferenceEquals(this.ParentNode, this.ParentNumber.FirstNode);
            }
        }
        /// <summary>
        /// when queried will perform a lazy load of the next digit (ie. expand the registers)
        /// </summary>
        public Digit NextDigit
        {
            get
            {
                if (this.ParentNode == null)
                    return null;

                if (this.ParentNode.NextNode == null)
                {
                    return this.ParentNumber.AddEmptyDigit().Value;
                }
                return this.ParentNode.NextNode.Value;
            }
        }
        /// <summary>
        /// when queried will perform a lazy load of the previous digit (ie. expand the registers)
        /// </summary>
        public Digit PreviousDigit
        {
            get
            {
                if (this.ParentNode == null)
                    return null;

                if (this.ParentNode.PreviousNode == null)
                {
                    return this.ParentNumber.AddEmptyDecimalDigit().Value;
                }
                return this.ParentNode.PreviousNode.Value;
            }
        }
        #endregion

        #region Comparators
        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        /// <param name="digit"></param>
        /// <returns></returns>
        public bool? Compare(Digit digit)
        {
            if (digit == null)
                throw new ArgumentNullException("digit");

            if(digit.Symbol.Equals(this.Symbol))
                return null;

            var numeral = this.ZeroNumeral;
            
            //find which digit comes first thru simple iteration
            while(numeral != null)
            {
                if(numeral.Value.Equals(this.Symbol))
                    return true;

                if(numeral.Value.Equals(digit.Symbol))
                    return false;

                if(numeral.IsLast)
                    throw new InvalidOperationException("bad digit compare");

                numeral = numeral.NextNode as CircularLinkedListNode<string>;
            }
            return null;
        }
        public bool IsEqualTo(Digit digit)
        {
            return this.Compare(digit).Equals(null);
        }
        public bool IsGreaterThan(Digit digit)
        {
            return this.Compare(digit).Equals(true);
        }
        public bool IsLessThan(Digit digit)
        {
            return this.Compare(digit).Equals(false);
        }
        #endregion

        #region Methods
        public bool SetValue(string numeral)
        {
            this._numeral = this._numeral.FindNodeByValue(numeral);
            return true;
        }
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

        * 
        */

        /// <summary>
        /// moves to the next digit.  if currently on the last digit, returns true for rollover
        /// </summary>
        /// <param name="rollover"></param>
        /// <returns></returns>
        internal bool MoveForward()
        {
            var rv = this._numeral.MoveForward(out _numeral);
            if (rv && this.NextDigit != null)
            {
                this.NextDigit.MoveForward();
            }
            return rv;
        }

        /// <summary>
        /// moves to the last digit.  if currently on the first digit, returns true for rollover
        /// </summary>
        /// <param name="rollover"></param>
        /// <returns></returns>
        internal bool MoveBack()
        {

            var rv = this._numeral.MoveBack(out _numeral);
            if (rv && this.NextDigit != null)
            {
                this.NextDigit.MoveBack();
            }
            return rv;
        }
        internal bool MoveForwardBy(string val)
        {
            var rv = this._numeral.MoveForwardBy(val, out _numeral);
            if (rv && this.NextDigit != null)
                this.NextDigit.MoveForward();
            return rv;
        }
        internal bool MoveBackBy(string val)
        {
            var rv = this._numeral.MoveBackBy(val, out _numeral);
            if (rv && this.NextDigit != null)
                this.NextDigit.MoveBack();
            return rv;
        }
        #endregion
    }
}
