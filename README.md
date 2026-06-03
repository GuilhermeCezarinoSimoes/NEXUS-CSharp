# NEXUS — Backend C# · Global Solution FIAP 2025

> **Sistema Operacional para Ambientes Extremos** — bases na Antártida, plataformas offshore e bases militares isoladas.
> Inspirado na resiliência e autonomia da ISS (Estação Espacial Internacional).

---

## Integrantes

| Nome | RM |
|---|---|
| Guilherme Cezarino Simões | RM560539 |
| Fabrini | RM560207 |

---

## Descrição do Projeto

O **NEXUS** é um backend de microsserviços desenvolvido em **ASP.NET Core (.NET 10)** para monitorar em tempo real os dados vitais de operadores e sensores em bases remotas de ambientes extremos.

O sistema é composto por dois microsserviços desacoplados que se comunicam via **RabbitMQ**:

- **TelemetryAPI** — recebe dados de saúde dos operadores (temperatura, batimentos cardíacos, SpO2) e de energia da base (nível de bateria, voltagem, status do gerador). Detecta anomalias e publica alertas na fila de mensageria.
- **AlertsAPI** — consome a fila de alertas do RabbitMQ, registra ocorrências no banco de dados e gerencia protocolos de emergência.

---

## Arquitetura

```
[Sensores IoT / Mobile App]
          │
          ▼
 ┌─────────────────────┐      Fila RabbitMQ       ┌─────────────────────┐
 │   TelemetryAPI      │  ──── nexus.alerts ────►  │     AlertsAPI       │
 │   (porta 5256)      │                            │   (porta 5080)      │
 │                     │                            │                     │
 │  POST /api/v1/      │                            │  GET  /api/v1/      │
 │  Telemetria/saude   │                            │  Alertas/active     │
 │  POST /api/v1/      │                            │  PATCH /api/v1/     │
 │  Energia            │                            │  Alertas/{id}/ack   │
 └──────┬──────────────┘                            └──────┬──────────────┘
        │                                                  │
        ▼                                                  ▼
  NEXUS_Telemetry (SQL Server / InMemory dev)    NEXUS_Alerts (SQL Server / InMemory dev)
```

### Fluxo de Anomalia

1. Operador publica leitura via `POST /api/v1/Telemetria/saude`
2. `TelemetryService` valida, persiste e chama `IsAnomaly()` no modelo
3. Se anomalia → `RabbitMQPublisher` publica `AlertMessage` (struct) na fila `nexus.alerts`
4. `RabbitMQConsumer` (BackgroundService na AlertsAPI) consome a mensagem
5. `AlertService` deserializa o `AlertPayload` (struct) e registra o `Alert` no banco
6. Se 3+ alertas críticos na mesma base em 15 min → Protocolo de Emergência acionado

---

## Estrutura de Pastas

```
GS_C#/
├── src/
│   ├── TelemetryAPI/
│   │   ├── Controllers/
│   │   │   ├── TelemetryController.cs     # Endpoints de saúde do operador
│   │   │   └── EnergyController.cs        # Endpoints de energia da base
│   │   ├── Data/
│   │   │   └── TelemetryDbContext.cs      # EF Core DbContext
│   │   ├── Interfaces/
│   │   │   ├── ITelemetryRepository.cs    # Contrato do repositório de telemetria
│   │   │   ├── IEnergyRepository.cs       # Contrato do repositório de energia
│   │   │   └── IMessagePublisher.cs       # Contrato do publisher RabbitMQ
│   │   ├── Messaging/
│   │   │   ├── RabbitMQPublisher.cs       # Publica alertas na fila
│   │   │   └── NullMessagePublisher.cs    # Null Object Pattern (sem broker)
│   │   ├── Migrations/                    # EF Core migrations
│   │   ├── Models/
│   │   │   ├── SensorBase.cs              # Classe ABSTRATA base (herança)
│   │   │   ├── TelemetryReading.cs        # Herda SensorBase — saúde do operador
│   │   │   ├── EnergyReading.cs           # Herda SensorBase — energia da base
│   │   │   └── Structs/
│   │   │       └── AlertMessage.cs        # STRUCT para mensagem RabbitMQ
│   │   ├── Repositories/
│   │   │   ├── TelemetryRepository.cs     # Repository Pattern — EF Core
│   │   │   └── EnergyRepository.cs        # Repository Pattern — EF Core
│   │   └── Services/
│   │       ├── TelemetryService.cs        # Orquestração (PARTIAL CLASS)
│   │       └── TelemetryService.Validation.cs  # Validações (PARTIAL CLASS)
│   │
│   └── AlertsAPI/
│       ├── Controllers/
│       │   └── AlertsController.cs        # Endpoints de alertas e emergências
│       ├── Data/
│       │   └── AlertsDbContext.cs         # EF Core DbContext
│       ├── Interfaces/
│       │   └── IAlertRepository.cs        # Contrato do repositório de alertas
│       ├── Messaging/
│       │   └── RabbitMQConsumer.cs        # BackgroundService — consome fila
│       ├── Migrations/                    # EF Core migrations
│       ├── Models/
│       │   ├── AlertBase.cs               # Classe ABSTRATA base
│       │   ├── Alert.cs                   # Herda AlertBase — alerta registrado
│       │   ├── EmergencyIncident.cs       # Herda AlertBase — incidente múltiplo
│       │   └── Structs/
│       │       └── AlertPayload.cs        # STRUCT para payload RabbitMQ
│       ├── Repositories/
│       │   └── AlertRepository.cs         # Repository Pattern — EF Core
│       └── Services/
│           └── AlertService.cs            # Lógica de alertas e emergências
│
└── tests/
    └── NEXUS.Tests/
        ├── TelemetryServiceTests.cs       # 6 testes com Moq (mocks de infra)
        └── SensorModelTests.cs            # 17 testes de lógica pura (sem mocks)
```

---

## Critérios da Rubrica Atendidos

| Critério | Pontos | Implementação |
|---|---|---|
| **Modelagem de Domínio & POO** | 20pts | `SensorBase` → `TelemetryReading`, `EnergyReading`; `AlertBase` → `Alert`, `EmergencyIncident`. Métodos `override`: `IsAnomaly()`, `GetAlertDescription()`, `GetSensorType()`, `RequiresImmediateAction()` |
| **Abstração e Interfaces** | 20pts | `SensorBase` e `AlertBase` abstratas; 4 interfaces com DI: `ITelemetryRepository`, `IEnergyRepository`, `IMessagePublisher`, `IAlertRepository` |
| **Lógica, Métodos e Datas** | 15pts | `IsAnomaly()`, `GetSeverity()`, `IsDataStale()`, `GetDataAge()`, filtros por período com `DateTime`, `GetResponseTime()` |
| **Tratamento de Exceções** | 10pts | `ArgumentException`, `ArgumentOutOfRangeException`, `KeyNotFoundException`, `InvalidOperationException` com mensagens específicas |
| **Structs e Partial** | 5pts | `AlertMessage` e `AlertPayload` são structs; `TelemetryService` é partial class em 2 arquivos |
| **Organização** | 30pts | Estrutura de pastas consistente, nomenclatura NEXUS em todo o projeto, README detalhado, 23 testes passando |

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- *(Opcional)* Docker Desktop — para SQL Server e RabbitMQ reais

---

## Como Executar

### Sem Docker (modo desenvolvimento — banco InMemory)

**Terminal 1 — TelemetryAPI:**
```powershell
cd src/TelemetryAPI
dotnet run
# Swagger: http://localhost:5256
```

**Terminal 2 — AlertsAPI:**
```powershell
cd src/AlertsAPI
dotnet run
# Swagger: http://localhost:5080
```

### Com Docker (banco e mensageria reais)

```powershell
docker compose up -d sqlserver rabbitmq
# Depois rode as duas APIs com: dotnet run
```

### Rodar os Testes

```powershell
cd tests/NEXUS.Tests
dotnet test --verbosity normal
# Resultado: 23/23 aprovados
```

---

## Exemplos de Uso via Swagger

### Registrar leitura normal de saúde

```json
POST /api/v1/Telemetria/saude
{
  "sensorId": "BIO-001",
  "operatorId": "OP-Guilherme",
  "baseLocation": "Base-Antarctica-Alpha",
  "temperature": 22.5,
  "heartRate": 72,
  "oxygenLevel": 99.0
}
```
**Resposta:** HTTP 201 — leitura salva, sem anomalia.

### Registrar anomalia crítica (dispara alerta)

```json
POST /api/v1/Telemetria/saude
{
  "sensorId": "BIO-002",
  "operatorId": "OP-Fabrini",
  "baseLocation": "Base-Antarctica-Alpha",
  "temperature": -65.0,
  "heartRate": 28,
  "oxygenLevel": 82.0
}
```
**Resposta:** HTTP 201 — anomalia detectada, alerta publicado no RabbitMQ → registrado na AlertsAPI.

### Simular queda total de energia

```
POST /api/v1/Energia/simular-emergencia
```

### Verificar status dos alertas

```
GET /api/v1/Alertas/status
```

### Confirmar alerta (operador assume responsabilidade)

```
PATCH /api/v1/Alertas/1/acknowledge?operatorId=OP-Guilherme
```

---

## Tecnologias Utilizadas

| Tecnologia | Versão | Uso |
|---|---|---|
| ASP.NET Core | .NET 10 | Framework dos microsserviços |
| Entity Framework Core | 10.0.8 | ORM + Repository Pattern |
| SQL Server / InMemory | — | Persistência (prod / dev) |
| RabbitMQ.Client | 7.2.1 | Mensageria assíncrona |
| Swashbuckle (Swagger) | 10.1.7 | Documentação das APIs |
| xUnit | 2.9.3 | Framework de testes |
| Moq | 4.20.72 | Mocks para testes unitários |

---

## Evidências de Execução

### TelemetryAPI — Swagger UI
![TelemetryAPI Swagger](docs/telemetry-swagger.png)

### AlertsAPI — Swagger UI
![AlertsAPI Swagger](docs/alerts-swagger.png)

---

*Projeto desenvolvido para a Global Solution · FIAP · Tema: Space Connect · 2025*
