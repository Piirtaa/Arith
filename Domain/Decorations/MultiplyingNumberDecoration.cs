﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arith.Decorating;
using System.Runtime.Serialization;
using Arith.DataStructures;

namespace Arith.Domain.Decorations
{
    public interface IHasMultiplication : INumberDecoration
	{
		void Multiply(string number); 
	}

    public class MultiplyingNumberDecoration : NumberDecorationBase, IHasMultiplication
    {
        #region Declarations
        private readonly object _stateLock = new object();
        private SquareLookup<SymbolicNumber> _multMap = null;
        #endregion

        #region Ctor
        public MultiplyingNumberDecoration(INumber decorated)
            : base(decorated)
        {
            this.InitMap();
        }
        #endregion


        #region ISerializable
        protected MultiplyingNumberDecoration(SerializationInfo info, StreamingContext context)
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
        public override IDecorationOf<INumber> ApplyThisDecorationTo(INumber thing)
        {
            return new MultiplyingNumberDecoration(thing);
        }
        #endregion

        #region Helpers
        private void InitMap()
        {
            var keys = this.NumberSystem.SymbolSet.Values;
            this._multMap = new SquareLookup<SymbolicNumber>(keys);

            foreach (string each in keys)
            {
                foreach (string each2 in keys)
                {
                    //add each to itself, each2 times
                    var total = new SymbolicNumber(each, this.NumberSystem);
                    var counter = new SymbolicNumber(each2, this.NumberSystem);
                    counter.CountdownToZero(c =>
                    {
                        total.Add(total);
                    });

                    //register it
                    this._multMap.Add(each, each2, total);
                }
            }
        }
        /// <summary>
        /// multiplies 2 digits given their positions
        /// </summary>
        /// <param name="digit1"></param>
        /// <param name="digit2"></param>
        /// <param name="digit1Pos"></param>
        /// <param name="digit2Pos"></param>
        /// <returns></returns>
        private SymbolicNumber MultiplyDigits(string digit1, string digit2, 
            SymbolicNumber digit1Pos, SymbolicNumber digit2Pos)
        {
            var val = this._multMap.Get(digit1, digit2);

            var rv = SymbolicNumber.Clone(val);

            digit1Pos.CountdownToZero((c) =>
            {
                rv.ShiftLeft();
            });
            digit2Pos.CountdownToZero((c) =>
            {
                rv.ShiftLeft();
            });
            return rv;
        }
        #endregion

        #region IHasMultiplication
        public void Multiply(string number)
        {
            /*
             * Xn, Xn-1,...,X1, X0
             * Yn, Yn-1,...,Y1, Y0
             * 
             * To multiply this process is followed:
             * 
             * foreach X digit
             *  foreach Y digit
             *      multiply digits to get a Number
             *      shift left X position + Y position times
             *          (eg. Y0 * Xn-1, shift left n-1 times)
             *      add to total
             * 
             */

            lock (this._stateLock)
            {
                var arg = new SymbolicNumber(number, this.NumberSystem);
                var thisNum = SymbolicNumber.Clone(this.SymbolicNumber);
                Number sum = new Number(this.NumberSystem.ZeroSymbol, this.NumberSystem);

                //shift both arg and thisnum to zero, to eliminate clarity with decimal shift handling
                //we'll shift back at the end.  this way we're only multiplying whole numbers.
                //also makes the code cleaner

                var argShifts = arg.ShiftToZero();
                var thisNumShifts = thisNum.ShiftToZero();

                this.SymbolicNumber.IterateFromZeroth((thisDigit, thisIdx) =>
                {
                    arg.IterateFromZeroth((argDigit, argIdx) =>
                    {
                        var digitProduct = this.MultiplyDigits(argDigit.Symbol, thisDigit.Symbol, argIdx, thisIdx);
                        sum.SymbolicNumber.Add(digitProduct);
                    }, (argDigit, argIdx) =>
                    {
                        //no operation will happen here because we've shifted to zero
                    });
                }, (thisDigit, thisIdx) =>
                {
                    //no operation will happen here because we've shifted to zero
                });

                //shift back
                sum.SymbolicNumber.ShiftRight(argShifts).ShiftRight(thisNumShifts);

                this.SymbolicNumber.SetValue(sum.SymbolicNumber);
            }
        }
        #endregion
    }


}
