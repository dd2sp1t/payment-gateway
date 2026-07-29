# Payment Gateway

Решение тестового задания Fintech Dev Lab.

## Требования

- Docker
- Docker Compose v2

Дополнительные зависимости не требуются.

---

# Запуск

Клонировать репозиторий:

```bash
git clone https://github.com/dd2sp1t/payment-gateway.git
cd <repo>
```

Запустить все сервисы:

```bash
docker compose up --build
```

При первом запуске будут автоматически:

- собран `candidate-service`;
- создана база данных PostgreSQL;
- применены миграции EF Core;
- запущен Provider Simulator;
- запущены Seq, Prometheus и Grafana.

---

## Доступные сервисы

### Candidate Service (8080)

- API — http://localhost:8080
- Swagger UI — http://localhost:8080/swagger
- Health Check — http://localhost:8080/health
- Prometheus Metrics — http://localhost:8080/metrics

### Infrastructure

- Provider Simulator — http://localhost:8081
- Seq — http://localhost:5341
- Prometheus — http://localhost:9090
- Grafana — http://localhost:3000

Grafana credentials:

- **Username:** `admin`
- **Password:** `admin`

---

# Архитектура

Проект реализован по принципам Clean Architecture.

Состав решения:

- PaymentGateway.Api
- PaymentGateway.Application
- PaymentGateway.Domain
- PaymentGateway.Infrastructure

Используемые технологии:

- ASP.NET Core 10
- Entity Framework Core
- PostgreSQL
- Docker Compose
- Serilog
- Seq
- Prometheus
- Grafana

---

---

# Интеграционные тесты

Проект содержит набор интеграционных тестов, покрывающих основные сценарии работы платежного шлюза.

## Запуск всех тестов

```bash
dotnet test
```

## Запуск отдельных сценариев

### Базовый жизненный цикл операции

```bash
dotnet test --filter BasicFlowTests
```

### Идемпотентность обработки callback'ов

```bash
dotnet test --filter CallbackIdempotencyTests
```

### Callback раньше ответа провайдера

```bash
dotnet test --filter EarlyCallbackTests
```

### Конкурентный submit одной операции

```bash
dotnet test --filter SubmitConcurrencyTests
```

## Что проверяется

- успешное завершение операции;
- отклонение операции;
- идемпотентность callback'ов;
- callback, пришедший раньше ответа провайдера;
- конкурентный submit одной операции;
- неизменяемость финального состояния операции после любых последующих действий.

Во всех сценариях также проверяется корректная последовательность доменных событий.

---

# Остановка

Остановить контейнеры:

```bash
docker compose down
```

Удалить контейнеры вместе с данными:

```bash
docker compose down -v
```