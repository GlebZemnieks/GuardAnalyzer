using System;
using Guards;

namespace SonDar.ParagonChallenge.Example
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

            Guard.ArgumentNull(() => test2);
        }

    }
}
