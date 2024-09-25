using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary.Entities
{
    public class Price
    {
        public int Id { get; private set; }
        public float PriceValue { get; set; }

        [JsonConstructor]
        public Price(float priceValue)
        {
            this.PriceValue = priceValue;
        }
        public Price(int id, float priceValue)
        {
            this.Id = id;
            this.PriceValue = priceValue;
        }

        public void SetId(int id) => Id = id;


    }
}
