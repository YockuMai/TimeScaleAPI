# Тестовое задание C#

Веб-приложение на ASP.NET Core (NET 8) для обработки CSV-файлов с данными измерений: загрузка файла, агрегация значений, фильтрация и получение результатов.

## Структура проекта

```
TestTaskCS/
├── WebApplication.sln                  # Решение
├── README.md                           # Документация
├── WebApplication/                     # Основной веб-проект
│   ├── Program.cs                      # Точка входа, DI-конфигурация
│   ├── Application/
│   │   ├── DTOs/                       # Модели передачи данных
│   │   ├── Interfaces/                 # Интерфейсы сервисов
│   │   └── Services/                   # Реализации сервисов
│   ├── Model/
│   │   ├── AppDbContext.cs             # Контекст EF Core
│   │   └── Entities/                   # Сущности БД
│   ├── Migrations/                     # Миграции EF Core
│   ├── WebApi/                         # Контроллеры
│   └── Tests/files/                    # Примеры CSV-файлов
└── WebApplication.Tests/               # Юнит-тесты (xUnit)
```

## Технологии

- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core 8** (PostgreSQL)
- **Swagger / OpenAPI**
- **xUnit** — юнит-тесты
- **Moq** — моки для тестирования
- **SQLite (in-memory)** — тестовая БД

## Эндпоинты API

### 1. Загрузка CSV-файла

```
POST /api/file/upload
Content-Type: multipart/form-data
```

| Параметр | Тип      | Описание |
|----------|----------|----------|
| `file`   | `IFormFile` | CSV-файл (`.csv`) |

Формат CSV (разделитель `;`):

```
Date;ExecutionTime;Value
2024-09-24T11-35-36.7939Z;23;657.4498
2024-09-25T12-00-00.0000Z;7;36.8968
```

- **Date** — дата в формате `yyyy-MM-dd'T'HH-mm-ss.FFFFFFF'Z'` (UTC)
- **ExecutionTime** — целое число ≥ 0
- **Value** — число ≥ 0

Ограничения:
- Количество строк данных: от 1 до 10000
- Дата должна быть не раньше 01.01.2000 и не позже текущего момента

Ответы:
- `200 OK` — файл успешно сохранён
- `400 Bad Request` — ошибки валидации (пустой файл, неверный формат, недопустимое расширение)
- `500 Internal Server Error` — внутренняя ошибка

### 2. Получение последних 10 значений по имени файла

```
GET /api/value/values?FileName=файл.csv
```

| Параметр    | Тип      | Описание               |
|-------------|----------|------------------------|
| `FileName`  | `string` | Имя загруженного файла |

Ответ:
- `200 OK` — список из максимум 10 значений, отсортированных по дате убыванию

### 3. Получение результатов агрегации с фильтрацией

```
GET /api/result/result?FileName=файл.csv&MeanValueFrom=100&MeanValueTo=500
```

Параметры фильтра (все опциональны):

| Параметр                 | Тип       | Описание                     |
|--------------------------|-----------|------------------------------|
| `FileName`               | `string`  | Имя файла                    |
| `FirstOperationFrom`     | `DateTime`| Минимальная дата (MinDate ≥) |
| `FirstOperationTo`       | `DateTime`| Максимальная дата (MinDate ≤)|
| `MeanValueFrom`          | `double`  | Среднее значение ≥           |
| `MeanValueTo`            | `double`  | Среднее значение ≤           |
| `MeanExecutionTimeFrom`  | `double`  | Среднее время выполнения ≥   |
| `MeanExecutionTimeTo`    | `double`  | Среднее время выполнения ≤   |

Ответ:
- `200 OK` — список результатов агрегации

Результат агрегации содержит: `FileId`, `FileName`, `DeltaDate` (разница между макс. и мин. датой в секундах), `MinDate`, `MeanExecutionTime`, `MeanValue`, `MedianValue`, `MaxValue`, `MinValue`.

## Агрегация данных

При загрузке CSV-файла для каждого файла вычисляется:

- **DeltaDate** — разница между максимальной и минимальной датой (в секундах)
- **MinDate** — минимальная дата
- **MeanExecutionTime** — среднее время выполнения
- **MeanValue** — среднее значение
- **MedianValue** — медианное значение
- **MinValue** — минимальное значение
- **MaxValue** — максимальное значение

При повторной загрузке файла с тем же именем, предыдущие данные и результат агрегации удаляются.

## Запуск проекта

1. Настройте строку подключения к PostgreSQL в `WebApplication/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TestTaskCS;User Id=postgres;Password=password;"
  }
}
```

2. Запустите приложение (миграции применяются автоматически при старте):

```bash
dotnet run --project WebApplication
```

Приложение автоматически перенаправит на Swagger: `http://localhost:PORT/swagger/index.html`

## Запуск тестов

```bash
dotnet test WebApplication.sln
```

Тесты покрывают:

| Класс                   | Кол-во тестов |
|-------------------------|--------------|
| `CsvParserTests`        | 12           |
| `AggregationCalculatorTests` | 5        |
| `ValueFilterTests`      | 4            |
| `AggregationFilterTests`| 10           |
| `FileControllerTests`   | 7            |
| `ValueControllerTests`  | 2            |
| `ResultControllerTests` | 3            |
| **Всего**               | **43**       |

## Структура сервисов

| Интерфейс                 | Реализация                | Описание |
|---------------------------|---------------------------|----------|
| `ICsvParser`              | `CsvParser`              | Парсинг CSV-файлов и сохранение в БД |
| `IAggregationCalculator`  | `AggregationCalculator`  | Расчёт агрегации значений |
| `IValueFilter`            | `ValueFilter`            | Получение последних 10 значений |
| `IAggregationFilter`      | `AggregationFilter`      | Фильтрация результатов агрегации |