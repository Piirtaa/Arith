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
        void SetValue(string number);

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
    /// extends node interface, upon which some extensions can work
    /// </summary>
    public interface IDigitNode : ILinkedListNode<IDigit>
    {
    }

    public static class IDigitNodeExtensions
    {
        public static Numeric ParentNumber(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return digitNode.ParentList as Numeric;
        }
        /// <summary>
        /// does this digit have a value of zero?
        /// </summary>
        /// <param name="digitNode"></param>
        /// <returns></returns>
        public static bool IsZero(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return digitNode.Value.IsEqualTo(digitNode.ParentNumber().NumberSystem.ZeroSymbol);
        }
        /// <summary>
        /// does this digit have a value of one?
        /// </summary>
        /// <param name="digitNode"></param>
        /// <returns></returns>
        public static bool IsOne(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return digitNode.Value.IsEqualTo(digitNode.ParentNumber().NumberSystem.OneSymbol);
        }

        /// <summary>
        /// reference compares Parent's Zeroth Digit to this instance 
        /// </summary>
        public static bool IsZerothDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return object.ReferenceEquals(digitNode, digitNode.ParentNumber().ZerothDigit);

        }
        /// <summary>
        /// reference compares Parent's MSD Digit to this instance 
        /// </summary>
        public static bool IsMostSignificantDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return object.ReferenceEquals(digitNode, digitNode.ParentNumber().GetMostSignificantDigit());
        }
        /// <summary>
        /// reference compares Parent's LSD Digit to this instance 
        /// </summary>
        public static bool IsLeastSignificantDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return object.ReferenceEquals(digitNode, digitNode.ParentNumber().FirstNode);
        }
        public static bool HasNextDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return digitNode.NextNode != null;
        }
        public static bool HasPreviousDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return digitNode.PreviousNode != null;
        }
        public static IDigitNode NextDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return digitNode.NextNode as IDigitNode;
        }
        public static IDigitNode PreviousDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return digitNode.PreviousNode as IDigitNode;
        }
    }

    public static class INumericExtensions
    {
        public static bool IsEqualTo(this INumeric thisNumber, INumeric number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return thisNumber.Compare(number).Equals(null);
        }
        public static bool IsGreaterThan(this INumeric thisNumber, INumeric number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return thisNumber.Compare(number).Equals(true);
        }
        public static bool IsLessThan(this INumeric thisNumber, INumeric number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return thisNumber.Compare(number).Equals(false);
        }
        /// <summary>
        /// formats the numeric as a string
        /// </summary>
        /// <param name="numeric"></param>
        /// <returns></returns>
        public static string GetSimpleFormat(this INumeric numeric)
        {
            if (numeric == null)
                throw new ArgumentNullException("numeric");

            StringBuilder sb = new StringBuilder();

            if (!numeric.IsPositive)
                sb.Append(numeric.NumberSystem.NegativeSymbol);

            string decimals = string.Empty;

            bool hasLeadingZero = true;
            bool hasTrailingZero = true;
            numeric.ZoneIterate((dig) =>
            {
                IDigitNode node = dig as IDigitNode;

                if (node.IsZerothDigit())
                {
                    //always include the zeroth digit
                    sb.Append(node.Value.Symbol);
                }
                else
                {
                    if (hasLeadingZero && dig.IsZero())
                    {
                        //skip
                    }
                    else
                    {
                        hasLeadingZero = false;
                        sb.Append(node.Value.Symbol);
                    }
                }
            },
            (dig) =>
            {
                IDigitNode node = dig as IDigitNode;

                if (hasTrailingZero && dig.IsZero())
                {
                    //skip
                }
                else
                {
                    hasTrailingZero = false;
                    decimals = node.Value.Symbol + decimals;
                }

            }, true);

            var rv = sb.ToString();
            if (!hasTrailingZero)
            {
                sb.Append(numeric.NumberSystem.DecimalSymbol);
                sb.Append(decimals);
            }

            return rv;
        }
        public static IDigitNode LeastSignificantDigit(this INumeric numeric)
        {
            if (numeric == null)
                throw new ArgumentNullException("numeric");

            return numeric.FirstNode as IDigitNode;
        }

        public static IDigitNode MostSignificantDigit(this INumeric numeric)
        {
            if (numeric == null)
                throw new ArgumentNullException("numeric");

            return numeric.LastNode as IDigitNode;
        }

        /// <summary>
        /// has 2 zones to iterate - from zeroth to MSD, and zoroth.Previous to LSD.
        /// postZero and preZero actions occur on each item in their respective zones.
        /// towards zero indicates if we are moving towards the zero point, or originating
        /// iteration from it, outwards to MSD and LSD.
        /// </summary>
        /// <param name="postZeroAction"></param>
        /// <param name="preZeroAction"></param>
        public static void ZoneIterate(this INumeric numeric,
            Action<IDigitNode> postZeroAction,
            Action<IDigitNode> preZeroAction,
            bool towardsZero = true)
        {
            if (numeric == null)
                throw new ArgumentNullException("numeric");

            //the add process

            var zero = numeric.ZerothDigit;
            var lsd = numeric.LeastSignificantDigit();
            var msd = numeric.MostSignificantDigit();

            if (towardsZero)
            {
                var node = msd;
                while (node != null && node.IsZerothDigit() == false)
                {
                    postZeroAction(node);
                    node = node.PreviousDigit();
                }
                postZeroAction(zero);
                node = lsd;
                while (node != null && node.IsZerothDigit() == false)
                {
                    preZeroAction(node);
                    node = node.NextDigit();
                }
            }
            else
            {
                var node = zero;
                while (node != null && node.IsMostSignificantDigit() == false)
                {
                    postZeroAction(node);
                    node = node.NextDigit();
                }
                postZeroAction(msd);
                node = zero.PreviousDigit();
                while (node != null && node.IsLeastSignificantDigit() == false)
                {
                    preZeroAction(node);
                    node = node.NextDigit();
                }
                preZeroAction(lsd);
            }
        }

    }
}
