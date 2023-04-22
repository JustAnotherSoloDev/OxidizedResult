using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    public class TestClass
    {
        public static int AddOne(int value)
        {
            return value + 1;
        }
        public static int AddTwo(int value) { return value + 2; }
        public static int AddThree(int value) { return value + 3; }
        public static int AddFour(int value) { return value + 4; }
        public static int ThrowNotFinitneError(int value) { throw new NotFiniteNumberException(); }

        public static int ThrowArgumentException(int value)
        {
            throw new ArgumentException();
        }
    }
}
