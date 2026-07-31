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

# Интеграционные тесты

Проект содержит набор интеграционных тестов, покрывающих основные сценарии работы.

## Запуск всех тестов

```bash
dotnet test
```

## Запуск отдельных сценариев

### Базовый жизненный цикл операции

- операция успешно завершается после подтверждения провайдера и callback со статусом `COMPLETED`;
- операция отклоняется после подтверждения провайдера и callback со статусом `REJECTED`.

```bash
dotnet test --filter BasicFlowTests
```

### Идемпотентность обработки callback'ов

- повторный callback со статусом `COMPLETED` игнорируется;
- повторный callback со статусом `REJECTED` игнорируется;
- после `COMPLETED` приходит `REJECTED` — создается событие `IGNORED`;
- конкурентные callback `COMPLETED` и `REJECTED` корректно обрабатываются, проигравший создает событие `IGNORED`.

```bash
dotnet test --filter CallbackIdempotencyTests
```

### Callback раньше ответа провайдера

- callback `COMPLETED` приходит раньше HTTP-ответа провайдера, терминальный статус сохраняется;
- callback `REJECTED` приходит раньше HTTP-ответа провайдера, терминальный статус сохраняется.

```bash
dotnet test --filter EarlyCallbackTests
```

### Несовпадающий ProviderPaymentId

- callback с другим `ProviderPaymentId` отклоняется с ошибкой `409 Conflict`;
- submit с другим `ProviderPaymentId` после callback безопасно завершается.

```bash
dotnet test --filter ProviderPaymentIdMismatchTests
```

### Конкурентный submit одной операции

- несколько одновременных submit-запросов приводят только к одной отправке операции провайдеру.

```bash
dotnet test --filter SubmitConcurrencyTests
```

---

# Наблюдаемость

## Логи

Все структурированные логи доступны в Seq:

- http://localhost:5341

Позволяют проследить полный жизненный цикл операции, взаимодействие с провайдером и диагностику ошибок.

## Метрики

Приложение публикует Prometheus-метрики:

- http://localhost:8080/metrics

Сбор осуществляется Prometheus:

- http://localhost:9090

## Grafana

Готовый дашборд доступен по адресу:

- http://localhost:3000

В Grafana уже настроена панель **Payment Gateway Infrastructure Diagnostics**, содержащая метрики по:

### Операциям

- количество созданных операций;
- количество отправленных операций;
- количество успешно завершённых операций;
- количество отклонённых операций;
- количество повторных отправок операций (retry) при временных ошибках провайдера;
- динамика жизненного цикла операций.

### Application Layer

Отслеживаются запросы уровня Application (MediatR commands/queries):

- количество выполненных запросов;
- latency (p50 / p95 / average);
- throughput;
- heatmap времени выполнения;
- количество автоматических повторных выполнений команд при возникновении конфликтов оптимистичной блокировки (с разбивкой по типу команды).

### Dispatch

- длительность обработки batch'ей;
- размер batch'ей;
- возраст самой старой операции в состоянии `PROCESSING`.

### Интеграции с Provider

- успешность отправок;
- latency запросов к провайдеру;
- timeline успешных и ошибочных dispatch;
- heatmap времени выполнения запросов к провайдеру.

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