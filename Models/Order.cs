using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Order
    {
        public int KeyNumber { get; set; }
        public List<Product> products { get; set; }
        
    }
}
