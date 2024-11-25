using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary.DTO
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

        public override string ToString()
        {
            return $"Energy: {EnergyKJ} KJ / {EnergyKJ} Kcal\nFat: {FatPer100G} g\nSaturated Fat: {SaturatedFatPer100G} g\n" +
                   $"Carbohydrates: {CarbohydratesPer100G} g\nSugars: {SugarsPer100G} g\nFiber: {FiberPer100G} g\nProtein: {ProteinPer100G} g\nSalt: {SaltPer100G} g";
        }
    }
}
