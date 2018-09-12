using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Threading
{
    public class Payment
    {
        public string Type { get; set; }
        public string ps { get; set; }
        public string fee { get; set; }
        public string psPayMethod { get; set; }      
    }

    public class Channel
    {
        public List<Payment> Payment { get; set; }
        public string Name { get; set; }
        public Channel()
        {
            Payment = new List<Payment>();
        }
    }

    public class Billing
    {
        public List<Channel> Channel { get; set; }

        public Billing()
        {
            Channel = new List<Channel>();
        }
    }
}
