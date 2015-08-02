using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Arith.DataStructures;
using Arith.Decorating;
using Arith.Domain.Digits;
using Arith.Extensions;
using Arith.DataStructures.Decorations;

namespace Arith.Domain.Numbers.Decorations
{
    public interface IHasDigitHooks : INumericDecoration,
        IHasA<IHasNodeBuilding<IDigit>>
    {
        /// <summary>
        /// when a digit mutates on a number with this decoration, the mutate strategy runs
        /// </summary>
        Action<IDigitNode, string, MutationMode> PostMutateStrategy { get; set; }
    }

    public class HookingDigitNumericDecoration : NumericDecorationBase,
        IHasDigitHooks
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public HookingDigitNumericDecoration(object decorated,
            string cakeName = null)
            : base(decorated, cakeName)
        {
            //inject node decoration with this strategy
            var builder = this.OuterNodeBuildingList.NodeBuildingStrategy;
            this.OuterNodeBuildingList.NodeBuildingStrategy = (x, list) =>
            {
                var rv = builder(x, list).HasHookingDigitNode();
                var hooker = list.As<HookingDigitNumericDecoration>(false);
                rv.PostMutateStrategy = hooker.PostMutateStrategy;
                return rv;
            };
        }
        #endregion

        #region Static
        public static HookingDigitNumericDecoration New(object decorated, string cakeName = null)
        {
            return new HookingDigitNumericDecoration(decorated, cakeName);
        }
        #endregion

        #region ISerializable
        protected HookingDigitNumericDecoration(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        /// <summary>
        /// since we don't want to expose ISerializable concerns publicly, we use a virtual protected
        /// helper function that does the actual implementation of ISerializable, and is called by the
        /// explicit interface implementation of GetObjectData.  This is the method to be overridden in 
        /// derived classes.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected override void ISerializable_GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.ISerializable_GetObjectData(info, context);
        }
        #endregion


        #region Cake stuff
        public NodeBuildingLinkedListDecoration<IDigit> OuterNodeBuildingList
        {
            get { return this.As<NodeBuildingLinkedListDecoration<IDigit>>(false); }
        }
        #endregion

        #region Overrides
        public override IDecoration ApplyThisDecorationTo(object thing)
        {
            return new HookingDigitNumericDecoration(thing, this.CakeName);
        }
        #endregion

        #region IHasDigitHooks
        public Action<IDigitNode, string, MutationMode> PostMutateStrategy { get; set; }
        #endregion
    }

    public static class HookingDigitNumericDecorationExtensions
    {
        public static HookingDigitNumericDecoration HasHookingDigits(this object number,
            string cakeName = null)
        {

            return HookingDigitNumericDecoration.New(number, cakeName);

        }
    }


    public class HookingNumberTests
    {
        public static void Test()
        {
            //init the set
            NumeralSet set = new NumeralSet(".", "-");
            for (int i = 0; i < 10; i++)
            {
                set.AddSymbolToSet(i.ToString());
            }

            var numA = Numeric.New(set, null).HasHookingDigits();

            numA.PostMutateStrategy = (digit, oldSymbol, mode) =>
            {
                var orderOfMag = digit.GetDigitMagnitude();
                Debug.WriteLine("in number {0}, digit @ pos {1} changed from {2} to {3} via {4}",
                    numA.SymbolsText,
                    orderOfMag.SymbolsText,
                    oldSymbol,
                    digit.NodeValue.Symbol,
                    mode.ToString());
            };
            numA.SetValue("1234567890.246");
            int topLimit = 100;
            for (int x = 0; x < topLimit; x++)
            {
                for (int y = 0; y < topLimit; y++)
                {
                    var num1 = numA.HasAddition();
                    var num2 = Numeric.New(set, y.ToString());
                    num1.Add(num2);
                    num1.AddOne();
                    num1.Subtract(num2);
                    num1.SubtractOne();


                }
            }




        }

    }
}
