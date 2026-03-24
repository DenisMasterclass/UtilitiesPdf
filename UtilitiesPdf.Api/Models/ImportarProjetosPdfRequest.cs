using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UtilitiesPdf.Api.Models;

public class ImportarPropostasPdfRequest
{
    [Required]
    [MinLength(1)]
    public List<IFormFile> Arquivos { get; set; } = [];
}
