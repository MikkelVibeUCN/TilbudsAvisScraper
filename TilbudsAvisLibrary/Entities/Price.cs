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
        public int ExternalAvisId { get; set; }

        [JsonConstructor]
        public Price(float priceValue)
        {
            this.PriceValue = priceValue;
        }
        public Price(int id, float priceValue, int externalAvisId)
        {
            this.Id = id;
            this.PriceValue = priceValue;
            this.ExternalAvisId = externalAvisId;
        }

        public void SetId(int id) => Id = id;


    }
}
