# Soccer — Clean Architecture

Це переробка навчального прикладу [sunmeat/aspnetcore_layers](https://github.com/sunmeat/aspnetcore_layers)
(ASP.NET Core MVC + тришарова архітектура Presentation/BLL/DAL) під **Clean Architecture**.
Кожен шар — окремий проєкт у спільному рішенні `Soccer.sln`.

## Проєкти рішення

```
Soccer.sln
│
├── Soccer.Domain          — сутності (Player, Team) та контракти (IRepository, IUnitOfWork).
│                            Не залежить ні від чого. Жодних NuGet-пакетів.
│
├── Soccer.Common          — наскрізні речі, спільні для кількох шарів.
│                            Зараз тут лише ValidationException (його кидає Application,
│                            а ловить Presentation).
│
├── Soccer.Application     — DTO, use-case-сервіси (PlayerService, TeamService),
│                            інтерфейс IEntityService<T>, AutoMapper-профіль.
│                            Залежить від Domain і Common.
│
├── Soccer.Infrastructure  — EF Core: SoccerContext, репозиторії, EFUnitOfWork,
│                            DI-реєстрація (AddInfrastructure). Залежить лише від Domain.
│
└── Soccer.Presentation    — ASP.NET Core MVC: контролери, Views, Program.cs.
                             Композиційний корінь застосунку: єдине місце,
                             де одночасно підключені Application та Infrastructure.
```

## Напрямок залежностей

```
Soccer.Presentation ──► Soccer.Application ──► Soccer.Domain
        │                       │                    ▲
        │                       └──► Soccer.Common ───┘
        └──────────────────► Soccer.Infrastructure ──► Soccer.Domain
```

Головне правило Clean Architecture дотримано: усі стрілки залежностей спрямовані
всередину, до `Soccer.Domain`. Ні `Domain`, ні `Application` нічого не знають
про Entity Framework Core, SQL Server чи ASP.NET Core MVC — ці деталі
ізольовані в `Infrastructure` та `Presentation` відповідно.

`Program.cs` у `Soccer.Presentation` — єдине місце, що посилається одразу
на `Soccer.Application` (`AddApplication()`) і на `Soccer.Infrastructure`
(`AddInfrastructure(connection)`), тобто виконує роль композиційного кореня.

## Що саме змінилося порівняно з оригіналом

| Було (тришарова архітектура)              | Стало (Clean Architecture)                          |
|--------------------------------------------|------------------------------------------------------|
| `Soccer.DAL/Entities`                       | `Soccer.Domain/Entities`                             |
| `Soccer.DAL/Interfaces` (IRepository, IUnitOfWork) | `Soccer.Domain/Interfaces` — контракти належать ядру |
| `Soccer.DAL/EF/SoccerContext.cs`            | `Soccer.Infrastructure/Persistence/SoccerContext.cs` |
| `Soccer.DAL/Repositories`                   | `Soccer.Infrastructure/Repositories`                 |
| `Soccer.BLL/DTO`, `Soccer.BLL/Services`, `Soccer.BLL/Interfaces` | `Soccer.Application/DTO`, `.../Services`, `.../Interfaces` |
| `Soccer.BLL/Infrastructure/SoccerContextExtensions.cs`, `UnitOfWorkServiceExtensions.cs` | `Soccer.Infrastructure/DependencyInjection/InfrastructureServiceExtensions.cs` (`AddInfrastructure`) |
| Реєстрація сервісів прямо в `Program.cs`    | `Soccer.Application/DependencyInjection/ApplicationServiceExtensions.cs` (`AddApplication`) |
| `Soccer.BLL/Infrastructure/ValidationException.cs` | `Soccer.Common/Exceptions/ValidationException.cs` |
| `MapperConfiguration` створювався заново на кожен виклик `GetAll()` | Один `MappingProfile`, зареєстрований через `services.AddAutoMapper(...)`, `IMapper` інжектиться в сервіси |
| `Soccer` (веброзетка MVC)                    | `Soccer.Presentation`                                |

### Дрібне виправлення поведінки

В оригіналі сервіси (`PlayerService.Get`, `TeamService.Get`) кидали
`System.ComponentModel.DataAnnotations.ValidationException` (бо власний клас
з `Soccer.BLL.Infrastructure` не був підключений через `using`), тоді як
контролери ловили саме користувацький `Soccer.BLL.Infrastructure.ValidationException`.
Через розбіжність типів `catch` фактично ніколи не спрацьовував, і запит
неіснуючого гравця/команди призводив би до необробленого виключення (500),
а не до очікуваного `404 NotFound`. У цій версії обидва місця використовують
один і той самий `Soccer.Common.Exceptions.ValidationException`, тож
`NotFound` тепер справді повертається.

## Запуск

```
git clone <ваш форк або цей архів>
cd Soccer-Clean
dotnet restore
dotnet run --project Soccer.Presentation
```

Перед запуском переконайтеся, що SQL Server доступний і `DefaultConnection`
у `Soccer.Presentation/appsettings.json` вказує на потрібний сервер/базу.
Стартовий маршрут (як і в оригіналі): `/Teams/Index`.
