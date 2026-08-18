# Pedidos

Aplicação para criação e consulta de pedidos, composta por uma API REST em .NET 8, com persistência em SQL Server, e um frontend Angular. O backend utiliza uma arquitetura em camadas baseada em Clean Architecture, aplicando conceitos de DDD, SOLID e Clean Code de forma proporcional ao escopo do desafio.

## Estrutura da solution

```text
Pedidos.sln
├── src/
│   ├── Pedidos.Api
│   ├── Pedidos.Application
│   ├── Pedidos.Domain
│   └── Pedidos.Infrastructure
├── tests/
│   └── Pedidos.UnitTests
└── frontend/
    └── pedidos-web
```

- **Pedidos.Api**: controllers, configuração HTTP, Swagger, injeção de dependência e tratamento centralizado de exceções.
- **Pedidos.Application**: casos de uso, validações, DTOs, contratos de serviços e interfaces de persistência.
- **Pedidos.Domain**: entidades e comportamentos do domínio de pedidos, sem dependências de infraestrutura.
- **Pedidos.Infrastructure**: Entity Framework Core, SQL Server, `DbContext`, mappings, migrations e repositories.
- **Pedidos.UnitTests**: testes unitários de controller e service, sem acesso a banco de dados.
- **pedidos-web**: aplicação Angular standalone com Reactive Forms para criar e consultar pedidos.

As dependências seguem a direção `Api → Application`, `Infrastructure → Application` e `Application → Domain`. A API referencia Infrastructure somente para compor as dependências da aplicação.

## Principais decisões técnicas

### Requisitos atendidos do desafio

- API ASP.NET Core em .NET 8.
- SQL Server com Entity Framework Core e migration inicial.
- Swagger/OpenAPI.
- Criação e consulta de pedidos.
- Testes unitários do GET no controller e no service.
- Retorno do GET compatível com o contrato solicitado.
- Preço histórico por item de pedido.
- Respostas de erro padronizadas com códigos HTTP apropriados.
- Frontend Angular para criação e consulta de pedidos, incluindo validações e tratamento dos estados de carregamento, sucesso e erro.

### Decisões de implementação

- `Pedido` é o aggregate root de `Pedido` e seus `ItemPedido`.
- `Produto.Valor` representa o preço atual do produto.
- `ItemPedido.ValorUnitario` persiste uma cópia do preço praticado no momento da criação do pedido.
- `ValorTotal` é calculado por `ValorUnitario × Quantidade` e não é persistido.
- Entidades de domínio não são expostas diretamente pela API; request e response usam DTOs próprios.
- O mapeamento entre entidades e DTOs é manual.
- Foram adotados repositories específicos para Pedido e Produto.
- Não foram adicionados `GenericRepository`, MediatR, CQRS, AutoMapper ou Unit of Work customizada.
- Erros são representados com `ProblemDetails`; exceções inesperadas são tratadas centralmente e não expõem stack trace.
- Operações de I/O são assíncronas e recebem `CancellationToken`.
- Uma única chamada a `SaveChangesAsync` persiste Pedido e seus itens, sem transação manual adicional.

## Decisões sobre ambiguidades do enunciado

Os pontos abaixo são escolhas realizadas para tornar o comportamento explícito; não devem ser interpretados como requisitos textuais do desafio:

- A consulta obrigatória foi implementada como `GET /api/pedidos/{id}`.
- `ItemPedido.Id` é a chave primária identity. `IdPedido` e `IdProduto` são foreign keys, sem chave primária composta.
- O POST aceita somente `NomeCliente`, `EmailCliente`, `Pago`, `ProdutoId` e `Quantidade`.
- `DataCriacao`, `ValorUnitario`, identificadores e valores calculados são definidos pelo servidor.
- O mesmo `ProdutoId` repetido no payload de criação resulta em HTTP 400; não há consolidação implícita de itens.
- Produto inexistente durante a criação resulta em HTTP 400.
- Três produtos iniciais são disponibilizados por seed para viabilizar a demonstração sem CRUD de Produto.
- O nome do produto retornado vem do cadastro atual; o requisito histórico é aplicado ao preço.

## Como executar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Docker Desktop com Docker Compose
- Node.js 24.15 ou superior dentro da linha 24, com npm

### Banco de dados

Na raiz do projeto, copie o arquivo de exemplo e defina uma senha forte para o usuário `sa`:

```powershell
Copy-Item .env.example .env
notepad .env
```

Mantenha as aspas simples e substitua somente o placeholder. O `.env` está ignorado pelo Git e não deve ser versionado.

Suba o SQL Server:

```powershell
docker compose up -d
docker compose ps
```

Restaure a ferramenta local do EF Core:

```powershell
dotnet tool restore
```

Carregue temporariamente a connection string a partir do `.env`:

```powershell
$envEntry = Get-Content .env |
    Where-Object { $_ -match '^SQLSERVER_SA_PASSWORD=' } |
    Select-Object -First 1

$saPassword = (($envEntry -split '=', 2)[1]).Trim().Trim("'")
$env:ConnectionStrings__PedidosDatabase = "Server=localhost,1433;Database=PedidosDb;User Id=sa;Password=$saPassword;TrustServerCertificate=True"
```

Aplique a migration existente:

```powershell
dotnet ef database update `
    --project src\Pedidos.Infrastructure\Pedidos.Infrastructure.csproj `
    --startup-project src\Pedidos.Api\Pedidos.Api.csproj `
    --context PedidosDbContext
```

Execute a API em Development, mantendo a variável de ambiente configurada na mesma sessão:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src\Pedidos.Api\Pedidos.Api.csproj
```

Com o profile HTTP do projeto, a API fica disponível em `http://localhost:5008`. Em outra sessão do terminal, instale as dependências e execute o frontend:

```powershell
Set-Location frontend\pedidos-web
npm install
npm start
```

O Angular fica disponível em `http://localhost:4200` e utiliza a API em `http://localhost:5008`. Em ambiente de desenvolvimento, a API aplica uma policy CORS nomeada que permite somente a origem `http://localhost:4200`; não há liberação global de origens.

Ao encerrar, as variáveis temporárias podem ser removidas:

```powershell
Remove-Item Env:\ConnectionStrings__PedidosDatabase -ErrorAction SilentlyContinue
Remove-Item Env:\ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
Remove-Variable saPassword, envEntry -ErrorAction SilentlyContinue
```

## Endpoints

### Criar pedido

```http
POST /api/pedidos
Content-Type: application/json
```

Request:

```json
{
  "nomeCliente": "Cliente Teste",
  "emailCliente": "cliente@email.com",
  "pago": false,
  "itens": [
    {
      "produtoId": 1,
      "quantidade": 2
    }
  ]
}
```

Em caso de sucesso, retorna `201 Created`, inclui o header `Location` apontando para o GET e devolve o pedido criado.

### Consultar pedido

```http
GET /api/pedidos/{id}
```

Response `200 OK`:

```json
{
  "id": 1,
  "nomeCliente": "Cliente Teste",
  "emailCliente": "cliente@email.com",
  "pago": false,
  "valorTotal": 7000.00,
  "itensPedido": [
    {
      "id": 1,
      "idProduto": 1,
      "nomeProduto": "Notebook",
      "valorUnitario": 3500.00,
      "quantidade": 2
    }
  ]
}
```

Pedido inexistente retorna `404 Not Found` com `ProblemDetails`. Payload inválido ou regra de criação violada retorna `400 Bad Request`.

### Atualizar pedido

```http
PUT /api/pedidos/{id}
Content-Type: application/json
```

Atualiza `nomeCliente`, `emailCliente`, `pago` e `itens` usando a mesma estrutura de payload da criação. Retorna `200 OK` com o pedido atualizado ou `404 Not Found` quando o pedido não existe.

O `ValorUnitario` histórico é preservado para produtos que já pertenciam ao pedido, inclusive quando sua quantidade é alterada. Produtos adicionados durante a atualização usam o valor atual cadastrado em `Produto`.

### Remover pedido

```http
DELETE /api/pedidos/{id}
```

Remove o pedido e seus `ItensPedido`, que são excluídos por cascade. Retorna `204 No Content` quando removido ou `404 Not Found` quando o pedido não existe.

## Swagger

Em ambiente `Development`, usando o profile HTTP do projeto, o Swagger UI fica disponível em `http://localhost:5008/swagger`.

## Testes

Execute a suíte com:

```powershell
dotnet test
```

Para executar os testes padrão do frontend:

```powershell
Set-Location frontend\pedidos-web
npm test
```

Atualmente existem 7 testes unitários:

- GET no `PedidoService`: pedido existente e inexistente.
- GET no `PedidosController`: respostas 200 e 404 com `ProblemDetails`.
- POST no `PedidoService`: criação válida, produto inexistente e produto duplicado.
- Cenário explícito no qual `Produto.Valor` difere de `ItemPedido.ValorUnitario`, garantindo que o total utiliza o preço histórico.

Os testes usam mocks dos contratos necessários e não acessam SQL Server, EF Core InMemory ou `PedidosDbContext`.

## Validações realizadas

O fluxo foi validado manualmente com:

- migration aplicada em SQL Server 2022 executado via Docker;
- seed dos três produtos iniciais;
- POST de pedido retornando HTTP 201;
- GET de pedido existente retornando HTTP 200 e o contrato esperado;
- GET de pedido inexistente retornando HTTP 404;
- POST inválido retornando HTTP 400;
- alteração posterior de `Produto.Valor` sem mudança em `ItemPedido.ValorUnitario` ou `ValorTotal` de um pedido existente.

## Escopo atual

Estão implementados:

- `POST /api/pedidos`;
- `GET /api/pedidos/{id}`;
- `PUT /api/pedidos/{id}`;
- `DELETE /api/pedidos/{id}`;
- frontend Angular em `frontend/pedidos-web` para criação e consulta de pedidos.

Ainda não estão implementados nesta versão:

- paginação;
- CRUD de Produto.
