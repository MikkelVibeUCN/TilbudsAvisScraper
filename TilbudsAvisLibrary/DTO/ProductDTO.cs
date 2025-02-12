﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace TilbudsAvisLibrary.DTO
{
    public class ProductDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public NutritionInfoDTO? NutritionInfo { get; set; }
        public List<PriceDTO> Prices { get; set; }
        public string ExternalId { get; set; }
        public float Amount { get; set; }
    }
}
