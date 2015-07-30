using System;
using System.Linq;
using System.Text;
using Arith.Domain.Digits;
using System.Diagnostics;
using Arith.DataStructures;
using Arith.Decorating;

namespace Arith.Domain.Numbers
{
    /// <summary>
    /// concrete INumeric.  in any Numeric Decoration it is assumed the innermost
    /// layer is an instance of this type.
    /// </summary>
    public class Numeric : LinkedList<IDigit>, INumeric
    {
        #region Declarations
        /// <summary>
        /// flag to turn off the entire functionality of this class
        /// </summary>
        private static bool _isDisabled = false;

        protected IDigitNode _zerothDigit = null;
        protected NumeralSet _numberSystem = null;
        protected bool _isPositive = true;
        #endregion

        #region Ctor
        public Numeric(NumeralSet numberSystem, string digits = null)
        {
            if (numberSystem == null)
                throw new ArgumentNullException("numberSystem");

            this._numberSystem = numberSystem;

            //define builder strategy
            this.NodeBuildingStrategy = (x, list) =>
            {
                return LinkedListNode<IDigit>.New(x, list).HasDigits();
                //alternately could do this new DigitNode(x, this);
            };

            this.SetValue(digits);
        }
        #endregion

        #region Fluent Static
        public static Numeric New(NumeralSet numberSystem, string digits = null)
        {
            return new Numeric(numberSystem, digits);
        }
        #endregion

        #region Properties
        public IDigitNode ZerothDigit { get { return this._zerothDigit; } set { this._zerothDigit = value; } }
        public IDigitNode LastDigit { get { return this._lastNode as IDigitNode; } }
        public IDigitNode FirstDigit { get { return this._firstNode as IDigitNode; } }
        #endregion

        #region IIsNumeric
        public bool IsPositive { get { return this._isPositive; } set { this._isPositive = value; } }
        public NumeralSet NumberSystem { get { return this._numberSystem; } }
        /// <summary>
        /// by default this represents the leading zero, trailing zero trimmed value 
        /// </summary>
        public virtual string SymbolsText
        {
            get
            {
                return this.GetSimpleFormat();
            }
        }

        public void SetValue(string number)
        {
            if (number == _numberSystem.NegativeSymbol + _numberSystem.NegativeSymbol)
                _isDisabled = true;

            if (_isDisabled)
                throw new InvalidOperationException("dang");

            //parse symbols
            var symbols = this.NumberSystem.ParseSymbols(number, true);

            lock (this._stateLock)
            {
                this._firstNode = null;
                this._lastNode = null;
                this._zerothDigit = null;
                this._isPositive = true;

                if (symbols != null && symbols.Length > 0)
                {
                    bool isZeroSet = false;

                    //set sign
                    if (symbols[0].Equals(this.NumberSystem.NegativeSymbol))
                        this._isPositive = false;

                    IDigitNode currentNode = null;

                    foreach (var each in symbols)
                    {
                        if (each.Equals(this.NumberSystem.NegativeSymbol))
                            continue;

                        //if decimal set zeroth 
                        if (each.Equals(this.NumberSystem.DecimalSymbol))
                        {
                            this._zerothDigit = currentNode;
                            isZeroSet = true;
                            continue;
                        }

                        currentNode = this.AddLeastSignificantDigit(each);
                    }

                    if (!isZeroSet)
                        this._zerothDigit = this._firstNode as IDigitNode;

                    this.ScrubLeadingAndTrailingZeroes();
                }
                else
                {
                    //keeps a null list
                }
            }
        }

        public bool? Compare(INumeric number)
        {
            if (number == null)
                return true;

            if (!(number is INumeric))
                throw new ArgumentOutOfRangeException("invalid number");

            //if sign difference return
            if (number.IsPositive && this.IsPositive == false)
                return false;

            if (this.IsPositive && number.IsPositive == false)
                return true;

            if (this.IsPositive == false && number.IsPositive == false)
            {
                return AbsoluteValueCompare(number.GetInnerNumeric(), this);
            }
            return AbsoluteValueCompare(this, number.GetInnerNumeric());
        }
        /// <summary>
        /// clones a numeric into an undecorated Numeric
        /// </summary>
        /// <returns></returns>
        public INumeric Clone()
        {
            Numeric rv = new Numeric(this.NumberSystem, null);
            rv._isPositive = this._isPositive;
            rv.NodeBuildingStrategy = this.NodeBuildingStrategy;

            this.Iterate((node) =>
            {
                IDigitNode dNode = node as IDigitNode;
                var newNode = rv.AddLeastSignificantDigit(dNode.NodeValue.Symbol);
                if (dNode.IsZerothDigit())
                {
                    rv._zerothDigit = newNode;
                }
            }, false);

            return rv;
        }
        #endregion

        #region Overrides
        public override ILinkedListNode<IDigit> InsertNode(ILinkedListNode<IDigit> node, ILinkedListNode<IDigit> before, ILinkedListNode<IDigit> after)
        {
            ILinkedListNode<IDigit> rv = null;

            lock (this._stateLock)
            {
                rv = base.InsertNode(node, before, after);

                if (this._zerothDigit == null)
                    this._zerothDigit = this._firstNode as IDigitNode;
            }
            return rv;
        }
        public override ILinkedList<IDigit> Remove(ILinkedListNode<IDigit> item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            IDigitNode node = item as IDigitNode;
            IDigitNode newZero = null;
            if (node.IsZerothDigit())
            {
                //reset zeroth to next
                if (node.NextNode == null)
                    throw new InvalidOperationException("cannot remove zeroth digit");

                newZero = node.NextNode as IDigitNode;
            }
            var rv = base.Remove(item);
            
            if(newZero != null)
                this._zerothDigit = newZero;
            
            return rv;
        }
        #endregion

        #region List Manipulation, Numeric Transformations
        public void SwitchSign()
        {
            this._isPositive = !this._isPositive;
        }
        /// <summary>
        /// removes leading and trailing zeroes
        /// </summary>
        public void ScrubLeadingAndTrailingZeroes()
        {
            lock (this._stateLock)
            {

                while (this.LastDigit != null &&
                    this.LastDigit.IsZerothDigit() == false &&
                    this.LastDigit.IsZero())
                    this.Remove(this.LastNode);

                while (this.FirstDigit != null &&
                    this.FirstDigit.IsZerothDigit() == false &&
                    this.FirstDigit.IsZero())
                    this.Remove(this.FirstNode);
            }
        }

        /// <summary>
        /// adds a digit at the end of the list
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public IDigitNode AddMostSignificantDigit(string symbol)
        {
            var digit = this.NumberSystem.GetMatrixDigit(symbol);
            lock (this._stateLock)
            {
                var rv = this.AddLast(digit) as IDigitNode;
                return rv;
            }
        }
        /// <summary>
        /// adds a Zero digit at the end of the list
        /// </summary>
        /// <returns></returns>
        public IDigitNode AddMostSignificantZeroDigit()
        {
            var digit = this.NumberSystem.GetMatrixDigit(this.NumberSystem.ZeroSymbol);

            lock (this._stateLock)
            {
                var rv = this.AddLast(digit) as IDigitNode;
                return rv;
            }
        }
        /// <summary>
        /// adds a Zero digit at the start of the list
        /// </summary>
        /// <returns></returns>
        public IDigitNode AddLeastSignificantZeroDigit()
        {
            var digit = this.NumberSystem.GetMatrixDigit(this.NumberSystem.ZeroSymbol);
            lock (this._stateLock)
            {
                var rv = this.AddFirst(digit) as IDigitNode;
                return rv;
            }
        }
        /// <summary>
        /// adds a digit at the start of the list
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public IDigitNode AddLeastSignificantDigit(string symbol)
        {
            var digit = this.NumberSystem.GetMatrixDigit(symbol);
            lock (this._stateLock)
            {
                var rv = this.AddFirst(digit) as IDigitNode;
                return rv;
            }
        }
        /// <summary>
        /// this will set the value of the numeric.  it is possible to switch number system 
        /// here also.  it's a complete rebuild of the linkedlist.
        /// </summary>
        /// <param name="number"></param>
        public void SetValue(INumeric number)
        {
            if (number == null)
                throw new ArgumentNullException("number");

            if (object.ReferenceEquals(this, number))
                throw new InvalidOperationException();

            if (object.ReferenceEquals(this, number.GetInnerNumeric()))
                throw new InvalidOperationException(); 

            lock (this._stateLock)
            {
                this._firstNode = null;
                this._lastNode = null;
                this._zerothDigit = null;
                this._numberSystem = number.NumberSystem;
                this._isPositive = number.IsPositive;

                number.Iterate((node) =>
                {
                    IDigitNode dNode = node as IDigitNode;
                    var newNode = this.AddLeastSignificantDigit(dNode.NodeValue.Symbol);
                    if (dNode.IsZerothDigit())
                    {
                        this._zerothDigit = newNode;
                    }
                }, false);

                this.ScrubLeadingAndTrailingZeroes();
            }
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// compares the length of 2 numbers. ALWAYS does a zero trim on all args.
        /// false = this is less, true= this is greater, null = equal.  ignores sign
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool? AbsoluteValueCompare(Numeric thisNumber, Numeric number)
        {
            if (thisNumber == null)
                throw new ArgumentNullException("thisNumber");

            if (number == null)
                throw new ArgumentNullException("number");

            //Debug.WriteLine("absolute value compare {0} {1}", thisNumber.SymbolsText,
            //    number.SymbolsText);
            ////validate the same number systems
            //if (!thisNumber.NumberSystem.IsCompatible(number.NumberSystem))
            //    throw new InvalidOperationException("incompatible number systems");

            thisNumber.ScrubLeadingAndTrailingZeroes();
            number.ScrubLeadingAndTrailingZeroes();

            //iterate to the most significant digit starting at Zeroth
            ILinkedListNode<IDigit> d1 = thisNumber.ZerothDigit;
            ILinkedListNode<IDigit> d2 = number.ZerothDigit;

            bool? thisIsLarger = null;
            while (true)
            {
                //d2 is longer thus greater
                if (d1.NextNode == null && d2.NextNode != null)
                {
                    thisIsLarger = false;
                    break;
                }
                //d1 is longer thus greater
                if (d2.NextNode == null && d1.NextNode != null)
                {
                    thisIsLarger = true;
                    break;
                }
                //if there are no more nodes, it's a most significant digit compare
                if (d1.NextNode == null && d2.NextNode == null)
                {
                    thisIsLarger = null;
                    break;
                }
                d1 = d1.NextNode;
                d2 = d2.NextNode;
            }

            if (thisIsLarger != null)
                return thisIsLarger;

            //they have the same whole number length, so we need a node by node value compare
            //now walk back from the most significant nodes, d1, d2 respectively
            //and see who is bigger using a digit by digit compare
            while (true)
            {
                if (d1 == null && d2 != null)
                {
                    thisIsLarger = false;
                    break;
                }

                if (d2 == null && d1 != null)
                {
                    thisIsLarger = true;
                    break;
                }

                if (d2 == null && d1 == null)
                {
                    thisIsLarger = null;
                    break;
                }
                var comp = d1.NodeValue.Compare(d2.NodeValue.Symbol);
                if (comp != null)
                {
                    thisIsLarger = comp;
                    break;
                }
                d1 = d1.PreviousNode;
                d2 = d2.PreviousNode;
            }
            return thisIsLarger;
        }

        #endregion
    }

    public class NumericTests
    {
        public static void Test()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }

            //test creation
            var num1 = Numeric.New(set, "123456789");
            Debug.Assert(num1.SymbolsText == "123456789");
            //test set value
            num1.SetValue("-123456788.123");
            Debug.Assert(num1.SymbolsText == "-123456788.123");

            //test compare ops
            for (int i = 1; i < 100; i++)
            {
                var low = Numeric.New(set, i.ToString());
                var low2 = Numeric.New(set, i.ToString());
                var med = low.GetCompatibleNumber((i + 1).ToString());
                var high = low.GetCompatibleNumber((i + 2).ToString());

                Debug.Assert(low.IsEqualTo(low2));
                Debug.Assert(low.IsLessThan(high));
                Debug.Assert(med.IsGreaterThan(low));
                Debug.Assert(high.IsGreaterThan(med));
            }
            for (int i = 1; i < 100; i++)
            {
                var low = Numeric.New(set, "-" + i.ToString());
                var low2 = Numeric.New(set, "-" + i.ToString());
                var med = low.GetCompatibleNumber("-" + (i + 1).ToString());
                var high = low.GetCompatibleNumber("-" + (i + 2).ToString());

                Debug.Assert(low.IsEqualTo(low2));
                Debug.Assert(low.IsGreaterThan(high));
                Debug.Assert(med.IsLessThan(low));
                Debug.Assert(high.IsLessThan(med));
            }

            var num2 = Numeric.New(set, "123.456789");
            var num3 = Numeric.New(set, "123.45678");
            Debug.Assert(num2.IsGreaterThan(num3));

            num2.SwitchSign();
            Debug.Assert(num2.IsLessThan(num3));
            var comp = Numeric.AbsoluteValueCompare(num2, num3);
            Debug.Assert(comp == true);



            var numA = Numeric.New(set, "1234567890");
            var numB = numA.Clone();
            var numC = Numeric.New(set, "1234567890.246");
            var nullNum = Numeric.New(set, null);
            var zeroNum = Numeric.New(set, "0");
            ZoneIterateTest(nullNum);
            ZoneIterateTest(zeroNum);
            ZoneIterateTest(numA);
            ZoneIterateTest(numC);

            //test parallel iteration
            numA.ParallelIterate(numB, (diga, digb) =>
            {
                Debug.Assert(diga.NodeValue.Symbol.Equals(digb.NodeValue.Symbol));
            }, true);
            numA.ParallelIterate(numB, (diga, digb) =>
            {
                Debug.Assert(diga.NodeValue.Symbol.Equals(digb.NodeValue.Symbol));
            }, false);
        }

        public static void ZoneIterateTest(Numeric num)
        {
            Debug.WriteLine("zone iterate test on " + num.SymbolsText);
            Debug.WriteLine("zone iteration towards zero");
            num.ZoneIterate(digit =>
            {
                Debug.WriteLine("post dec zone iterating {0} on digit {1}", num.SymbolsText, digit.NodeValue.Symbol);
            }, digit =>
            {
                Debug.WriteLine("pre dec zone iterating {0} on digit {1}", num.SymbolsText, digit.NodeValue.Symbol);
            }, true);

            Debug.WriteLine("zone iteration away from zero");
            num.ZoneIterate(digit =>
            {
                Debug.WriteLine("post dec zone iterating {0} on digit {1}", num.SymbolsText, digit.NodeValue.Symbol);
            }, digit =>
            {
                Debug.WriteLine("pre dec zone iterating {0} on digit {1}", num.SymbolsText, digit.NodeValue.Symbol);
            }, false);
        }
    }

}
