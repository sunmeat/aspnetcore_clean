using Microsoft.AspNetCore.Mvc;
using Soccer.Application.DTO;
using Soccer.Application.Interfaces;
using Soccer.Common.Exceptions;

namespace Soccer.Presentation.Controllers
{
    public class TeamsController : Controller
    {
        private readonly IEntityService<TeamDTO> teamService;

        public TeamsController(IEntityService<TeamDTO> serv)
        {
            teamService = serv; // зберігаємо сервіс команд
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            return View(await teamService.GetAll()); // повертаємо список команд у подання
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound(); // не знайдено, якщо ідентифікатор відсутній
                }
                TeamDTO team = await teamService.Get((int)id);
                return View(team); // показуємо деталі команди
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message); // повертаємо повідомлення про помилку валідації
            }
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            return View(); // повертаємо порожню форму створення команди
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamDTO team)
        {
            if (ModelState.IsValid)
            {
                await teamService.Create(team); // створюємо нову команду
                return View("~/Views/Teams/Index.cshtml", await teamService.GetAll()); // перенаправляємо на список
            }
            return View(team); // повертаємо форму з помилками
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound(); // не знайдено, якщо ідентифікатор відсутній
                }
                TeamDTO team = await teamService.Get((int)id);
                return View(team); // показуємо форму редагування команди
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message); // повертаємо повідомлення про помилку валідації
            }
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TeamDTO team)
        {
            if (ModelState.IsValid)
            {
                await teamService.Update(team); // оновлюємо дані команди
                return View("~/Views/Teams/Index.cshtml", await teamService.GetAll()); // перенаправляємо на список
            }
            return View(team); // повертаємо форму з помилками
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound(); // не знайдено, якщо ідентифікатор відсутній
                }
                TeamDTO team = await teamService.Get((int)id);
                return View(team); // показуємо підтвердження видалення
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message); // повертаємо повідомлення про помилку валідації
            }
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await teamService.Delete(id); // видаляємо команду
            return View("~/Views/Teams/Index.cshtml", await teamService.GetAll()); // перенаправляємо на список
        }
    }
}