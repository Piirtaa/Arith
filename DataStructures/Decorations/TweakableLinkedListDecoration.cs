using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Decorating;
using System.Runtime.Serialization;
using Arith.DataStructures;
using System.Diagnostics;

namespace Arith.DataStructures.Decorations
{
    /// <summary>
    /// decorates the list with ability to edit linked list first and last nodes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHasTweaks<T> : ILinkedListDecoration<T>, 
                IIsA<IHasHooks<T>>
    {
        void SetFirstNode(ILinkedListNode<T> node);
        void SetLastNode(ILinkedListNode<T> node);
    }

    public class TweakableLinkedListDecoration<T> : LinkedListDecorationBase<T>,
        IHasTweaks<T>
    {
        #region Declarations
        private readonly object _stateLock = new object();
        #endregion

        #region Ctor
        public TweakableLinkedListDecoration(object decorated)
            : base(decorated)
        {
        }
        #endregion

        #region Static
        public static TweakableLinkedListDecoration<T> New(object decorated)
        {
            return new TweakableLinkedListDecoration<T>(decorated);
        }
        #endregion

        #region ISerializable
        protected TweakableLinkedListDecoration(SerializationInfo info, StreamingContext context)
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
            return new TweakableLinkedListDecoration<T>(thing);
        }
        #endregion

        #region IHasTweaks
        public void SetFirstNode(ILinkedListNode<T> node)
        {
            lock (this._stateLock)
            {
                var inner = this.As<LinkedList<T>>(false);
                inner._firstNode = node;
                this.FirstNode.PreviousNode = null;

                this.As<HookedLinkedListDecoration<T>>(true).RunPostMutateHook();
            }
        }
        public void SetLastNode(ILinkedListNode<T> node)
        {
            lock (this._stateLock)
            {
                var inner = this.As<LinkedList<T>>(false);
                inner._lastNode = node;
                this.LastNode.NextNode = null;

                this.As<HookedLinkedListDecoration<T>>(true).RunPostMutateHook();
            }
        }
        #endregion
    }

    public static class TweakableLinkedListDecorationExtensions
    {
        public static TweakableLinkedListDecoration<T> HasTweaks<T>(this object thing)
        {
            var decoration = thing.ApplyDecorationIfNotPresent<TweakableLinkedListDecoration<T>>(x =>
            {
                return TweakableLinkedListDecoration<T>.New(thing.HasHooks<T>().Outer);
            });

            return decoration;
        }
    }



    public class TweakableLinkedListTests
    {
        public static void Test()
        {
            var list = LinkedList<int>.New().HasTweaks<int>();

            int topLimit = 1000;
            for (int x = 1; x < topLimit; x++)
            {
                list.AddLast(x);
            }

            list.SetLastNode(list.LastNode.PreviousNode);
            Debug.Assert(list.LastNode.Value.Equals(998));
            list.SetFirstNode(list.FirstNode.NextNode);
            Debug.Assert(list.FirstNode.Value.Equals(2));

        }

    }
}
