using Microsoft.AspNetCore.Mvc;
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

        // DI container injects ITaskRepository automatically
        public TasksController(ITaskRepository repository)
        {
            _repository = repository;
        }

        // ── Helper: Map TaskItem to TaskResponseDto ───────────────────────────
        private TaskResponseDto MapToResponseDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt
            };
        }

        // ── GET ALL ───────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAllTasks()
        {
            var tasks = await _repository.GetAllAsync();
            var responseDtos = tasks.Select(task => MapToResponseDto(task));
            return Ok(responseDtos);
        }

        // ── GET ONE ───────────────────────────────────────────────────────────

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTaskById(int id)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(MapToResponseDto(task));
        }

        // ── CREATE ────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> CreateTask(TaskCreateDto createDto)
        {
            var newTask = new TaskItem
            {
                Title = createDto.Title,
                Description = createDto.Description,
                IsCompleted = createDto.IsCompleted,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(newTask);
            await _repository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, MapToResponseDto(newTask));
        }

        // ── UPDATE FULL ───────────────────────────────────────────────────────

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTask(int id, TaskCreateDto updateDto)
        {
            var existingTask = await _repository.GetByIdAsync(id);

            if (existingTask == null)
            {
                return NotFound();
            }

            existingTask.Title = updateDto.Title;
            existingTask.Description = updateDto.Description;
            existingTask.IsCompleted = updateDto.IsCompleted;

            await _repository.UpdateAsync(existingTask);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        // ── UPDATE PARTIAL ────────────────────────────────────────────────────

        [HttpPatch("{id}")]
        public async Task<ActionResult> PatchTask(int id, TaskPatchDto patchDto)
        {
            var existingTask = await _repository.GetByIdAsync(id);

            if (existingTask == null)
            {
                return NotFound();
            }

            if (patchDto.Title != null)
            {
                existingTask.Title = patchDto.Title;
            }

            if (patchDto.Description != null)
            {
                existingTask.Description = patchDto.Description;
            }

            if (patchDto.IsCompleted != null)
            {
                existingTask.IsCompleted = patchDto.IsCompleted.Value;
            }

            await _repository.UpdateAsync(existingTask);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        // ── DELETE ────────────────────────────────────────────────────────────

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(task);
            await _repository.SaveChangesAsync();

            return NoContent();
        }
    }
}