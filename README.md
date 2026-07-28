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

## Сквозной сценарий

### 1. Создать операцию

```bash
curl -X POST http://localhost:8080/operations \
  -H "Content-Type: application/json" \
  -d '{
    "operationId":"operation-123",
    "amount":"1000.00",
    "currency":"RUB",
    "description":"Оплата заказа"
  }'
```

### 2. Отправить операцию на обработку

```bash
curl -X POST \
http://localhost:8080/operations/operation-123/submit
```

Ожидаемый результат — `202 Accepted` при первой отправке.

### 3. Проверить состояние операции

```bash
curl \
http://localhost:8080/operations/operation-123
```

После обработки провайдером статус станет `COMPLETED` или `REJECTED`.

### 4. Получить историю событий

```bash
curl \
http://localhost:8080/operations/operation-123/events
```

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