using System;
using System.Linq;
using System.Text;
using Arith.DataStructures;
using Arith.Domain.Digits;

namespace Arith.Domain.Numbers
{
    /// <summary>
    /// layer numeric concepts on a linked list of digits
    /// </summary>
    public interface INumeric : ILinkedList<IDigit>
    {
        NumeralSet NumberSystem { get; }
        bool IsPositive { get; }

        /// <summary>
        /// the number in symbolic form
        /// </summary>
        string SymbolsText { get; }
        /// <summary>
        /// sets the value from a string
        /// </summary>
        /// <param name="number"></param>
        void SetValue(string number);
        /// <summary>
        /// sets the value from a numeric.  Note: in any decorations that mutate numeric state
        /// this method should be called last - this will ensure interception of mutation
        /// </summary>
        /// <param name="number"></param>
        void SetValue(INumeric number);
        IDigitNode ZerothDigit { get; }

        /// <summary>
        /// false = this is less, true= this is greater, null = equal
        /// </summary>
        bool? Compare(INumeric number);
        /// <summary>
        /// node by node copy
        /// </summary>
        /// <returns></returns>
        INumeric Clone();
    }

    /// <summary>
    /// extends node interface, applying a digit behaviour
    /// </summary>
    public interface IDigitNode : ILinkedListNode<IDigit>
    {
        void SetValue(string symbol);
        bool Add(string symbol);
        bool Subtract(string symbol);
        bool AddOne();
        bool SubtractOne();
    }



}
