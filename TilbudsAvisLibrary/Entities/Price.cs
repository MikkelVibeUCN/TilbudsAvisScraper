using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary.Entities
{
    public class Price
    {
        public int Id { get; set; }
        public float PriceValue { get; set; }
        public Price(int id, float priceValue)
        {
            this.Id = id;
            this.PriceValue = priceValue;
        }
    }
}
