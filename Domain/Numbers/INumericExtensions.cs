﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.DataStructures;
using Arith.DataStructures.Decorations;
using Arith.Domain.Numbers.Decorations;
using Arith.Domain.Digits;
using System.Diagnostics;

namespace Arith.Domain.Numbers
{

    public static class INumericExtensions
    {
        #region Compatibility Stuff
        public static Numeric GetCompatibleEmpty(this INumeric thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            return new Numeric(thisNumber.NumberSystem, null);
        }
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
        public static Numeric GetCompatibleNumber(this INumeric thisNumber,
            string number)
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
        public static bool HasCompatibleNumberSystem(this INumeric thisNumber,
            INumeric number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            if (number == null)
                return false;

            return thisNumber.NumberSystem.IsCompatible(number.NumberSystem);
        }
        #endregion

        #region Inner Numeric
        /// <summary>
        /// finds the inner Numeric instance
        /// </summary>
        /// <param name="thisNumber"></param>
        /// <returns></returns>
        public static Numeric GetInnerNumeric(this INumeric thisNumber)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            if (thisNumber is Numeric)
                return thisNumber as Numeric;

            if (thisNumber is NumericDecorationBase)
                return (thisNumber as NumericDecorationBase).InnerNumeric;

            throw new InvalidOperationException("cannot find inner numeric");
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

            //cannot iterate empty lists
            if (numeric.IsEmpty())
                return;

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
                if (node == null)
                    return;

                while (node != null && node.IsLeastSignificantDigit() == false)
                {
                    preZeroAction(node);
                    node = node.PreviousDigit();
                }
                preZeroAction(lsd);
            }
        }
        /// <summary>
        /// returns a numeric that are the digits trimmed to(and including) the specified
        /// digit.
        /// 
        /// Eg. 123456 trimmed to the 3 "towards most significant digit" == 123
        /// 123456 trimmed to the 3 where toMSD = false == 3456
        /// 123.456 trimmed to the 3 where toMSD = false == 3.456
        /// </summary>
        /// <param name="numeric"></param>
        /// <param name="digit"></param>
        /// <param name="toMSD"></param>
        /// <returns></returns>
        public static Numeric Trim(this INumeric numeric,
            DigitNode digit, bool toMSD)
        {
            var rv = Numeric.New(numeric.NumberSystem, null);
            bool nodeFound = false;

            numeric.Iterate((node) =>
            {
                if (object.ReferenceEquals(node, digit))
                {
                    nodeFound = true;
                }

                if (nodeFound)
                {
                    DigitNode newNode = null;
                    if (toMSD)
                    {
                        newNode = rv.AddMostSignificantDigit(node.Value.Symbol);
                    }
                    else
                    {
                        newNode = rv.AddLeastSignificantDigit(node.Value.Symbol);
                    }
                    DigitNode dNode = node as DigitNode;
                    if (dNode.IsZerothDigit())
                        rv.ZerothDigit = newNode;
                }

            }, toMSD);

            //set zeroth to first if it's not set
            if (rv.ZerothDigit == null)
                rv.ZerothDigit = rv.FirstDigit;

            Debug.WriteLine("trimming number={0} on digit={1} to msd={2} result={3}",
                numeric.SymbolsText,
                digit.Value.Symbol,
                toMSD.ToString(),
                rv.SymbolsText);

            return rv;
        }


    }
}
