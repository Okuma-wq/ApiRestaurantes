using System;
using System.Collections.Generic;

namespace AvaliacaoRestaurantesAPI.DTOs;

public class ReviewAlterarDto
{
    public int Nota { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateTime Data { get; set; } = DateTime.UtcNow;
}
