using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.DataStructures;
using Arith.DataStructures.Decorations;

namespace Arith.Domain.Numbers
{

    public static class INumericExtensions
    {
        #region Compatibility Stuff
        /// <summary>
        /// returns a symbolic number with a value of 0 in thisNumber's number system
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <returns></returns>
        public static Numeric GetCompatibleZero(this INumeric thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return new Numeric(thisNumber.NumberSystem, thisNumber.NumberSystem.ZeroSymbol);
        }
        /// <summary>
        /// returns a symbolic number with a value of 1 in thisNumber's number system
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <returns></returns>
        public static Numeric GetCompatibleOne(this INumeric thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return new Numeric(thisNumber.NumberSystem, thisNumber.NumberSystem.OneSymbol);
        }
        /// <summary>
        /// returns a symbolic number with the supplied value in thisNumber's number system
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <returns></returns>
        public static Numeric GetCompatibleNumber(this INumeric thisNumber, string number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return new Numeric(thisNumber.NumberSystem, number);
        }
        /// <summary>
        /// returns whether the numerics have the same number system
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool HasCompatibleNumberSystem(this INumeric thisNumber, INumeric number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            if (number == null)
                return false;

            return thisNumber.NumberSystem.IsCompatible(number.NumberSystem);
        }
        #endregion

        #region Compare
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
        public static bool IsGreaterThanOrEqual(this INumeric thisNumber, INumeric number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return !thisNumber.Compare(number).Equals(false);
        }
        public static bool IsLessThan(this INumeric thisNumber, INumeric number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return thisNumber.Compare(number).Equals(false);
        }
        public static bool IsLessThanOrEqual(this INumeric thisNumber, INumeric number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return !thisNumber.Compare(number).Equals(true);
        }
        #endregion


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

            if (!hasTrailingZero)
            {
                sb.Append(numeric.NumberSystem.DecimalSymbol);
                sb.Append(decimals);
            }
            var rv = sb.ToString();
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
        /// <summary>
        /// returns a numeric that are the digits trimmed to(and including) the specified
        /// digit.
        /// </summary>
        /// <param name="numeric"></param>
        /// <param name="digit"></param>
        /// <param name="toMSD"></param>
        /// <returns></returns>
        public static Numeric Trim(this Numeric numeric,
            DigitNode digit, bool toMSD)
        {
            var rv = numeric.GetCompatibleZero();

            numeric.Filter(node =>
            {
                DigitNode newNode =  null;
                if (toMSD)
                {
                    newNode = rv.AddLeastSignificantDigit(node.Value.Symbol);
                }
                else
                {
                    newNode = rv.AddMostSignificantDigit(node.Value.Symbol);
                }

                DigitNode dnode = node as DigitNode;
                if (dnode.IsZerothDigit())
                    rv.ZerothDigit = newNode;

                if (object.ReferenceEquals(digit, node))
                    return true;

                return false;
            }, !toMSD);

            return rv;
        }


    }
}
