using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Arith.Domain.Digits;
using Arith.DataStructures;
using Arith.DataStructures.Decorations;
using Arith.Decorating;

namespace Arith.Domain.Numbers
{
    /// <summary>
    /// digit node decorates linkedlistnode, adding numeric semantics and behaviour
    /// towards a parent numeric
    /// </summary>
    [DebuggerDisplay("{Symbol}")]
    public class DigitNodeDecoration : LinkedListNodeDecorationBase<IDigit>,
        IDigitNode
    {
        #region Ctor
        public DigitNodeDecoration(object decorated,
            string decorationName = null)
            : base(decorated, decorationName)
        {
        }
        #endregion

        #region Static Builders
        public static DigitNodeDecoration New(object decorated,
            string decorationName = null)
        {
            return new DigitNodeDecoration(decorated, decorationName);
        }
        #endregion

        #region Parent Number-related Calculated Properties
        private NumeralSet NumberSystem { get { return this.ParentNumeric().NumberSystem; } }
        #endregion

        #region Calculated Properties
        public string Symbol { get { return this.NodeValue.Symbol; } }
        #endregion

        #region IDigitNode
        public void SetValue(string symbol)
        {
            this.NodeValue.SetValue(symbol);
        }
        /// <summary>
        /// this will add the symbol to the digits position, and handles rollover by 
        /// lazily loading the NextDigit and incrementing it, recursively
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool Add(string symbol)
        {
            var rv = this.NodeValue.Add(symbol);
            if (rv)
            {
                this.LoadNextDigit().AddOne();
            }
            return rv;
        }
        /// <summary>
        /// this will subtract the symbol to the digits position, and handles rollover by
        /// getting the NextDigit and subtracting it, recursively
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool Subtract(string symbol)
        {
            var rv = this.NodeValue.Subtract(symbol);
            if (rv)
            {
                //if we are in a position where NextDigit does not exist, then we throw 
                //an exception.  the design of the number system is such that we should
                //never exhaust a registry
                if (!this.HasNextDigit())
                    throw new InvalidOperationException("unexpected sign change");

                this.LoadNextDigit().SubtractOne();
            }
            return rv;
        }
        /// <summary>
        /// this will the one symbol to the digits position, and handles rollover by 
        /// lazily loading the NextDigit and incrementing it, recursively
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool AddOne()
        {
            var rv = this.NodeValue.AddOne();
            if (rv)
            {
                this.LoadNextDigit().AddOne();
            }
            return rv;
        }
        /// <summary>
        /// this will subtract the symbol to the digits position, and handles rollover by
        /// getting the NextDigit and subtracting it, recursively
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool SubtractOne()
        {
            var rv = this.NodeValue.SubtractOne();
            if (rv)
            {
                //if we are in a position where NextDigit does not exist, then we throw 
                //an exception.  the design of the number system is such that we should
                //never exhaust a registry
                if (!this.HasNextDigit())
                    throw new InvalidOperationException("unexpected sign change");

                this.LoadNextDigit().SubtractOne();
            }
            return rv;
        }
        #endregion

        #region Overrides
        public override IDecoration ApplyThisDecorationTo(object thing)
        {
            return new DigitNodeDecoration(thing, this.DecorationName);
        }
        #endregion
    }

    public static class DigitNodeDecorationExtensions
    {
        public static DigitNodeDecoration HasDigits(this object number,
            string decorationName = null)
        {
            return DigitNodeDecoration.New(number, decorationName);
        }
    }
}
