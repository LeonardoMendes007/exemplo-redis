using ExemploRedis.Entities;
using ExemploRedis.Infra.Caching;
using ExemploRedis.Infra.Data;
using ExemploRedis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ExemploRedis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDosController : ControllerBase
    {
        private readonly ToDoListDbContext _context;
        private readonly ICachingService _cachingService;

        public ToDosController(ToDoListDbContext context, ICachingService cachingService)
        {
            _context = context;
            _cachingService = cachingService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) {

            var todoCache = await _cachingService.GetAsync(id.ToString());
            ToDo? todo;

            if(!string.IsNullOrEmpty(todoCache)) {
                todo = JsonConvert.DeserializeObject<ToDo>(todoCache);
                return Ok(todo);
            }

            todo = await _context.ToDos.SingleOrDefaultAsync(t => t.Id == id);

            if(todo == null)
            {
                return NotFound();
            }

            await _cachingService.SetAsync(id.ToString(), JsonConvert.SerializeObject(todo));

            return Ok(todo);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ToDoInputModel model)
        {
            var todo = new ToDo(0, model.Title, model.Description);

            await _context.ToDos.AddAsync(todo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = todo.Id }, model);
        }
    }
}
