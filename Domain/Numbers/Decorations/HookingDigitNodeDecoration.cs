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

namespace Arith.Domain.Numbers.Decorations
{
    public enum MutationMode
    {
        Add,
        Subtract,
        Set
    }
    public interface IHasHookingDigits : IDigitNodeDecoration
    {
        /// <summary>
        /// strategy operates on new node, old value, and mutation mode
        /// </summary>
        Action<IDigitNode, string, MutationMode> PostMutateStrategy { get; set; }
    }

    public class HookingDigitNodeDecoration : DigitNodeDecorationBase, IHasHookingDigits
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public HookingDigitNodeDecoration(object decorated)
            : base(decorated)
        {
        }
        #endregion

        #region Static
        public static HookingDigitNodeDecoration New(object decorated)
        {
            return new HookingDigitNodeDecoration(decorated);
        }
        #endregion

        #region ISerializable
        protected HookingDigitNodeDecoration(SerializationInfo info, StreamingContext context)
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

        #region Overrides
        public override IDecoration ApplyThisDecorationTo(object thing)
        {
            return new HookingDigitNodeDecoration(thing);
        }
        #endregion

        #region IHasHookingDigits
        public Action<IDigitNode, string, MutationMode> PostMutateStrategy { get; set; }
        private void FireMutateStrategy(MutationMode mode, string oldValue)
        {
            if (this.PostMutateStrategy != null)
                this.PostMutateStrategy(this.DecoratedOf, oldValue, mode);
        }

        #endregion

        #region IDigitNodeDecoration
        public override void SetValue(string symbol)
        {
            string oldValue = this.DecoratedOf.Value.Symbol;
            base.SetValue(symbol);
            this.FireMutateStrategy(MutationMode.Set, oldValue);
        }
        public override bool Add(string symbol)
        {
            string oldValue = this.DecoratedOf.Value.Symbol;
            var rv = base.Add(symbol);
            this.FireMutateStrategy(MutationMode.Set, oldValue);
            return rv;
        }
        public override bool Subtract(string symbol)
        {
            string oldValue = this.DecoratedOf.Value.Symbol;
            var rv = base.Subtract(symbol);
            this.FireMutateStrategy(MutationMode.Set, oldValue);
            return rv;
        }
        public override bool AddOne()
        {
            string oldValue = this.DecoratedOf.Value.Symbol;
            var rv = base.AddOne();
            this.FireMutateStrategy(MutationMode.Set, oldValue);
            return rv;
        }
        public override bool SubtractOne()
        {
            string oldValue = this.DecoratedOf.Value.Symbol;
            var rv = base.SubtractOne();
            this.FireMutateStrategy(MutationMode.Set, oldValue);
            return rv;
        }
        #endregion
    }

    public static class HookingDigitNodeDecorationBaseExtensions
    {
        public static HookingDigitNodeDecoration HasHookingDigitNode(this object obj)
        {
            var decoration = obj.ApplyDecorationIfNotPresent<HookingDigitNodeDecoration>(x =>
            {
                return HookingDigitNodeDecoration.New(obj);
            });

            return decoration;
        }
    }
 }
