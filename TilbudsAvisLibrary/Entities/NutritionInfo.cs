using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TilbudsAvisLibrary.Entities
{
    public class NutritionInfo : IParameters
    {
        private const float ConversionRate = 0.239006f;
        public int? Id { get; set; }
        public float EnergyKJ { get; set; }
        [JsonIgnore]
        public float EnergyKcal
        {
            get { return EnergyKJ * ConversionRate; }
        }
        public float FatPer100G { get; set; }
        public float SaturatedFatPer100G { get; set; }
        public float CarbohydratesPer100G { get; set; }
        public float SugarsPer100G { get; set; }
        public float FiberPer100G { get; set; }
        public float ProteinPer100G { get; set; }
        public float SaltPer100G { get; set; }

        public NutritionInfo(float energyKJ, float fatPer100g, float carbohydratesPer100G, float sugarsPer100G, float FiberPer100G, float ProteinPer100G, float saltPer100G)
        {
            this.EnergyKJ = energyKJ;
            this.FatPer100G = fatPer100g;
            this.CarbohydratesPer100G = carbohydratesPer100G;
            this.SugarsPer100G = sugarsPer100G;
            this.FiberPer100G = FiberPer100G;
            this.ProteinPer100G = ProteinPer100G;
            this.SaltPer100G = saltPer100G;
        }

        public NutritionInfo()
        {

        }

        public override string ToString()
        {
            return $"Energy: {EnergyKJ} KJ / {EnergyKcal} Kcal\nFat: {FatPer100G} g\nSaturated Fat: {SaturatedFatPer100G} g\n" +
                   $"Carbohydrates: {CarbohydratesPer100G} g\nSugars: {SugarsPer100G} g\nFiber: {FiberPer100G} g\nProtein: {ProteinPer100G} g\nSalt: {SaltPer100G} g";
        }

        public int TotalParameterAmount()
        {
            return 9;
        }
    }
}
