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
        public string ExternalAvisId { get; set; } 
        public string CompareUnitPrice { get; set; }

        [JsonConstructor]
        public Price(int id, float priceValue, string externalAvisId, string compareUnitPrice)
        {
            this.PriceValue = priceValue;
            this.Id = id;
            this.ExternalAvisId = externalAvisId;
            this.CompareUnitPrice = compareUnitPrice;
        }
        // Base price generation
        public Price(float priceValue, string externalAvisId, string compareUnitPrice)
        {
            PriceValue = priceValue;
            ExternalAvisId = externalAvisId;
            CompareUnitPrice = compareUnitPrice;
        }
        public Price(float priceValue, string compareUnitPrice)
        { 
            this.PriceValue = priceValue;
            this.CompareUnitPrice = compareUnitPrice;
        }
        public void SetId(int id) => Id = id;


    }
}
