using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace AvaliacaoRestaurantesAPI.DTOs;

public class ReviewAlterarDto
{
    public int Nota { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public IList<IFormFile>? Fotos { get; set; }
}
