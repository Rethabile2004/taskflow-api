using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;
using TaskFlowAPI.Repositories;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _repository;

        public TasksController(ITaskRepository repository)
        {
            _repository = repository;
        }

        private TaskResponseDto MapToResponseDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                CategoryId = task.CategoryId,
                CategoryName = task.Category?.Name
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PagedResult< TaskResponseDto>>>> GetAllTasks([FromQuery]TaskQueryParameters taskQueryParameters)
        {
            // Destructure the tuplet returned by the repository
            var (tasks,totalCount) = await _repository.GetAllAsync(taskQueryParameters);
            var pagedResult = new PagedResult<TaskResponseDto>()
            {
                Page = taskQueryParameters.Page,
                PageSize = taskQueryParameters.PageSize,
                TotalCount = totalCount,
                Data = tasks.Select(task => MapToResponseDto(task))
            };
            return Ok(pagedResult);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTaskById(int id)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null) return NotFound();

            return Ok(MapToResponseDto(task));
        }

        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> CreateTask(TaskCreateDto createDto)
        {
            var newTask = new TaskItem
            {
                Title = createDto.Title,
                Description = createDto.Description,
                IsCompleted = createDto.IsCompleted,
                CategoryId = createDto.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(newTask);
            await _repository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, MapToResponseDto(newTask));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTask(int id, TaskCreateDto updateDto)
        {
            var existingTask = await _repository.GetByIdAsync(id);

            if (existingTask == null) return NotFound();

            existingTask.Title = updateDto.Title;
            existingTask.Description = updateDto.Description;
            existingTask.IsCompleted = updateDto.IsCompleted;
            existingTask.CategoryId = updateDto.CategoryId;

            await _repository.UpdateAsync(existingTask);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> PatchTask(int id, TaskPatchDto patchDto)
        {
            var existingTask = await _repository.GetByIdAsync(id);

            if (existingTask == null) return NotFound();

            if (patchDto.Title != null) existingTask.Title = patchDto.Title;
            if (patchDto.Description != null) existingTask.Description = patchDto.Description;
            if (patchDto.IsCompleted != null) existingTask.IsCompleted = patchDto.IsCompleted.Value;

            await _repository.UpdateAsync(existingTask);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null) return NotFound();

            await _repository.DeleteAsync(task);
            await _repository.SaveChangesAsync();

            return NoContent();
        }
    }
}