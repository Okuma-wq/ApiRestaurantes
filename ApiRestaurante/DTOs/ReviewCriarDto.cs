using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace AvaliacaoRestaurantesAPI.DTOs;

public class ReviewCriarDto
{
    public string? IdRestaurante { get; set; }
    public string? NomeRestaurante { get; set; }
    public int Nota { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public IList<IFormFile>? Fotos { get; set; }
}
