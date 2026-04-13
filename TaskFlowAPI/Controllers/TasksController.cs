using Microsoft.AspNetCore.Mvc;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllersz
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private static List<TaskItem> _tasks =
        [
            new TaskItem { Id = 1, Title = "Buy groceries", Description = "Milk, eggs, bread" },
            new TaskItem { Id = 2, Title = "Study Web API", Description = "Week 1 Session 1" },
            new TaskItem { Id = 3, Title = "Exercise", Description = "30 min cardio", IsCompleted = true }
        ];
        private static int _nextId = 4;
        private TaskResponseDto MapToResponseDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                IsCompleted = task.IsCompleted
            };
        }
        // GET api/tasks
        [HttpGet]
        public ActionResult<IEnumerable<TaskResponseDto>> GetAllTasks()
        {
            var resposeDto = _tasks.Select(t => MapToResponseDto(t));
            return Ok(resposeDto);
        }
        [HttpGet("{id}")]
        public ActionResult<TaskResponseDto>GetTaskById(int id)
        {
            var item = _tasks.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound();//404
            }
            return Ok(MapToResponseDto(item));//200
        }
        [HttpPost]
        public ActionResult<TaskResponseDto>CreateTask(TaskCreateDto newTask)
        {
            var createTask = new TaskItem
            {
                CreatedAt = DateTime.UtcNow,
                Description = newTask.Description,
                Id = _nextId++,
                IsCompleted=newTask.IsCompleted,
                Title=newTask.Title,                
            };
            _tasks.Add(createTask);
            return CreatedAtAction(nameof(GetTaskById), new { id = createTask.Id }, MapToResponseDto(createTask));
        }
        [HttpPut("{id}")]
        public ActionResult<TaskResponseDto> UpdateTask(int id,TaskCreateDto updateDto)
        {
            var existing = _tasks.FirstOrDefault(t => t.Id == id);
            if (existing == null)
            {
                return NotFound();// 404
            }
            existing.Title = updateDto.Title;
            existing.Description = updateDto.Description;
            existing.IsCompleted = updateDto.IsCompleted;
            return NoContent();// 204
        }
        [HttpDelete("{id}")]
        public ActionResult<TaskResponseDto>DeleteTask(int id)
        {
            var existing = _tasks.FirstOrDefault(t => t.Id == id);
            if(existing == null){
                return NotFound();
            }
            _tasks.Remove(existing);
            return NoContent();
        }
    }
}