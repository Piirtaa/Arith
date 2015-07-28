using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Extensions;
using Arith.Domain.Numbers.Decorations;
using Arith.Domain.Digits;

namespace Arith.Domain.Numbers
{
    /// <summary>
    /// has number formatting functions
    /// </summary>
    public static class Formatter
    {
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

        /// <summary>
        /// formats the numeric such that 0 digits and decimal place show as " "
        /// </summary>
        /// <param name="numeric"></param>
        /// <returns></returns>
        public static string GetCarryLineFormat(this INumeric numeric)
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
                    if (node.Value.Symbol.Equals(numeric.NumberSystem.ZeroSymbol))
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(node.Value.Symbol);
                    }
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
                        if (node.Value.Symbol.Equals(numeric.NumberSystem.ZeroSymbol))
                        {
                            sb.Append(" ");
                        }
                        else
                        {
                            sb.Append(node.Value.Symbol);
                        }
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
                    if (node.Value.Symbol.Equals(numeric.NumberSystem.ZeroSymbol))
                    {
                        decimals = " " + decimals;
                    }
                    else
                    {
                        decimals = node.Value.Symbol + decimals;
                    }

                }

            }, true);

            if (!hasTrailingZero)
            {
                sb.Append(" ");
                sb.Append(decimals);
            }
            var rv = sb.ToString();
            return rv;
        }

        /// <summary>
        /// pads numeric format with spaces.  if no format function provided, format
        /// defaults to Simple
        /// </summary>
        /// <param name="thisNumeric"></param>
        /// <param name="maxWholeNumberLength"></param>
        /// <param name="maxDecimalLength"></param>
        /// <returns></returns>
        public static string DecimalAlignFormat(this INumeric thisNumeric,
      Numeric maxWholeNumberLength,
            Numeric maxDecimalLength,
            Func<INumeric, string> format = null
        )
        {
            if (thisNumeric == null)
                throw new ArgumentNullException("thisNumeric");

            Func<INumeric, string> actualFormat = (x) =>
            {
                return thisNumeric.GetSimpleFormat();
            };

            if (format != null)
                actualFormat = format; 

            string val = actualFormat(thisNumeric);

            Numeric eachWholeNumberLength = null;
            Numeric eachDecimalLength = null;
            thisNumeric.GetNumericLengths(out eachWholeNumberLength, out eachDecimalLength);

            var postPadLength = maxWholeNumberLength.Clone().HasAddition();
            postPadLength.Subtract(eachWholeNumberLength);

            var prePadLength = maxDecimalLength.Clone().HasAddition();
            prePadLength.Subtract(eachDecimalLength);

            postPadLength.PerformThisManyTimes(x =>
            {
                val = " " + val;
            });

            prePadLength.PerformThisManyTimes(x =>
            {
                val = val + " ";
            });
            return val;
        }

        /// <summary>
        /// for a given list of numbers gets the longest lengths
        /// </summary>
        /// <param name="thisList"></param>
        /// <param name="wholeNumberLength"></param>
        /// <param name="decimalLength"></param>
        public static void GetNumericListLengths(this List<INumeric> thisList,
    out Numeric wholeNumberLength,
    out Numeric decimalLength)
        {
            if (thisList == null)
                throw new ArgumentNullException("thisList");

            Numeric maxWholeNumberLength = null;
            Numeric maxDecimalLength = null;

            thisList.WithEach(num =>
            {
                Numeric eachWholeNumberLength = null;
                Numeric eachDecimalLength = null;
                num.GetNumericLengths(out eachWholeNumberLength, out eachDecimalLength);

                if (eachWholeNumberLength.IsGreaterThan(maxWholeNumberLength))
                    maxWholeNumberLength = eachWholeNumberLength;

                if (eachDecimalLength.IsGreaterThan(maxDecimalLength))
                    maxDecimalLength = eachDecimalLength;

            });


            wholeNumberLength = maxWholeNumberLength;
            decimalLength = maxDecimalLength;
        }
    }
}
