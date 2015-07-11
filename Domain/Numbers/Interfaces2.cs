using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Domain.Digits;

namespace Arith.Domain.Numbers
{
    //public interface ISymbolicNumber
    //{
    //    /// <summary>
    //    /// the number in symbolic form
    //    /// </summary>
    //    string SymbolsText { get; }
    //    void SetValue(string number);
    //    bool IsPositive { get; }
    //    NumeralSet NumberSystem { get; }
    //    /// <summary>
    //    /// false = this is less, true= this is greater, null = equal
    //    /// </summary>
    //    bool? Compare(ISymbolicNumber number);
    //    void Add(ISymbolicNumber number);
    //    void Subtract(ISymbolicNumber number);



    //}

    //public static class SymbolicNumberExtensions
    //{
    //    public static bool IsEqualTo(this ISymbolicNumber thisNumber, ISymbolicNumber number)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        return thisNumber.Compare(number).Equals(null);
    //    }
    //    public static bool IsGreaterThan(this ISymbolicNumber thisNumber, ISymbolicNumber number)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        return thisNumber.Compare(number).Equals(true);
    //    }
    //    public static bool IsLessThan(this ISymbolicNumber thisNumber, ISymbolicNumber number)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        return thisNumber.Compare(number).Equals(false);
    //    }
    //    public static void AddOne(this ISymbolicNumber thisNumber)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        thisNumber.Add(new SymbolicNumber(thisNumber.NumberSystem.OneSymbol, thisNumber.NumberSystem));
    //    }
    //    public static void SubtractOne(this ISymbolicNumber thisNumber)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        thisNumber.Subtract(new SymbolicNumber(thisNumber.NumberSystem.OneSymbol, thisNumber.NumberSystem));
    //    }

    //    /// <summary>
    //    /// performs the action for as many times as the number
    //    /// </summary>
    //    /// <param name="number"></param>
    //    /// <param name="action"></param>
    //    public static void CountdownToZero(this ISymbolicNumber number, Action<ISymbolicNumber> action)
    //    {
    //        if (number == null) return;
    //        if (action == null) throw new ArgumentNullException("action");

    //        if (!number.IsPositive)
    //            throw new ArgumentOutOfRangeException("number must be positive");

    //        var zero = new SymbolicNumber(number.NumberSystem.ZeroSymbol, number.NumberSystem);
            
    //        var num = (number as SymbolicNumber).Clone();
    //        while (num.IsGreaterThan(zero))
    //        {
    //            action(num);
    //            num.SubtractOne();
    //        }
    //    }

    //    /// <summary>
    //    /// fluent
    //    /// </summary>
    //    /// <param name="thisNumber"></param>
    //    /// <param name="numShifts"></param>
    //    /// <returns></returns>
    //    public static SymbolicNumber ShiftLeft(this SymbolicNumber thisNumber, SymbolicNumber numShifts)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        if (numShifts == null)
    //            throw new ArgumentNullException("numShifts");

    //        numShifts.CountdownToZero(c =>
    //        {
    //            thisNumber.ShiftLeft();
    //        });
    //        return thisNumber;
    //    }
    //    /// <summary>
    //    /// fluent
    //    /// </summary>
    //    /// <param name="thisNumber"></param>
    //    /// <param name="numShifts"></param>
    //    /// <returns></returns>
    //    public static SymbolicNumber ShiftRight(this SymbolicNumber thisNumber, SymbolicNumber numShifts)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        if (numShifts == null)
    //            throw new ArgumentNullException("numShifts");

    //        numShifts.CountdownToZero(c =>
    //        {
    //            thisNumber.ShiftRight();
    //        });
    //        return thisNumber;
    //    }
    //    /// <summary>
    //    /// shifts the number so that it has no decimal digits.  returns the shift length. 
    //    /// always a right shift. 
    //    /// </summary>
    //    /// <param name="thisNumber"></param>
    //    /// <returns></returns>
    //    public static SymbolicNumber ShiftToZero(this SymbolicNumber thisNumber)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        SymbolicNumber counter = new SymbolicNumber(thisNumber.NumberSystem.ZeroSymbol, thisNumber.NumberSystem);
    //        while (thisNumber.ZerothDigit.HasPreviousDigit)
    //        {
    //            thisNumber.ShiftRight();
    //            counter.AddOne();
    //        }
    //        return counter;
    //    }
    //    public static SymbolicNumber GetDecimalPlaces(this SymbolicNumber thisNumber)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");


    //        var places = thisNumber.ShiftToZero();
    //        thisNumber.ShiftLeft(places);

    //        return places;
    //    }
    //    public static void TruncateToDecimalPlaces(this SymbolicNumber thisNumber, 
    //        SymbolicNumber places)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        if (places == null)
    //            throw new ArgumentNullException("places");

    //        var currPlaces = thisNumber.GetDecimalPlaces();
    //        if (currPlaces.IsGreaterThan(places))
    //        {
    //            currPlaces.Subtract(places);

    //            currPlaces.CountdownToZero(x =>
    //            {
    //                thisNumber.Remove(thisNumber.LastNode);
    //            });
    //        }
    //    }
    //    /// <summary>
    //    /// returns a symbolic number with a value of 0 in thisNumber's number system
    //    /// </summary>
    //    /// <param name="thisNumber"></param>
    //    /// <returns></returns>
    //    public static SymbolicNumber GetCompatibleZero(this ISymbolicNumber thisNumber)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        return new SymbolicNumber(thisNumber.NumberSystem.ZeroSymbol, thisNumber.NumberSystem);
    //    }
    //    /// <summary>
    //    /// returns a symbolic number with a value of 1 in thisNumber's number system
    //    /// </summary>
    //    /// <param name="thisNumber"></param>
    //    /// <returns></returns>
    //    public static SymbolicNumber GetCompatibleOne(this ISymbolicNumber thisNumber)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        return new SymbolicNumber(thisNumber.NumberSystem.OneSymbol, thisNumber.NumberSystem);
    //    }
    //    /// <summary>
    //    /// returns a symbolic number with the supplied value in thisNumber's number system
    //    /// </summary>
    //    /// <param name="thisNumber"></param>
    //    /// <returns></returns>
    //    public static SymbolicNumber GetCompatibleNumber(this ISymbolicNumber thisNumber, string number)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        return new SymbolicNumber(number, thisNumber.NumberSystem);
    //    }
        
    //    public static SymbolicNumber Clone(this SymbolicNumber thisNumber)
    //    {
    //        if (thisNumber == null)
    //            throw new ArgumentNullException("thisNumber");

    //        return SymbolicNumber.Clone(thisNumber);
    //    }
    //}
}
