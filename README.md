# Soccer — Clean Architecture

Навчальний проєкт на **ASP.NET Core MVC**, побудований строго за принципами **Clean Architecture** (Роберт С. Мартін).

Мета проєкту — показати, як правильно організувати код у великому застосунку так, щоб:
- бізнес-логіка була повністю незалежною від фреймворків і бази даних;
- зміни в одній частині системи мінімально впливали на інші;
- проєкт було легко тестувати, розширювати і підтримувати.

---

## 🧠 Чому саме Clean Architecture?

Класична тришарова архітектура (Presentation → BLL → DAL) часто призводить до того, що бізнес-логіка починає «знати» про Entity Framework, SQL Server або навіть про HTTP. Це створює жорстку зв’язаність.

**Clean Architecture** розвертає залежності навпаки:

> Усі залежності спрямовані **всередину** — до ядра (Domain).  
> Зовнішні шари залежать від внутрішніх, але внутрішні **нічого не знають** про зовнішні.

Це дає кілька важливих переваг:

1. **Domain** можна тестувати без бази даних і веб-сервера.
2. Можна легко замінити SQL Server на PostgreSQL, MongoDB або навіть in-memory сховище — Application і Domain не помітять різниці.
3. Presentation (MVC, Minimal API, gRPC, Blazor) можна змінити, не чіпаючи бізнес-логіку.
4. Код стає більш зрозумілим: кожен шар має чітку і єдину відповідальність.

---

## 🏗️ Структура рішення

```
Soccer.sln
│
├── Soccer.Domain                 ← Ядро (найчистіший шар)
│   ├── Entities/
│   │   ├── Player.cs
│   │   └── Team.cs
│   └── Interfaces/
│       ├── IRepository.cs
│       └── IUnitOfWork.cs
│
├── Soccer.Common                 ← Спільні утиліти
│   └── Exceptions/
│       └── ValidationException.cs
│
├── Soccer.Application            ← Бізнес-логіка / Use Cases
│   ├── DTO/
│   ├── Interfaces/
│   ├── Services/
│   ├── Mapping/
│   └── DependencyInjection/
│
├── Soccer.Infrastructure         ← Реалізація доступу до даних
│   ├── Persistence/
│   ├── Repositories/
│   └── DependencyInjection/
│
└── Soccer.Presentation           ← Точка входу (ASP.NET Core MVC)
    ├── Controllers/
    ├── Views/
    ├── wwwroot/
    ├── Program.cs                ← Композиційний корінь
    └── appsettings.json
```

---

## 📦 Детальний опис кожного шару

### 1. Soccer.Domain — Ядро системи

**Що тут знаходиться:**
- Сутності (`Player`, `Team`) — чисті C#-класи без атрибутів EF Core.
- Контракти (`IRepository<T>`, `IUnitOfWork`) — інтерфейси, які описують, *що* потрібно від сховища даних, але не *як* це реалізовано.

**Чому саме так:**
- Domain не повинен залежати ні від чого. Жодних NuGet-пакетів, жодних `using Microsoft.EntityFrameworkCore`.
- Якщо бізнес-правила змінюються — змінюється тільки цей шар.
- Усі інші шари залежать від Domain, а не навпаки. Це і є Dependency Inversion Principle у дії.

### 2. Soccer.Common — Спільні речі

Містить код, який потрібен кільком шарам одночасно, але не є бізнес-логікою.

Зараз тут лише `ValidationException` — виняток, який:
- кидається в `Application` (коли сутність не знайдена);
- ловиться в `Presentation` (щоб повернути 404).

**Чому окремий проєкт:**
Якщо покласти цей виняток у Domain — Domain почне знати про «валідацію для веб».  
Якщо покласти в Application — Presentation не зможе на нього посилатися без зайвої залежності.  
Common вирішує цю проблему чисто.

### 3. Soccer.Application — Сценарії використання (Use Cases)

Тут живе вся бізнес-логіка застосунку:

- **DTO** — об’єкти, які ходять між Presentation і Application (не сутності Domain!).
- **Сервіси** (`PlayerService`, `TeamService`) — реалізують конкретні сценарії (отримати всіх гравців, додати команду тощо).
- **Інтерфейси сервісів** (`IEntityService<T>`).
- **AutoMapper-профілі** — єдине місце, де відбувається мапінг Entity ↔ DTO.
- **DependencyInjection** — метод розширення `AddApplication()`, який реєструє всі сервіси та AutoMapper.

**Чому Application не залежить від Infrastructure:**
Application працює тільки з інтерфейсами з Domain (`IUnitOfWork`, `IRepository`).  
Він *не знає*, що під капотом Entity Framework.  
Це дозволяє писати юніт-тести на сервіси, підставляючи фейкові репозиторії.

### 4. Soccer.Infrastructure — Реалізація деталей

Тут знаходиться все, що стосується конкретної технології зберігання даних:

- `SoccerContext` — DbContext Entity Framework Core.
- Реалізації репозиторіїв.
- `EFUnitOfWork`.
- Метод розширення `AddInfrastructure(connectionString)`.

**Чому Infrastructure залежить тільки від Domain:**
Він *реалізує* інтерфейси, оголошені в Domain.  
Application і Domain навіть не підозрюють про існування `DbContext` чи SQL Server.

Якщо завтра знадобиться замінити EF Core на Dapper або іншу ORM — змінюється лише цей проєкт.

### 5. Soccer.Presentation — Зовнішній шар

ASP.NET Core MVC додаток:

- Контролери
- Razor-представлення
- Статичні файли
- `Program.cs`

**Найважливіша роль `Program.cs`:**

Це **композиційний корінь** (Composition Root).  
Єдине місце в усьому рішенні, яке одночасно знає і про Application, і про Infrastructure:

```csharp
builder.Services.AddInfrastructure(connection); // реєструємо DbContext + UnitOfWork
builder.Services.AddApplication();              // реєструємо сервіси + AutoMapper
builder.Services.AddControllersWithViews();
```

Завдяки цьому всі інші проєкти залишаються чистими і не знають один про одного більше, ніж потрібно.

---

## 🔄 Потік виконання запиту (приклад)

1. Користувач відкриває `/Teams/Index`.
2. `TeamsController` викликає `IEntityService<TeamDTO>.GetAll()`.
3. `TeamService` (Application) через `IUnitOfWork` просить дані.
4. `EFUnitOfWork` + репозиторій (Infrastructure) йдуть у базу через EF Core.
5. Дані повертаються у вигляді сутностей Domain.
6. AutoMapper перетворює їх на DTO.
7. Контролер віддає DTO у View.

Жоден шар не порушує свої межі відповідальності.

---

## ✨ Що реалізовано в проєкті

- Повноцінний CRUD для гравців і команд
- Repository Pattern + Unit of Work
- AutoMapper з єдиним профілем (реєструється один раз через DI)
- Правильна обробка ситуації «сутність не знайдена» через `ValidationException` → 404
- Чиста реєстрація залежностей через extension-методи
- Дотримання Dependency Rule Clean Architecture

---

## 🚀 Як запустити

```bash
git clone https://github.com/sunmeat/aspnetcore_clean.git
cd aspnetcore_clean

dotnet restore
dotnet run --project Soccer.Presentation
```

Перед запуском відкрий файл:

```
Soccer.Presentation/appsettings.json
```

і перевір рядок підключення `DefaultConnection` (має вказувати на доступний SQL Server).

Після запуску відкрий у браузері:

```
https://localhost:xxxx/Teams/Index
```

(порт подивись у консолі).

---

## 🛠️ Технологічний стек

| Технологія               | Для чого використовується                  |
|--------------------------|--------------------------------------------|
| ASP.NET Core MVC         | Presentation layer                         |
| Entity Framework Core    | Persistence (Infrastructure)               |
| AutoMapper               | Мапінг між Domain-сутностями та DTO        |
| SQL Server               | База даних                                 |
| Dependency Injection     | Зв’язування всіх шарів                     |
| .NET (актуальна LTS)     | Платформа                                  |

---

## 🎯 Для кого цей проєкт

- Для тих, хто вивчає Clean Architecture і хоче побачити її на реальному прикладі.
- Для тих, хто хоче зрозуміти, чому Domain має бути в центрі, а не DAL.
- Для тих, хто планує писати підтримувані і тестовані застосунки на ASP.NET Core.

Проєкт навмисно зроблений невеликим і зрозумілим, щоб можна було швидко розібратися в потоках залежностей і принципах, а не потонути в зайвому коді.
```
