using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary.Entities
{
    public class Price : IParameters
    {
        public int Id { get; private set; } 
        public float PriceValue { get; set; }
        public string? ExternalAvisId { get; set; } 
        public float UnitPrice { get; set; }
        public string? CompareUnitString { get; set; }

        [JsonConstructor]
        public Price(int id, float priceValue, string externalAvisId,string compareUnitString)
        {
            this.PriceValue = priceValue;
            this.Id = id;
            this.ExternalAvisId = externalAvisId;
            this.CompareUnitString = compareUnitString;
        }
        public Price(float priceValue, string compareUnitString, string externalAvisId = "")
        { 
            this.PriceValue = priceValue;
            this.CompareUnitString = compareUnitString;
            this.ExternalAvisId = externalAvisId;
        }

        public Price(float price)
        {
            this.PriceValue = price;
        }

        public Price()
        {
        }
        public void SetId(int id) => Id = id;

        public int TotalParameterAmount()
        {
            return 4;
        }
    }
}
