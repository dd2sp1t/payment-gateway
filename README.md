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
- динамика жизненного цикла операций.

### Application Layer

Отслеживаются запросы уровня Application (MediatR commands/queries):

- latency (p50 / p95 / average);
- throughput;
- heatmap времени выполнения;
- количество concurrency retries.

### Dispatch

- возраст самой старой операции в состоянии `PROCESSING`;
- длительность обработки batch'ей;
- размер batch'ей.

### Интеграции с Provider

- успешность отправок;
- latency запросов к провайдеру;
- timeline выполнения dispatch операций, включая успешные отправки, запланированные retry и достижение лимита повторов;
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