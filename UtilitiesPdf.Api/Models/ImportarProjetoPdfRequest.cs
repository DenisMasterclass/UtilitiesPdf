using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UtilitiesPdf.Api.Models;

public class ImportarPropostaPdfRequest
{
    [Required]
    public IFormFile Arquivo { get; set; } = default!;
}
