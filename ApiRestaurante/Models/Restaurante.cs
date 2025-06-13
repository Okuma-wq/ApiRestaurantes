using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AvaliacaoRestaurantesAPI.Models
{
    public class Restaurante
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string? Id { get; set; }

        [BsonElement("nome")]
        public string Nome { get; set; } = string.Empty;

        [BsonElement("endereco")]
        public Endereco Endereco { get; set; } = new Endereco();

        [BsonElement("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [BsonElement("avaliacao_media")]
        public double AvaliacaoMedia { get; set; }

        [BsonElement("foto")]
        public string Foto { get; set; } = string.Empty;

        [BsonElement("telefone")]
        public string Telefone { get; set; } = string.Empty;

        [BsonElement("faixa_preco")]
        public string FaixaPreco { get; set; } = string.Empty;

        [BsonElement("comodidades")]
        public Comodidades Comodidades { get; set; } = new Comodidades();
    }

    public class Endereco
    {
        [BsonElement("rua")]
        public string Rua { get; set; } = string.Empty;

        [BsonElement("numero")]
        public int Numero { get; set; }

        [BsonElement("cidade")]
        public string Cidade { get; set; } = string.Empty;

        [BsonElement("estado")]
        public string Estado { get; set; } = string.Empty;
    }

    public class Comodidades
    {
        [BsonElement("wifi")]
        public bool Wifi { get; set; }

        [BsonElement("estacionamento")]
        public bool Estacionamento { get; set; }

        [BsonElement("acessibilidade")]
        public bool Acessibilidade { get; set; }

        [BsonElement("ar_condicionado")]
        public bool ArCondicionado { get; set; }

        [BsonElement("espaco_kids")]
        public bool EspacoKids { get; set; }

        [BsonElement("pet_friendly")]
        public bool PetFriendly { get; set; }

        [BsonElement("musica_ao_vivo")]
        public bool MusicaAoVivo { get; set; }

        [BsonElement("delivery")]
        public bool Delivery { get; set; }

        [BsonElement("bebidas_alcoolicas")]
        public bool BebidasAlcoolicas { get; set; }

        [BsonElement("vegano_vegetariano")]
        public bool VeganoVegetariano { get; set; }

        [BsonElement("reserva_online")]
        public bool ReservaOnline { get; set; }

        [BsonElement("ambiente_externo")]
        public bool AmbienteExterno { get; set; }
    }
}
