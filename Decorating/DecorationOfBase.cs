using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Arith.Extensions;
using Arith.DataStructures;

namespace Arith.Decorating
{

    /// <summary>
    /// abstract class that provides templated implementation of a Decorator/Wrapper
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>
    /// Implements ISerializable so that derivations from this class will have hooks to implement
    /// native serialization
    /// </remarks>
    public abstract class DecorationOfBase<T> : DisposableBase,
        IDecorationOf<T>, ISerializable, 
        IDecoratorAwareDecoration, ITogglingDecoration
    {
        #region Declarations
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private T _Decorated;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private T _Inner;
        #endregion

        #region Ctor
        /// <summary>
        /// the base ctor for a decoration.  it MUST decorate something!!  
        /// </summary>
        /// <param name="decorated">kacks on null</param>
        public DecorationOfBase(T decorated)
        {
            this.SetDecorated(decorated);
        }
        #endregion

        #region ISerializable
        protected DecorationOfBase(SerializationInfo info, StreamingContext context)
        {
            this._Decorated = (T)info.GetValue("_Decorated", typeof(T));
        }
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ISerializable_GetObjectData(info, context);
        }
        /// <summary>
        /// since we don't want to expose ISerializable concerns publicly, we use a virtual protected
        /// helper function that does the actual implementation of ISerializable, and is called by the
        /// explicit interface implementation of GetObjectData.  This is the method to be overridden in 
        /// derived classes.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected virtual void ISerializable_GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_Decorated", this._Decorated);
        }
        #endregion

        #region IDecoration
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Decorated { get { return this._Decorated; } }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object IDecoration.Decorated
        {
            get { return this.Decorated; }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Inner { get { return this._Inner; } }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object IDecoration.Inner { get { return this.Inner; } }
        #endregion

        #region IDecorationOf
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public abstract T This { get; }
        #endregion

        #region IDecoratorAwareDecoration
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        /// <summary>
        /// the thing decorating this
        /// </summary>
        public object Decorator { get; set; }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        /// <summary>
        /// the outermost decoration in the stack
        /// </summary>
        public object Outer { get { return this.GetOuterDecorator(); } }
        #endregion

        #region ITogglingDecoration
        public bool IsDecorationEnabled { get; set; }
        #endregion

        #region Calculated Properties
        /// <summary>
        /// returns whether the decoration is decorating something.  We use this as a check that the
        /// decoration chain is unadulterated, and was constructed in a correct way. The general principle is
        /// to NOT allow decorations to be incorrectly constructed.
        /// </summary>
        public bool IsDecorating
        {
            get { return this.Decorated != null; }
        }
        public List<object> Cake { get { return this.GetAllDecorations(); } }
        #endregion


        #region Methods
        /// <summary>
        /// sets the Decorated property.  If null, kacks
        /// </summary>
        /// <param name="decorated"></param>
        protected void SetDecorated(T decorated)
        {
            if (decorated == null)
                throw new InvalidOperationException("null decoration injection");

            if (decorated is ISealedDecoration)
                throw new InvalidOperationException("Cannot decorate a SealedDecoration");

            //if decorated is a decoration, we must ensure that none of the decoration layers are equal to this 
            //or we'll get a circ reference situation
            var decorationList = decorated.GetAllDecorations();
            //remove the first decoration because it is equivalent to "this"

            if (decorationList != null)
            {
                foreach (var each in decorationList)
                {
                    if (object.ReferenceEquals(each, this))
                        throw new InvalidOperationException("circular reference");
                }
            }

            //set decorated
            this._Decorated = decorated;

            //set inner
            if (decorated is IDecorationOf<T>)
            {
                IDecorationOf<T> dec = decorated as IDecorationOf<T>;
                this._Inner = dec.Inner;
            }
            else
            {
                this._Inner = decorated;
            }

            //set the decorator backreference
            if (decorated.IsADecoratorAwareDecoration())
            {
                (decorated as IDecoratorAwareDecoration).Decorator = this;
            }

            //validate IHasA constraints
            this.ValidateIHasAConstraints();

        }

        #endregion

        #region IDecoration
        public IDecoration ApplyThisDecorationTo(object thing)
        {
            return this.ApplyThisDecorationTo((T)thing);
        }
        public abstract IDecorationOf<T> ApplyThisDecorationTo(T thing);
        #endregion

        #region Disposable
        protected override void DisposeManaged()
        {
            //dispose the wrapper
            if (this.Decorated != null && this.Decorated is IDisposable)
            {
                ((IDisposable)(this.Decorated)).Dispose();
            }
            base.DisposeManaged();
        }
        #endregion
    }


}
