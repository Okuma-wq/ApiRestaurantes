using System.Text.Json.Serialization;

namespace AvaliacaoRestaurantesAPI.DTOs
{
    public class GoogleLoginDto
    {
        [JsonPropertyName("iss")]
        public string Iss { get; set; } = string.Empty;

        [JsonPropertyName("azp")]
        public string Azp { get; set; } = string.Empty;

        [JsonPropertyName("aud")]
        public string Aud { get; set; } = string.Empty;

        [JsonPropertyName("sub")]
        public string Sub { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("at_hash")]
        public string AtHash { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("picture")]
        public string Picture { get; set; } = string.Empty;

        [JsonPropertyName("given_name")]
        public string GivenName { get; set; } = string.Empty;

        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; } = string.Empty;

        [JsonPropertyName("locale")]
        public string Locale { get; set; } = string.Empty;

        [JsonPropertyName("iat")]
        public long Iat { get; set; }

        [JsonPropertyName("exp")]
        public long Exp { get; set; }
    }
}
