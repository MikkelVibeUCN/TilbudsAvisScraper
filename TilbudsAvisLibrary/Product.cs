using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary
{
    public class Product
    {
        public string Name { get; set; }
        public float Price { get; set; }
        public Product(string name, float price)
        {
            this.Name = name;
            this.Price = price;
        }
    }
}