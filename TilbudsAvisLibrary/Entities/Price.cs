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
        public int _id { get; private set; }
        public float _priceValue { get; set; }
        public string _externalAvisId { get; set; }
        public string _compareUnitPrice { get; set; }
        
        [JsonConstructor]
        public Price(float priceValue)
        {
            this._priceValue = priceValue;
        }
        public Price(float priceValue, string compareUnitPrice)
        {
            this._priceValue = priceValue;
            this._compareUnitPrice = compareUnitPrice;
        }
        // Base price generation
        public Price(float priceValue, string externalAvisId, string compareUnitPrice)
        {
            _priceValue = priceValue;
            _externalAvisId = externalAvisId;
            _compareUnitPrice = compareUnitPrice;
        }

        public void SetId(int id) => _id = id;


    }
}
