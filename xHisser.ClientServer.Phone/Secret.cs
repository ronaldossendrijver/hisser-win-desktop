using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigMath;

namespace Hisser.ClientServer
{
    public class Secret
    {
        public static readonly BigInteger p = new BigInteger("416064700201658306196320137931");
        public static readonly BigInteger g = 2;

        public BigInteger Value
        {
            get
            {
                return OtherComponent.PublicValue.ModPow(MyComponent.PrivateValue, p);
            }
        }

        public SentComponent MyComponent { get; private set; }

        public ReceivedComponent OtherComponent { get; private set; }
        
        public Secret(
            SentComponent myComponent,
            ReceivedComponent otherComponent)
        {
            MyComponent = myComponent;
            OtherComponent = otherComponent;
        }
    }
}
