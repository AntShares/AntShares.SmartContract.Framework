using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

namespace Neo.Compiler.MSIL.TestClasses
{
    public class Contract_Iterator : SmartContract.Framework.SmartContract
    {
        public static int TestNextArray(int[] a)
        {
            int sum = 0;
            var iterator = Iterator<int, int>.Create(a);

            while (iterator.Next())
            {
                sum += iterator.Value;
            }

            return sum;
        }

        public static int TestConcatArray(int[] a, int[] b)
        {
            int sum = 0;
            var iteratorA = Iterator<int, int>.Create(a);
            var iteratorB = Iterator<int, int>.Create(b);
            var iteratorC = iteratorA.Concat(iteratorB);

            while (iteratorC.Next())
            {
                sum += iteratorC.Value;
            }

            return sum;
        }

        public static int TestConcatMap(Map<int, int> a, Map<int, int> b)
        {
            int sum = 0;
            var iteratorA = Iterator<int, int>.Create(a);
            var iteratorB = Iterator<int, int>.Create(b);
            var iteratorC = iteratorA.Concat(iteratorB);

            while (iteratorC.Next())
            {
                sum += iteratorC.Key;
                sum += iteratorC.Value;
            }

            return sum;
        }

        public static int TestConcatKeys(Map<int, int> a, Map<int, int> b)
        {
            int sum = 0;
            var iteratorA = Iterator<int, int>.Create(a);
            var iteratorB = Iterator<int, int>.Create(b);
            var iteratorC = iteratorA.Concat(iteratorB);
            var enumerator = iteratorC.Keys;

            while (enumerator.Next())
            {
                sum += enumerator.Value;
            }

            return sum;
        }

        public static int TestConcatValues(Map<int, int> a, Map<int, int> b)
        {
            int sum = 0;
            var iteratorA = Iterator<int, int>.Create(a);
            var iteratorB = Iterator<int, int>.Create(b);
            var iteratorC = iteratorA.Concat(iteratorB);
            var enumerator = iteratorC.Values;

            while (enumerator.Next())
            {
                sum += enumerator.Value;
            }

            return sum;
        }
    }
}
