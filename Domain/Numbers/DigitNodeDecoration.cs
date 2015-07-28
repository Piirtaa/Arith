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
    public class DigitNodeDecoration : LinkedListNodeDecorationBase<IDigit>, IDigitNode
    {
        #region Ctor
        public DigitNodeDecoration(object decorated)
            : base(decorated)
        {
        }
        #endregion

        #region Static Builders
        public static DigitNodeDecoration New(object decorated)
        {
            return new DigitNodeDecoration(decorated);
        }
        #endregion

        #region Parent Number-related Calculated Properties
        private NumeralSet NumberSystem { get { return this.ParentNumeric().NumberSystem; } }
        #endregion

        #region Calculated Properties
        public string Symbol { get { return this.Value.Symbol; } }
        #endregion

        #region IDigitNode
        public void SetValue(string symbol)
        {
            this.Value.SetValue(symbol);
        }
        /// <summary>
        /// this will add the symbol to the digits position, and handles rollover by 
        /// lazily loading the NextDigit and incrementing it, recursively
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool Add(string symbol)
        {
            var rv = this.Value.Add(symbol);
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
            var rv = this.Value.Subtract(symbol);
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
            var rv = this.Value.AddOne();
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
            var rv = this.Value.SubtractOne();
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
            return new DigitNodeDecoration(thing);
        }
        #endregion
    }

    public static class DigitNodeDecorationExtensions
    {
        public static DigitNodeDecoration HasDigits(this object number)
        {
            var decoration = number.ApplyDecorationIfNotPresent<DigitNodeDecoration>(x =>
            {
                return DigitNodeDecoration.New(number);
            });

            return decoration;
        }
    }
}
