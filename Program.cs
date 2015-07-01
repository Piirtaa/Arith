using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Arith
{
    class Program
    {
        static void Main(string[] args)
        {
            LinkedListTests.Test();
            CircularLinkedListTests.Test();
            CircularLinkedListTests.SequenceTest();
            NumeralSetTests.Test();
            SymbolicDigitTests.Test();
            NumberTests.Test();

        }
    }
}
