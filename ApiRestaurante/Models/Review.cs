using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AvaliacaoRestaurantesAPI.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string? Id { get; set; }

        [BsonElement("id_restaurante")]
        [BsonRepresentation(BsonType.String)]
        public string? IdRestaurante { get; set; }

        [BsonElement("id_usuario")]
        [BsonRepresentation(BsonType.String)]
        public string? IdUsuario { get; set; }

        [BsonElement("nota")]
        public int Nota { get; set; }

        [BsonElement("comentario")]
        public string Comentario { get; set; } = string.Empty;

        [BsonElement("data")]
        public DateTime Data { get; set; }

        [BsonElement("fotos")]
        public List<string?> Fotos { get; set; } = new List<string?>();
    }
}