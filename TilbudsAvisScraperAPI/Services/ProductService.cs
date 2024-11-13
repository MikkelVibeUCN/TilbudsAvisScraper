﻿using APIIntegrationLibrary.DTO;
using DAL.Data.Interfaces;
using TilbudsAvisLibrary;
using TIlbudsAvisScraperAPI.Tools;

namespace TIlbudsAvisScraperAPI.Services
{
    public class ProductService
    {
        private readonly IProductDAO _productDAO;
        public ProductService(IProductDAO productDAO)
        {
            _productDAO = productDAO;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsAsync(ProductQueryParameters parameters)
        {
            var companies = await _productDAO.GetAllProdudctsWithInformationFromCompany(parameters);

            return EntityMapper.MapToDTO(companies);
        }

        public async Task<int> GetProductCountAsync(ProductQueryParameters parameters)
        {
            return await _productDAO.GetProductCountAsync(parameters);
        }

        public async Task<IEnumerable<string>> GetValidCompanyNamesFromProductSerach(ProductQueryParameters parameters)
        {
            return await _productDAO.GetValidCompanyNamesFromProductSerach(parameters);
        }
    }
}