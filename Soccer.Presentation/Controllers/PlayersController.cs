using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Soccer.Application.DTO;
using Soccer.Application.Interfaces;
using Soccer.Common.Exceptions;

namespace Soccer.Presentation.Controllers
{
    public class PlayersController : Controller
    {
        private readonly IEntityService<PlayerDTO> playerService;
        private readonly IEntityService<TeamDTO> teamService;

        public PlayersController(IEntityService<PlayerDTO> playerserv, IEntityService<TeamDTO> teamserv)
        {
            playerService = playerserv; // зберігаємо сервіс гравців, сервіс буде створений через DI та потрібен для крад-операцій
            teamService = teamserv;
        }

        // GET: Players
        public async Task<IActionResult> Index()
        {
            return View(await playerService.GetAll()); // повертаємо список гравців у подання
        }

        // GET: Players/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound(); // не знайдено, якщо ідентифікатор відсутній
                }
                PlayerDTO player = await playerService.Get((int)id);
                return View(player); // показуємо деталі гравця
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message); // повертаємо повідомлення про помилку валідації
            }
        }

        // GET: Players/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.ListTeams = new SelectList(await teamService.GetAll(), "Id", "Name"); // заповнюємо список команд, селектліст корисний для випадаючих списків
            return View(); // повертаємо порожню форму створення
        }

        // POST: Players/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlayerDTO player)
        {
            if (ModelState.IsValid)
            {
                await playerService.Create(player); // створюємо нового гравця
                return View("~/Views/Players/Index.cshtml", await playerService.GetAll()); // перенаправляємо на список
            }
            ViewBag.ListTeams = new SelectList(await teamService.GetAll(), "Id", "Name", player.TeamId); // відновлюємо список з вибраною командою
            return View(player); // повертаємо форму з помилками
        }

        // GET: Players/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound(); // не знайдено, якщо ідентифікатор відсутній
                }
                PlayerDTO player = await playerService.Get((int)id);
                ViewBag.ListTeams = new SelectList(await teamService.GetAll(), "Id", "Name", player.TeamId); // заповнюємо список команд
                return View(player); // показуємо форму редагування
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message); // повертаємо повідомлення про помилку валідації
            }
        }

        // POST: Players/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PlayerDTO player)
        {
            if (ModelState.IsValid)
            {
                await playerService.Update(player); // оновлюємо дані гравця
                return View("~/Views/Players/Index.cshtml", await playerService.GetAll()); // перенаправляємо на список
            }
            ViewBag.ListTeams = new SelectList(await teamService.GetAll(), "Id", "Name", player.TeamId); // відновлюємо список з вибраною командою
            return View(player); // повертаємо форму з помилками
        }

        // GET: Players/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound(); // не знайдено, якщо ідентифікатор відсутній
                }
                PlayerDTO player = await playerService.Get((int)id);
                return View(player); // показуємо підтвердження видалення
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message); // повертаємо повідомлення про помилку валідації
            }
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await playerService.Delete(id); // видаляємо гравця
            return View("~/Views/Players/Index.cshtml", await playerService.GetAll()); // перенаправляємо на список
        }
    }
}