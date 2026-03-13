using System.Text;
using Utils.Repositories.Entities;

namespace Utils.Repositories
{
    public interface IRepository
    {
        Task<ProjetoEntity> CamposProjetos(StringBuilder sb);
        Task<ProjetoEntity> ReadProject(ProjetoEntity projetoEntity);

    }
}