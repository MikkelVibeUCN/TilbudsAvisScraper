using DAL.Data.Interfaces;
using TilbudsAvisLibrary.DTO;
using TilbudsAvisLibrary.Entities;
using TIlbudsAvisScraperAPI.Tools;

namespace TIlbudsAvisScraperAPI.Services
{
    public class AvisService
    {
        private readonly IAvisDAO _avisDAO;

        public AvisService(IAvisDAO avisDAO)
        {
            _avisDAO = avisDAO;
        }

        public async Task<AvisDTO> GetValidAvisForCompany(int company)
        {
            Avis avis = await _avisDAO.Get(company);
            return EntityMapper.MapToDTO(avis);
        }

        public async Task<int> Add(AvisDTO avisDto, int companyId)
        {
            Avis mappedAvis = EntityMapper.MapToEntity(avisDto);
            return await _avisDAO.Add(mappedAvis, companyId);
        }
    }
}
