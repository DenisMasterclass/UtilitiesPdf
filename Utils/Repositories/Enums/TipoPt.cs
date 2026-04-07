using System.ComponentModel.DataAnnotations;

namespace Utils.Repositories.Enums
{
    public enum TipoPt
    {
        [Display(Name = "Projeto")]
        Projeto = 1,

        [Display(Name = "Alocacao")]
        Alocação = 2
    }
}
