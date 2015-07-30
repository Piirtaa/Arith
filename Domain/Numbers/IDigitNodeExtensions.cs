using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Domain.Digits;

namespace Arith.Domain.Numbers
{
    public static class IDigitNodeExtensions
    {
        /// <summary>
        /// returns the numeric that this digit is a part of
        /// </summary>
        /// <param name="digitNode"></param>
        /// <returns></returns>
        public static Numeric ParentNumeric(this IDigitNode digitNode)
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

            return digitNode.NodeValue.IsEqualTo(digitNode.ParentNumeric().NumberSystem.ZeroSymbol);
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

            return digitNode.NodeValue.IsEqualTo(digitNode.ParentNumeric().NumberSystem.OneSymbol);
        }

        /// <summary>
        /// reference compares Parent's Zeroth Digit to this instance 
        /// </summary>
        public static bool IsZerothDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return object.ReferenceEquals(digitNode, digitNode.ParentNumeric().ZerothDigit);
        }
        /// <summary>
        /// reference compares Parent's MSD Digit to this instance 
        /// </summary>
        public static bool IsMostSignificantDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return object.ReferenceEquals(digitNode, digitNode.ParentNumeric().MostSignificantDigit());
        }
        /// <summary>
        /// reference compares Parent's LSD Digit to this instance 
        /// </summary>
        public static bool IsLeastSignificantDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            return object.ReferenceEquals(digitNode, digitNode.ParentNumeric().LeastSignificantDigit());
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

        /// <summary>
        /// when queried will perform a lazy load of the next digit (ie. expand the registers)
        /// </summary>
        internal static IDigitNode LoadNextDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            if (!digitNode.HasNextDigit())
            {
                return digitNode.ParentNumeric().AddMostSignificantZeroDigit();
            }
            return digitNode.NextDigit();
        }

        /// <summary>
        /// when queried will perform a lazy load of the previous digit (ie. expand the registers)
        /// </summary>
        internal static IDigitNode LoadPreviousDigit(this IDigitNode digitNode)
        {
            if (digitNode == null)
                throw new ArgumentNullException("digitNode");

            if (!digitNode.HasPreviousDigit())
            {
                return digitNode.ParentNumeric().AddLeastSignificantZeroDigit();
            }
            return digitNode.PreviousDigit();

        }
    }
}
