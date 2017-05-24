using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Guards;

namespace SonDar.ParagonChallenge.Example
{
    class Example2
    {
        public void main()
        {
            //Example 
            string test1 = "test";
            string test2 = null;

            Guard.ArgumentNotNull(() => test1);
            Guard.ArgumentNotNull(() => test2);
            Guard.ArgumentNotNull(test1, nameof(test1));

        }
    }
}
