using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;
using TaskFlowAPI.Repositories;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize] // every endpoint in this controller requires a valid JWT
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _repository;

        public TasksController(ITaskRepository repository)
        {
            _repository = repository;
        }
        private int CurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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
            // read the users id from their token
            var userId = CurrentUserId();
            // Destructure the tuplet returned by the repository
            var (tasks,totalCount) = await _repository.GetAllAsync(taskQueryParameters, userId);
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
            var userId = CurrentUserId();

            var task = await _repository.GetByIdAsync(id, userId);

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
                CreatedAt = DateTime.UtcNow,
                UserId = CurrentUserId()
            };

            await _repository.CreateAsync(newTask);
            await _repository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, MapToResponseDto(newTask));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTask(int id, TaskCreateDto updateDto)
        {
            var userId = CurrentUserId();
            var existingTask = await _repository.GetByIdAsync(id, userId);

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
            var userId = CurrentUserId();
            var existingTask = await _repository.GetByIdAsync(id, userId);

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
            var userId = CurrentUserId();
            var task = await _repository.GetByIdAsync(id, userId);

            if (task == null) return NotFound();

            await _repository.DeleteAsync(task);
            await _repository.SaveChangesAsync();

            return NoContent();
        }
    }
}