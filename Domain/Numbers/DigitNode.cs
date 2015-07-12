using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Arith.Domain.Digits;
using Arith.DataStructures;

namespace Arith.Domain.Numbers
{
    /// <summary>
    /// digit node extends linkedlistnode, adding numeric semantics and behaviour
    /// towards a parent numeric
    /// </summary>
    [DebuggerDisplay("{Symbol}")]
    public class DigitNode : LinkedListNode<IDigit>, IDigitNode
    {
        #region Ctor
        public DigitNode(IDigit value, ILinkedList<IDigit> parentList)
            : base(value, parentList)
        {
        }
        #endregion

        #region Parent Number-related Calculated Properties
        private NumeralSet NumberSystem { get { return this.ParentNumeric().NumberSystem; } }

        /// <summary>
        /// when queried will perform a lazy load of the next digit (ie. expand the registers)
        /// </summary>
        internal DigitNode LoadNextDigit
        {
            get
            {
                if (this.NextNode == null)
                {
                    return this.ParentNumeric().AddMostSignificantZeroDigit() as DigitNode;
                }
                return this.NextNode as DigitNode;
            }
        }

        /// <summary>
        /// when queried will perform a lazy load of the previous digit (ie. expand the registers)
        /// </summary>
        internal DigitNode LoadPreviousDigit
        {
            get
            {
                if (this.PreviousNode == null)
                {
                    return this.ParentNumeric().AddLeastSignificantZeroDigit() as DigitNode;
                }
                return this.PreviousNode as DigitNode;
            }
        }
        #endregion

        #region Calculated Properties
        public string Symbol { get { return this.Value.Symbol; } }
        #endregion

        #region Methods
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
                this.LoadNextDigit.AddOne();
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
                if (!this.HasNextDigit())
                    throw new InvalidOperationException("unexpected sign change");

                this.LoadNextDigit.SubtractOne();
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
                this.LoadNextDigit.AddOne();
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
                if (!this.HasNextDigit())
                    throw new InvalidOperationException("unexpected sign change");

                this.LoadNextDigit.SubtractOne();
            }
            return rv;
        }
        #endregion
    }
}
