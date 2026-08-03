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

Проект содержит интеграционные тесты, покрывающие основные сценарии работы.

## Запуск всех тестов

```bash
dotnet build
```
```bash
dotnet test --no-build
```

## Запуск отдельных сценариев

### BasicFlowTests

Проверяет базовый жизненный цикл операции:

- успешное завершение операции (`COMPLETED`);
- отклонение операции (`REJECTED`).

```bash
dotnet test --no-build --filter BasicFlowTests
```

### DuplicateOperationTests

Проверяет создание операций:

- повторное создание операции с тем же `OperationId` возвращает `409 Conflict`.

```bash
dotnet test --no-build --filter DuplicateOperationTests
```

### SubmitConcurrencyTests

Проверяет конкурентную отправку операции:

- при нескольких одновременных submit ровно один запрос создаёт намерение на отправку (`202 Accepted`);
- остальные запросы возвращают уже сохранённое состояние (`200 OK`).

```bash
dotnet test --no-build --filter SubmitConcurrencyTests
```

### DispatchRetryTests

Проверяет повторную отправку операции при временных ошибках провайдера.

Покрываются следующие временные ошибки:

- `503 Service Unavailable`;
- `504 Gateway Timeout`;
- `429 Too Many Requests`;
- `TimeoutException`;
- `SocketException`;
- `IOException`.

Проверяется, что:

- после временной ошибки операция остаётся в статусе `PROCESSING`;
- увеличивается счётчик попыток (`RetryCount`);
- планируется следующая отправка (`NextDispatchAt`);
- после успешной повторной отправки операция корректно завершается.

Отдельно проверяется, что:

- после достижения максимального количества попыток повторные отправки больше не планируются (`NextDispatchAt = null`).

```bash
dotnet test --no-build --filter DispatchRetryTests
```

### CallbackIdempotencyTests

Проверяет идемпотентность обработки callback'ов:

- повторный callback `COMPLETED` игнорируется;
- повторный callback `REJECTED` игнорируется;

```bash
dotnet test --no-build --filter CallbackIdempotencyTests
```

### ConflictingCallbacksTests

Проверяет обработку конфликтующих callback'ов:

- один результат становится терминальным;
- противоположный сохраняется как `IGNORED`.

```bash
dotnet test --no-build --filter ConflictingCallbacksTests
```

### EarlyCallbackTests

Проверяет получение callback раньше ответа провайдера:

- callback `COMPLETED` приходит раньше HTTP-ответа провайдера;
- callback `REJECTED` приходит раньше HTTP-ответа провайдера.

Во всех случаях операция остается в соответствующем терминальном статусе и не возвращается в `PROCESSING`.

```bash
dotnet test --no-build --filter EarlyCallbackTests
```

### ProviderPaymentIdValidationTests

Проверяет валидацию `ProviderPaymentId`:

- callback с несовпадающим `ProviderPaymentId` отклоняется (`409 Conflict`);
- ответ провайдера с несовпадающим `ProviderPaymentId` безопасно игнорируется, если операция уже завершена callback'ом.

```bash
dotnet test --no-build --filter ProviderPaymentIdValidationTests
```

---

# Demo Runner

Проект `tools/PaymentGateway.DemoRunner` предназначен для генерации нагрузки, наполнения логов и метрик (Grafana / Prometheus), а также демонстрации основных сценариев работы системы.

## Доступные сценарии

### Basic

Полный успешный сценарий работы системы:

- создание операции;
- отправка операции;
- ожидание обработки Provider;
- получение актуального состояния операции;
- получение receipt;
- получение истории событий.

```bash
dotnet run --project tools/PaymentGateway.DemoRunner -- basic
```

### Concurrent

Проверка optimistic concurrency при одновременной отправке одной операции несколькими конкурентными запросами.

```bash
dotnet run --project tools/PaymentGateway.DemoRunner -- concurrent
```

### Duplicate

Повторные попытки создания операции с одинаковым `OperationId`.

```bash
dotnet run --project tools/PaymentGateway.DemoRunner -- duplicate
```

### Validation

Генерация некорректных запросов для проверки валидации и обработки ошибок:

- неверные параметры CreateOperation;
- неверные параметры SubmitOperation;
- неверные callback'и ProcessReceipt;
- запросы к несуществующим операциям (404).

```bash
dotnet run --project tools/PaymentGateway.DemoRunner -- validation
```

### All

Последовательно выполняет все доступные сценарии.

```bash
dotnet run --project tools/PaymentGateway.DemoRunner -- all
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

Готовые дашборды доступны по адресу:

- http://localhost:3000

В Grafana настроены **4 отдельных дашборда**, каждый из которых покрывает свою область мониторинга и помещается на одном экране.

---

### 1. Payment Gateway - Operations

Дашборд отслеживает жизненный цикл операций:

**Overview**
- **Created Rate (5m)** — количество созданных операций;
- **Submitted Rate (5m)** — количество отправленных операций;
- **Completed Rate (5m)** — количество успешно завершенных операций;
- **Rejected Rate (5m)** — количество отклоненных операций.

**Operations Timeline**
- **Operations Timeline** — динамика всех операций по времени.

---

### 2. Payment Gateway - Application Layer

Дашборд отслеживает запросы уровня Application (MediatR commands/queries):

**Overview**
- **Failed Rate (5m)** — процент ошибочных запросов;
- **Concurrency Retry Rate (5m)** — процент оптимистичных retry.

**Failed Rate by Request**
- **Failed Rate by Request** — распределение ошибок по типам команд.

**Concurrency Retry Rate by Request**
- **Concurrency Retry Rate by Request** — распределение retry по типам команд.

**Requests Timeline & Errors**
- **Requests Timeline & Errors** — количество запущенных, успешных и ошибочных запросов.

**Latency & Performance**
- **Request Latency** — latency запросов (p50, p95, avg) с разбивкой по типам;
- **Request Duration Heatmap** — распределение времени выполнения запросов.

---

### 3. Payment Gateway - Dispatch Engine

Дашборд отслеживает работу механизма dispatch-обработки:

**Overview**
- **Oldest Processing Operation Age** — возраст самой старой операции в статусе `PROCESSING` (критическая метрика).

**Dispatch Processing**
- **Dispatch Batch Duration** — длительность обработки batch'ей (p95, avg);
- **Dispatch Batch Size** — размер batch'ей (p90, avg).

---

### 4. Payment Gateway - Payment Provider

Дашборд отслеживает интеграцию с платежным провайдером:

**Overview**
- **Failed Rate (5m)** — процент ошибочных запросов к провайдеру;
- **Retry Scheduled Rate (5m)** — процент запланированных retry;
- **Retry Limit Reached Rate (5m)** — процент достижений лимита повторов.

**Requests Timeline & Errors**
- **Requests Timeline & Errors** — динамика запущенных, успешных, ошибочных запросов, retry и достижений лимита.

**Latency & Performance**
- **Request Latency** — latency запросов к провайдеру (p50, p95, avg);
- **Request Duration Heatmap** — распределение времени выполнения запросов к провайдеру.

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