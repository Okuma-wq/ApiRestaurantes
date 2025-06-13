using System;
using System.Collections.Generic;

namespace AvaliacaoRestaurantesAPI.DTOs;

public class ReviewCriarDto
{
    public string? IdRestaurante { get; set; }
    public string? IdUsuario { get; set; }
    public int Nota { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateTime Data { get; set; } = DateTime.UtcNow;
}
