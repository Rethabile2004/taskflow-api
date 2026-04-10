using Microsoft.AspNetCore.Mvc;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private static List<TaskItem> _tasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Buy groceries", Description = "Milk, eggs, bread" },
            new TaskItem { Id = 2, Title = "Study Web API", Description = "Week 1 Session 1" },
            new TaskItem { Id = 3, Title = "Exercise", Description = "30 min cardio", IsCompleted = true }
        };
        private static int _nextId = 4;
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
        [HttpPost]
        public ActionResult<TaskItem> CreateTask(TaskItem newTask)
        {
            newTask.Id = _nextId++;
            newTask.CreatedAt = DateTime.UtcNow;

            _tasks.Add(newTask);
            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, newTask);// 201 created with a location pointing to the new source
        }
        [HttpPut("{id}")]
        public ActionResult UpdateTask(int id, TaskItem updatedItem)
        {
            var existingTask = _tasks.FirstOrDefault(t => t.Id==id);
            if (existingTask == null)
            {
                return NotFound();//404 not found
            }
            existingTask.Title = updatedItem.Title;
            existingTask.Description = updatedItem.Description;
            existingTask.IsCompleted = updatedItem.IsCompleted;
            return NoContent(); //204 success nothing to return
        }
        [HttpDelete("{id}")]
        public ActionResult DeleteTask(int id)
        {
            var item = _tasks.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound(); // 404 Not found
            }
            _tasks.Remove(item);
            return NoContent();// 204 success nothing to return
        }
    }
}