using System.ComponentModel.DataAnnotations;

namespace UtilitiesPdf.Api.Models;

public class ImportarPropostasPorPastaRequest
{
    [Required]
    public string CaminhoPasta { get; set; } = string.Empty;
}
