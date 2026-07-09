# FIAP Cloud Games - UsersAPI

Microsservico responsavel por usuarios, autenticacao, geracao de JWT e autorizacao.

## Status

Base criada para receber a migracao do contexto de usuarios do monolito Fiap Cloud Games.

## Responsabilidades

- Cadastrar usuarios.
- Autenticar usuarios.
- Gerar JWT.
- Controlar autorizacao por roles.
- Publicar `UserCreatedEvent` apos cadastro.

## Estrutura

```text
src/UsersAPI/
  Domain/          Regras e entidades de usuario.
  Application/     Casos de uso e abstracoes.
  Infrastructure/  EF Core, repositorios, JWT e integracoes tecnicas.
  Contracts/       Requests, responses e eventos de integracao.
  Controllers/     Endpoints HTTP.
k8s/               Manifestos Kubernetes do servico.
```

## Variaveis de ambiente previstas

| Variavel | Finalidade |
|---|---|
| `ConnectionStrings__DefaultConnection` | Banco SQL Server da UsersAPI. |
| `Jwt__Issuer` | Emissor do token JWT. |
| `Jwt__Audience` | Audiencia do token JWT. |
| `Jwt__Secret` | Chave de assinatura do token JWT. |
| `Jwt__ExpirationMinutes` | Tempo de expiracao do token. |
| `RabbitMq__Host` | Host do RabbitMQ. |
| `RabbitMq__VirtualHost` | Virtual host do RabbitMQ. |
| `RabbitMq__Username` | Usuario do RabbitMQ. |
| `RabbitMq__Password` | Senha do RabbitMQ. |

## Executar localmente

```powershell
dotnet run --project src\UsersAPI\UsersAPI.csproj
```

Swagger em ambiente de desenvolvimento:

```text
https://localhost:<porta>/swagger
```

## Docker

```powershell
docker build -t fiap-cloud-games-users-api:latest .
```

## Kubernetes

Os manifests serao adicionados em `k8s/` nas proximas etapas.

```powershell
kubectl apply -f .\k8s
kubectl get pods
kubectl logs deployment/users-api
```
