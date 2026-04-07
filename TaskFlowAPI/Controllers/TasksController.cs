using Microsoft.AspNetCore.Mvc;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        // Temporary in-memory list — we replace this with a DB in Week 3
        private static List<TaskItem> _tasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Buy groceries", Description = "Milk, eggs, bread" },
            new TaskItem { Id = 2, Title = "Study Web API", Description = "Week 1 Session 1" },
            new TaskItem { Id = 3, Title = "Exercise", Description = "30 min cardio", IsCompleted = true }
        };

        // GET api/tasks
        [HttpGet]
        public ActionResult<IEnumerable<TaskItem>> GetAllTasks()
        {
            return Ok(_tasks);
        }

        // GET api/tasks/1
        [HttpGet("{id}")]
        public ActionResult<TaskItem> GetTaskById(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);

            if (task == null)
            {
                return NotFound(); // 404
            }

            return Ok(task); // 200
        }
    }
}