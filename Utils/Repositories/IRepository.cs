using System.Text;
using Utils.Repositories.Entities;
using Utils.Repositories.Enums;

namespace Utils.Repositories
{
    public interface IRepository
    {
        Task<PropostaEntity> CamposPropostas(StringBuilder sb, TipoPt tipoPt);
        Task<int> InserirPropostaAsync(PropostaEntity Proposta);

    }
}
