using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Arith.DataStructures;
using Arith.Domain;

namespace Arith
{
    /// <summary>
    /// a number represented as a linked list of digits
    /// </summary>
    /// <remarks>
    /// the number 123.45 is represented as this sequence of nodes:
    /// 4,5,3,2,1 with the middle node 3 being set as the ZerothNode 
    /// 
    /// this is done so that the more significant digits are on the end of the list
    /// and the least significant at the start.  In this way we keep a correlation from
    /// ZerothNode moving to the end, as a marker of symbol position
    /// </remarks>
    /// 
    [DebuggerDisplay("{SymbolsText}")]
    public class Number : INumber
    {
        #region Ctor
        public Number(string digits, NumeralSet numberSystem)
        {
            this.SymbolicNumber = new SymbolicNumber(digits, numberSystem);
        }
        #endregion

        #region Properties
        public SymbolicNumber SymbolicNumber { get; private set; }
        #endregion

        #region INumber
        public string SymbolsText
        {
            get { return this.SymbolicNumber.SymbolsText; }
        }

        public void SetValue(string number)
        {
            this.SymbolicNumber.SetValue(number);
        }

        public void Add(string number)
        {
            this.SymbolicNumber.Add(new SymbolicNumber(number, this.NumberSystem));
        }

        public void Subtract(string number)
        {
            this.SymbolicNumber.Subtract(new SymbolicNumber(number, this.NumberSystem));
        }
        public void AddOne()
        {
            this.SymbolicNumber.AddOne();
        }

        public void SubtractOne()
        {
            this.SymbolicNumber.SubtractOne();
        }
        public bool IsPositive
        {
            get { return this.SymbolicNumber.IsPositive; }
        }

        public NumeralSet NumberSystem
        {
            get { return this.SymbolicNumber.NumberSystem; }
        }

        public bool? Compare(string number)
        {
            return this.SymbolicNumber.Compare(new SymbolicNumber(number, this.NumberSystem));
        }
        #endregion
    }


    public class NumberTests
    {
        public static void Test()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }


            //var num = new Number(null, set);
            //var b = num.SymbolsText;

            var num1 = new Number("123456789", set);
            var f = num1.SymbolicNumber.FirstDigit;
            var l = num1.SymbolicNumber.LastDigit;

            Debug.Assert(num1.SymbolsText == "123456789");

            num1.AddOne();
            Debug.Assert(num1.SymbolsText == "123456790");
            var counter = 123456790;
            for (int i = 1; i < 100; i++)
            {
                num1.AddOne();
                counter++;
                Debug.Assert(num1.SymbolsText == counter.ToString());
            }
            for (int i = 1; i < 100; i++)
            {
                num1.SubtractOne();
                counter--;
                Debug.Assert(num1.SymbolsText == counter.ToString());
            }
            var num2 = new Number("0", set);
            counter = 0;
            for (int i = 0; i < 200; i++)
            {
                num2.SubtractOne();
                counter--;
                Debug.Assert(num2.SymbolsText == counter.ToString());
            }
        }

        internal static void TestOperations()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }

            int number = 0;
            var num1 = new Number(number.ToString(), set);
            Debug.Assert(num1.SymbolsText == number.ToString());

            for (int i = 0; i < 1000; i++)
            {
                num1.AddOne();
                number++;
                Debug.Assert(num1.SymbolsText == number.ToString());
            }

            for (int i = 0; i < 1000; i++)
            {
                num1.SubtractOne();
                number--;
                Debug.Assert(num1.SymbolsText == number.ToString());
            }

        }
    }
}
