using DAL.Data.Interfaces;
using TilbudsAvisLibrary.DTO;
using TilbudsAvisLibrary.Entities;
using TilbudsAvisScraperAPI.Tools;

namespace TilbudsAvisScraperAPI.Services
{
    public class AvisService
    {
        private readonly IAvisDAO _avisDAO;

        public AvisService(IAvisDAO avisDAO)
        {
            _avisDAO = avisDAO;
        }

        public async Task<AvisDTO> GetLatestAvisForCompany(int company)
        {
            int newestId = await _avisDAO.GetLatestAvisId(company);

            if (newestId == -1)
            {
                throw new Exception("Failed to get latest avis from company");
            }

            Avis? avis = await _avisDAO.Get(newestId);

            if (avis == null)
            {
                throw new Exception($"Failed to get avis with id {newestId}");
            }

            return EntityMapper.MapToDTO(avis);
        }

        public async Task<int> Add(AvisDTO avisDto, int companyId)
        {
            Avis mappedAvis = EntityMapper.MapToEntity(avisDto);
            return await _avisDAO.Add(mappedAvis, companyId);
        }
    }
}
