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
    /// decorates the list with hooks on mutation events.  
    /// requires mutability.  overrides mutability.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHasLinkedListHooking<T> : ILinkedListDecoration<T>,
        IHasLinkedListMutability<T>, 
        IHasA<IHasLinkedListMutability<T>>
    {
        /// <summary>
        /// set if we have any post insert action/scrubs to perform
        /// </summary>
        Action<ILinkedListNode<T>> PostNodeInsertionStrategy { get; set; }
        /// <summary>
        /// if the list changes in any way (inserts or removal) this strategy is run.  happens after postnodeinsert hook
        /// </summary>
        Action<ILinkedList<T>> PostMutateStrategy { get; set; }
    }

    public class HookedMutableLinkedListDecoration<T> : LinkedListDecorationBase<T>,
        IHasLinkedListHooking<T>
    {
        #region Declarations
        private readonly object _stateLock = new object();

        protected bool _isInserting = false;
        protected bool _isMutating = false;
        #endregion

        #region Ctor
        public HookedMutableLinkedListDecoration(object decorated,
            Action<ILinkedListNode<T>> postNodeInsertionStrategy = null,
            Action<ILinkedList<T>> postMutateStrategy = null, 
            string decorationName = null)
            : base(decorated, decorationName)
        {
            this.PostMutateStrategy = postMutateStrategy;
            this.PostNodeInsertionStrategy = postNodeInsertionStrategy;
        }
        #endregion

        #region Static
        public static HookedMutableLinkedListDecoration<T> New(object decorated,
            Action<ILinkedListNode<T>> postNodeInsertionStrategy = null,
            Action<ILinkedList<T>> postMutateStrategy = null, 
            string decorationName = null)
        {
            return new HookedMutableLinkedListDecoration<T>(decorated, 
                postNodeInsertionStrategy, 
                postMutateStrategy,
                decorationName);
        }
        #endregion

        #region ISerializable
        protected HookedMutableLinkedListDecoration(SerializationInfo info, StreamingContext context)
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
            return new HookedMutableLinkedListDecoration<T>(thing,
                this.PostNodeInsertionStrategy,
                this.PostMutateStrategy,
                this.DecorationName);
        }
        #endregion

        #region Properties
        public IHasLinkedListMutability<T> MutableDecoratedOf
        {
            get { return this.Decorated.AsBelow<IHasLinkedListMutability<T>>(false); }
        }

        /// <summary>
        /// set if we have any post insert action/scrubs to perform
        /// </summary>
        public Action<ILinkedListNode<T>> PostNodeInsertionStrategy { get; set; }
        /// <summary>
        /// if the list changes in any way (inserts or removal) this strategy is run.  happens after postnodeinsert hook
        /// </summary>
        public Action<ILinkedList<T>> PostMutateStrategy { get; set; }
        #endregion

        #region Hook Helpers
        /// <summary>
        /// runs the post insert hook.  ensures that the hook doesn't trigger a recursion
        /// into this method if the hook implementation mutates the list itself.  which is another
        /// way of say if your hook mutates the list don't expect this method to run any checks
        /// </summary>
        public void RunPostInsertHook(ILinkedListNode<T> node)
        {
            if (this._isInserting)
                return;

            lock (this._stateLock)
            {
                if (this._isInserting)
                    return;

                try
                {
                    this._isInserting = true;

                    //post post mutate hook
                    if (this.PostNodeInsertionStrategy != null)
                    {
                        this.PostNodeInsertionStrategy(node);
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    this._isInserting = false;
                }
            }
        }
        /// <summary>
        /// runs the post mutation hook.  ensures that the hook doesn't trigger a recursion
        /// into this method if the hook implementation mutates the list itself.  which is another
        /// way of say if your hook mutates the list don't expect this method to run any checks
        /// </summary>
        public void RunPostMutateHook()
        {
            if (this._isMutating)
                return;

            lock (this._stateLock)
            {
                if (this._isMutating)
                    return;

                try
                {
                    this._isMutating = true;

                    //post post mutate hook
                    if (this.PostMutateStrategy != null)
                    {
                        this.PostMutateStrategy(this);
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    this._isMutating = false;
                }
            }
        }
        #endregion

        #region Overrides

        public virtual ILinkedList<T> Remove(ILinkedListNode<T> item)
        {
            var rv = this.MutableDecoratedOf.Remove(item);

            this.RunPostMutateHook();

            return rv;
        }
        public virtual ILinkedListNode<T> InsertNode(ILinkedListNode<T> node,
            ILinkedListNode<T> before, 
            ILinkedListNode<T> after)
        {
            var rv = this.MutableDecoratedOf.InsertNode(node, before, after);

            this.RunPostInsertHook(rv);

            this.RunPostMutateHook();

            return rv;
        }
        #endregion
    }

    public static class HookedLinkedListDecorationExtensions
    {
        public static HookedMutableLinkedListDecoration<T> HasHooks<T>(this ILinkedList<T> thing,
            Action<ILinkedListNode<T>> postNodeInsertionStrategy = null,
            Action<ILinkedList<T>> postMutateStrategy = null, 
            string decorationName = null)
        {
            return HookedMutableLinkedListDecoration<T>.New(
                 thing,
                 postNodeInsertionStrategy,
                 postMutateStrategy,
                 decorationName);
        }
        public static HookedMutableLinkedListDecoration<T> GetHookingCake<T>(this ILinkedList<T> thing,
    Action<ILinkedListNode<T>> postNodeInsertionStrategy = null,
    Action<ILinkedList<T>> postMutateStrategy = null,
    string decorationName = null)
        {
            var rv = thing.HasMutability<T>(decorationName).HasHooks<T>(postNodeInsertionStrategy,
                postMutateStrategy, decorationName);
            return rv;
        }
        public static void AppendNodeInsertionStrategy<T>(this HookedMutableLinkedListDecoration<T> thing,
            Action<ILinkedListNode<T>> postNodeInsertionStrategy = null)
        {
            var oldStrategy = thing.PostNodeInsertionStrategy;
            thing.PostNodeInsertionStrategy = (node) =>
            {
                if (oldStrategy != null)
                    oldStrategy(node);

                postNodeInsertionStrategy(node);
            };
        }
        public static void AppendPostMutateStrategy<T>(this HookedMutableLinkedListDecoration<T> thing,
            Action<ILinkedList<T>> postMutateStrategy = null)
        {
            var oldStrategy = thing.PostMutateStrategy;
            thing.PostMutateStrategy = (list) =>
            {
                if (oldStrategy != null)
                    oldStrategy(list);

                postMutateStrategy(list);
            };
        }
    }



    public class HookedLinkedListTests
    {
        public static void Test()
        {


            var list = LinkedList<int>.New().GetHookingCake<int>();

            int counter = 0;

            list.PostMutateStrategy = (l) =>
            {
                Debug.WriteLine("mutating to " + l.LastNode.NodeValue);
                Debug.Assert(counter == l.LastNode.NodeValue);
            };

            list.PostNodeInsertionStrategy = (item) =>
            {
                //this hook happens first, so the counter should lag the item
                Debug.Assert(counter == item.NodeValue - 1);
                counter++;
            };

            int topLimit = 1000;
            for (int x = 1; x < topLimit; x++)
            {
                list.DoWhileNodeBuilding<int>(null, o =>
                {
                    o.AddLast(x);
                });
            }

            list.PostMutateStrategy = (l) =>
            {
                if (!l.IsEmpty())
                {
                    Debug.WriteLine("mutating to " + l.LastNode.NodeValue);
                    counter--;
                    Debug.Assert(counter == l.LastNode.NodeValue);
                }
            };

            list.PostNodeInsertionStrategy = null;

            for (int x = 1; x < topLimit; x++)
            {
                list.Remove(list.LastNode);
            }
        }

    }
}
