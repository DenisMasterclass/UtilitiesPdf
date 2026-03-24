using System.Text;
using Utils.Repositories.Entities;

namespace Utils.Repositories
{
    public interface IRepository
    {
        Task<PropostaEntity> CamposPropostas(StringBuilder sb);
        Task<int> InserirPropostaAsync(PropostaEntity Proposta);

    }
}
