using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arith
{
    public class ArithMatrix
    {
        #region Declarations
        private int _baseSize = 0;
        #endregion

        #region Ctor
        public ArithMatrix(int baseSize)
        {
            if (baseSize <= 0)
                throw new ArgumentOutOfRangeException("base must be positive integer");

            this._baseSize = baseSize;
        }
        #endregion

        #region Properties

        #endregion

        #region Methods

        #endregion
    }
}
