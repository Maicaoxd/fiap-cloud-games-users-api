# FIAP Cloud Games - UsersAPI

Microsservico responsavel por cadastro, autenticacao, geracao de JWT e autorizacao de usuarios da FIAP Cloud Games.

Este repositorio faz parte da Fase 2 do Tech Challenge e representa o microsservico independente de usuarios.

## Responsabilidades

- Cadastrar usuarios.
- Autenticar usuarios com e-mail e senha.
- Gerar token JWT.
- Controlar autorizacao por roles.
- Persistir usuarios em banco SQL Server proprio.
- Publicar `UserCreatedEvent` apos cadastro.

## Tecnologias

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- RabbitMQ
- MassTransit
- JWT Bearer
- Docker
- xUnit, NSubstitute e Shouldly

## Estrutura

```text
src/UsersAPI/
  Domain/          Entidades, value objects e regras de dominio.
  Application/     Casos de uso, comandos, resultados e abstracoes.
  Infrastructure/  EF Core, repositorios, seguranca, JWT e mensageria.
  Contracts/       Requests, responses e eventos de integracao.
  Controllers/     Endpoints HTTP.
  Migrations/      Migrations EF Core do banco da UsersAPI.

tests/UsersAPI.Tests/
  Domain/          Testes de dominio e value objects.
  Application/     Testes dos casos de uso.
  Infrastructure/  Testes de JWT e hash de senha.
  Api/             Testes de controllers, middlewares e configuracoes.

k8s/               Manifestos Kubernetes do servico.
```

## Variaveis de ambiente

| Variavel | Finalidade |
|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string do banco SQL Server da UsersAPI. |
| `Jwt__Issuer` | Emissor do token JWT. |
| `Jwt__Audience` | Audiencia do token JWT. |
| `Jwt__Secret` | Chave usada para assinar o JWT. |
| `Jwt__ExpirationMinutes` | Tempo de expiracao do token. |
| RabbitMq__Host | Host do RabbitMQ. |
| RabbitMq__Port | Porta TCP usada pelo MassTransit. |
| `RabbitMq__VirtualHost` | Virtual host do RabbitMQ. |
| `RabbitMq__Username` | Usuario do RabbitMQ. |
| `RabbitMq__Password` | Senha do RabbitMQ. |

Configuracao local padrao em `src/UsersAPI/appsettings.json`:

```text
SQL Server: localhost,1433
Database: FiapCloudGamesUsers
User: sa
Password: Fcg@123456
RabbitMQ: localhost:5672
RabbitMQ Management: http://localhost:15672
```

Observacao: os segredos deste repositorio sao apenas para desenvolvimento local academico.

## Subir dependencias locais

O arquivo `docker-compose.dev.yml` sobe apenas as dependencias da UsersAPI:

- SQL Server
- RabbitMQ com Management UI

```powershell
cd C:\Projetos\FIAP\Projetos\fiap-cloud-games-users-api

docker compose -f docker-compose.dev.yml up -d
```

Validar containers:

```powershell
docker ps
```

RabbitMQ Management:

```text
http://localhost:15672
usuario: guest
senha: guest
```

Se precisar recriar o ambiente do zero:

```powershell
docker compose -f docker-compose.dev.yml down -v
docker compose -f docker-compose.dev.yml up -d
```

## Banco de dados

Criar/aplicar o banco a partir das migrations:

```powershell
dotnet ef database update --project src\UsersAPI\UsersAPI.csproj --startup-project src\UsersAPI\UsersAPI.csproj
```

Validar no SQL Server Management Studio:

```text
Server: localhost,1433
Authentication: SQL Server Authentication
Login: sa
Password: Fcg@123456
Trust server certificate: marcado
```

Queries uteis:

```sql
SELECT name FROM sys.databases ORDER BY name;

SELECT * FROM FiapCloudGamesUsers.dbo.__EFMigrationsHistory;

SELECT TABLE_SCHEMA, TABLE_NAME
FROM FiapCloudGamesUsers.INFORMATION_SCHEMA.TABLES
ORDER BY TABLE_SCHEMA, TABLE_NAME;

SELECT Id, Name, Email, Cpf, BirthDate, Role, IsActive, CreatedAt
FROM FiapCloudGamesUsers.dbo.Users;
```

### Seed do administrador

As migrations da UsersAPI criam um usuario administrador inicial para permitir testar endpoints protegidos por role `Administrator`.

Dados do administrador:

- e-mail: `admin@email.com`
- CPF: `52998224725`
- data de nascimento: `1990-01-01`
- role: `Administrator`

Caso precise definir uma senha localmente, use o endpoint `POST /api/auth/forgot-password` com os dados de recuperacao acima.

## Executar a API localmente

```powershell
dotnet run --project src\UsersAPI\UsersAPI.csproj
```

Swagger em ambiente de desenvolvimento:

```text
https://localhost:<porta>/swagger
http://localhost:<porta>/swagger
```

As portas exatas aparecem no terminal ou em `src/UsersAPI/Properties/launchSettings.json`.

## Endpoints principais

- `POST /api/users` - cadastra usuario e publica `UserCreatedEvent`.
- `GET /api/users` - lista usuarios, exige role `Administrator`.
- `PUT /api/users/me` - atualiza perfil do usuario autenticado.
- `PATCH /api/users/me/password` - altera senha do usuario autenticado.
- `PUT /api/users/{userId}` - atualizacao administrativa, exige role `Administrator`.
- `PATCH /api/users/{userId}/deactivate` - desativa usuario, exige role `Administrator`.
- `POST /api/auth/login` - autentica e retorna JWT.
- `POST /api/auth/forgot-password` - redefine senha com dados de recuperacao.
- `GET /health/live` - health check de liveness, valida se o processo da API esta de pe.
- `GET /health` - health check de readiness, valida se a API consegue conectar no SQL Server e no RabbitMQ.
- `GET /health/ready` - equivalente ao readiness check, util para Kubernetes.

## Exemplo de cadastro

```powershell
curl -X POST "http://localhost:<porta>/api/users" `
  -H "Content-Type: application/json" `
  -d '{
    "name": "Maicon Guedes",
    "email": "maicon@email.com",
    "cpf": "111.444.777-35",
    "birthDate": "1993-06-17",
    "password": "Senha@123",
    "confirmPassword": "Senha@123"
  }'
```

## Exemplo de login

```powershell
curl -X POST "http://localhost:<porta>/api/auth/login" `
  -H "Content-Type: application/json" `
  -d '{
    "email": "maicon@email.com",
    "password": "Senha@123"
  }'
```

## Evento publicado

### `UserCreatedEvent`

Publicado pela UsersAPI apos o cadastro de usuario.

```json
{
  "userId": "guid",
  "name": "string",
  "email": "string",
  "createdAt": "datetime"
}
```

A UsersAPI usa `Publish` do MassTransit. Isso publica o evento na exchange do tipo `UserCreatedEvent` no RabbitMQ.

Importante: enquanto nao existir um consumidor, como a futura NotificationsAPI, nao havera fila recebendo a mensagem. O evento pode aparecer como exchange/topologia, mas nao como mensagem parada em fila.

## Testes

Executar a suite:

```powershell
dotnet test UsersAPI.slnx -m:1
```

Estado atual:

```text
155 testes passando
```

Foram migrados testes do monolito relacionados a:

- dominio de usuarios;
- value objects de usuario;
- casos de uso de usuarios;
- controllers de usuario/autenticacao;
- JWT e hash de senha;
- middlewares e configuracoes da API.

Testes de jogos e biblioteca ficaram fora deste repositorio porque pertencem aos futuros microsservicos CatalogAPI e fluxo de compra.

## Docker da API

Build da imagem da UsersAPI:

```powershell
docker build -t maicaoxd/fiap-cloud-games-users-api:0.1.2 .
```

Para executar o ambiente completo com UsersAPI, CatalogAPI, PaymentsAPI, NotificationsAPI, RabbitMQ e bancos SQL Server, use o `docker-compose.yml` do repositorio `fiap-cloud-games-orchestration`.

## Kubernetes

Os manifests ficam em `k8s/` e contem:

- `Deployment`
- `Service`
- `ConfigMap`
- `Secret`
- `Job` de migration

Aplicar manifests deste servico:

```powershell
kubectl apply -k .\k8s
kubectl get pods -n fiap-cloud-games
kubectl get services -n fiap-cloud-games
kubectl logs deployment/users-api -n fiap-cloud-games
```





