﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Arith.DataStructures;
using Arith.Extensions;

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
    public abstract class DecorationBase : DisposableBase,
        IDecoration, ISerializable,
        IDecoratorAwareDecoration, ITogglingDecoration
    {
        #region Declarations
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object _Decorated;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object _Inner;
        #endregion

        #region Ctor
        /// <summary>
        /// the base ctor for a decoration.  it MUST decorate something!!  
        /// </summary>
        /// <param name="decorated">kacks on null</param>
        public DecorationBase(object decorated, string cakeName = null)
        {
            this.IsDecorationEnabled = true;

            //set cake name first, as SetDecorated will validate the cake
            //and cake validation ensures dependencies have same cake name
            if (cakeName != null)
                this.CakeName = cakeName;

            //set decorated
            this.SetDecorated(decorated);


        }
        #endregion

        #region ISerializable
        protected DecorationBase(SerializationInfo info, StreamingContext context)
        {
            Type type = info.GetValue("_type", typeof(Type)) as Type;
            this._Decorated = info.GetValue("_Decorated", type);
            this.CakeName = info.GetString("cakeName");
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
            info.AddValue("cakeName", this.CakeName);
            info.AddValue("_Decorated", this._Decorated);
            info.AddValue("_type", this.Decorated.GetType());
        }
        #endregion

        #region IDecoration
        /// <summary>
        /// provides discriminator to group layers together
        /// </summary>
        public string CakeName { get; protected set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public object Decorated { get { return this._Decorated; } }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public object Inner { get { return this._Inner; } }
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
        /// removes self from the cake and returns decorated value
        /// </summary>
        /// <returns></returns>
        public object Undecorate()
        {
            //set the decorator backreference
            if (this.Decorated.IsADecoratorAwareDecoration())
            {
                (this.Decorated as IDecoratorAwareDecoration).Decorator = null;
            }
            return this.Decorated;
        }
        /// <summary>
        /// sets the Decorated property.  If null, kacks
        /// </summary>
        /// <param name="decorated"></param>
        protected void SetDecorated(object decorated)
        {
            //validate
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
            if (decorated is IDecoration)
            {
                IDecoration dec = decorated as IDecoration;
                this._Inner = dec.Inner;

                //take the cakename from the decorated thing if we don't have one
                if (this.CakeName == null)
                    this.CakeName = dec.CakeName;
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

            //validate IHasA declarations exist
            this.ValidateIHasAConstraints();


        }
        #endregion

        #region IDecoration
        public abstract IDecoration ApplyThisDecorationTo(object thing);
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
