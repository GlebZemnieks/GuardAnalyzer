using System;
using Guards;

namespace SonDar.ParagonChallenge.GuardAnalyzer
{
    class ExampleClass
    {

        public void Test(String test)
        {
            //Example 
            string test1 = "test";
            string test2 = null;

            Guard.ArgumentNotNull(() => test1);
            Guard.ArgumentNotNull(() => test2);
        }

    }
}
