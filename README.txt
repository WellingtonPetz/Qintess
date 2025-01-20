MultiAccountBankAPI

A MultiAccountBankAPI é uma API desenvolvida para permitir a integração e a gestão de múltiplas contas bancárias. A API oferece funcionalidades para autenticação, manipulação de dados bancários, e consultas de informações financeiras de forma segura e eficiente.
Tecnologias Utilizadas

    ASP.NET Core 6/7 (para o desenvolvimento da API)

    JWT (JSON Web Token) (para autenticação e segurança)

    Entity Framework Core (para interações com o banco de dados)

    SQL Server/PostgreSQL/MySQL (dependendo do banco de dados escolhido)

    Swagger (para documentação da API)

Funcionalidades

    Autenticação JWT: O sistema utiliza tokens JWT para autenticação, garantindo que apenas usuários autorizados possam acessar as funcionalidades da API.

    Gerenciamento de Contas: Permite o gerenciamento de múltiplas contas bancárias, incluindo a consulta e o processamento de transações.

    Endpoints de Consulta: Endpoints RESTful para consultar saldos, extratos, transferências e outras operações bancárias.

    Segurança e Validação: Implementação de práticas recomendadas de segurança, como criptografia de dados e validação de entradas.

Endpoints
1. Autenticação

POST /api/auth/login Autentica o usuário e retorna um token JWT.

Exemplo de corpo da requisição:
{
  "username": "user_example",
  "password": "password_example"
}
Resposta
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}

2. Consultar Contas

GET /api/accounts Retorna todas as contas do usuário autenticado.

Exemplo de resposta:
[
  {
    "accountId": 1,
    "accountNumber": "123456789",
    "balance": 2500.50,
    "currency": "BRL"
  },
  {
    "accountId": 2,
    "accountNumber": "987654321",
    "balance": 1500.00,
    "currency": "BRL"
  }
]

3. Realizar Transferência

POST /api/transactions/transfer Realiza uma transferência entre contas.

Exemplo de corpo da requisição:
{
  "fromAccountId": 1,
  "toAccountId": 2,
  "amount": 500.00
}
Resposta:
{
  "status": "success",
  "message": "Transferência realizada com sucesso."
}

Como Rodar o Projeto
Pré-requisitos

    .NET 6 ou superior
    SQL Server/PostgreSQL/MySQL (dependendo do banco de dados escolhido)
    Postman/Swagger para testes


Passos para rodar a API localmente

   1. Clone o repositório para o seu ambiente local.

      git clone https://github.com/seuusuario/MultiAccountBankAPI.git

  2. Navegue até o diretório do projeto:

    cd MultiAccountBankAPI

  3. Restaure as dependências do projeto:

    dotnet restore

  4. Atualize o arquivo appsettings.json com as informações de conexão do banco de dados.

  5. Execute a API localmente:

    dotnet run

  A API estará disponível no endereço http://localhost:7000.


Documentação

A documentação da API está disponível através do Swagger, acessível em:

  http://localhost:7000/swagger


