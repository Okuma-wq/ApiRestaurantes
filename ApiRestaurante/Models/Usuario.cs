using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string? Id { get; set; }

        [BsonElement("nome")]
        public string Nome { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("senha")]
        public string Senha { get; set; } = string.Empty;

        [BsonElement("foto")]
        public string Foto { get; set; } = string.Empty;

        [BsonElement("data_cadastro")]
        public DateTime DataCadastro { get; set; }

        [BsonElement("favoritos")]
        public List<string?> Favoritos { get; set; } = new();
    }
}
