# 🍽️ RestaurantesAPI

API REST desenvolvida com ASP.NET Core para gerenciar dados de restaurantes.

---

## ✅ Pré-requisitos

Antes de executar o projeto, certifique-se de ter instalado:

- [.NET 6 SDK ou superior](https://dotnet.microsoft.com/download)
- Uma instância do **MongoDB** (local ou na nuvem, como [MongoDB Atlas](https://www.mongodb.com/cloud/atlas))
- Um editor como [Visual Studio](https://visualstudio.microsoft.com/) ou [Visual Studio Code](https://code.visualstudio.com/)

---

## ⚙️ Configuração

### 1. Banco de Dados (MongoDB)

Crie um banco de dados no MongoDB com o nome:

dados_restaurante


No caso do MongoDB Atlas, obtenha a **string de conexão** no painel do cluster e insira no `appsettings.json` conforme o exemplo abaixo.

### 2. Autenticação (JWT)

A API utiliza autenticação baseada em JWT. Você deve configurar os seguintes valores no `appsettings.json`:

- `Key`: Chave secreta (mínimo recomendado: 32 caracteres)
- `Issuer`: Nome da entidade emissora do token (ex: RestaurantesAPI)
- `Audience`: Nome da audiência alvo (ex: RestaurantesAPI)

---

## 🛠️ Exemplo de `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDB": {
    "ConnectionString": "mongodb+srv://<usuario>:<senha>@<host>/",
    "DatabaseName": "dados_restaurante"
  },
  "Jwt": {
    "Key": "sua-chave-super-secreta-aqui",
    "Issuer": "RestaurantesAPI",
    "Audience": "RestaurantesAPI"
  }
}
