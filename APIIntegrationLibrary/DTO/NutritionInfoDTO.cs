using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIIntegrationLibrary.DTO
{
    public class NutritionInfoDTO
    {
        public int? Id { get; set; }
        public float EnergyKJ { get; set; }
        public float FatPer100G { get; set; }
        public float SaturatedFatPer100G { get; set; }
        public float CarbohydratesPer100G { get; set; }
        public float SugarsPer100G { get; set; }
        public float FiberPer100G { get; set; }
        public float ProteinPer100G { get; set; }
        public float SaltPer100G { get; set; }
    }
}
