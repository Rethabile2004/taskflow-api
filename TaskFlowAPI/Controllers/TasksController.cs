using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;
using TaskFlowAPI.Repositories;

namespace TaskFlowAPI.Controllers
{
    /// <summary>
    /// Manages tasks for the authenticated user.
    /// </summary>
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _repository;

        public TasksController(ITaskRepository repository)
        {
            _repository = repository;
        }

        private int GetCurrentUserId()
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

        /// <summary>
        /// Returns a paginated, filterable list of tasks for the authenticated user.
        /// </summary>
        [HttpGet]
        [EnableRateLimiting("read")]
        public async Task<ActionResult<PagedResult<TaskResponseDto>>> GetAllTasks(
            [FromQuery] TaskQueryParameters queryParams)
        {
            var userId = GetCurrentUserId();
            var (tasks, totalCount) = await _repository.GetAllAsync(queryParams, userId);

            var pagedResult = new PagedResult<TaskResponseDto>
            {
                Page = queryParams.Page,
                PageSize = queryParams.PageSize,
                TotalCount = totalCount,
                Data = tasks.Select(task => MapToResponseDto(task))
            };

            return Ok(pagedResult);
        }

        /// <summary>
        /// Returns a single task by id. Only accessible by the task owner.
        /// </summary>
        [HttpGet("{id}")]
        [EnableRateLimiting("read")]
        public async Task<ActionResult<TaskResponseDto>> GetTaskById(int id)
        {
            var userId = GetCurrentUserId();
            var task = await _repository.GetByIdAsync(id, userId);

            if (task == null) return NotFound();

            return Ok(MapToResponseDto(task));
        }

        /// <summary>
        /// Creates a new task for the authenticated user.
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("write")]
        public async Task<ActionResult<TaskResponseDto>> CreateTask(TaskCreateDto createDto)
        {
            var userId = GetCurrentUserId();

            var categoryExists = await _repository.CategoryExistsAsync(createDto.CategoryId);
            if (!categoryExists)
                return NotFound(new { message = $"Category with id {createDto.CategoryId} does not exist." });

            var newTask = new TaskItem
            {
                Title = createDto.Title,
                Description = createDto.Description,
                IsCompleted = createDto.IsCompleted,
                CategoryId = createDto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _repository.CreateAsync(newTask);
            await _repository.SaveChangesAsync();

            var createdTask = await _repository.GetByIdAsync(newTask.Id, userId);
            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, MapToResponseDto(createdTask!));
        }

        /// <summary>
        /// Replaces an existing task entirely. Only accessible by the task owner.
        /// </summary>
        [HttpPut("{id}")]
        [EnableRateLimiting("write")]
        public async Task<ActionResult> UpdateTask(int id, TaskCreateDto updateDto)
        {
            var userId = GetCurrentUserId();
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

        /// <summary>
        /// Partially updates a task. Only provided fields are updated.
        /// </summary>
        [EnableRateLimiting("write")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> PatchTask(int id, TaskPatchDto patchDto)
        {
            var userId = GetCurrentUserId();
            var existingTask = await _repository.GetByIdAsync(id, userId);

            if (existingTask == null) return NotFound();

            if (patchDto.Title != null) existingTask.Title = patchDto.Title;
            if (patchDto.Description != null) existingTask.Description = patchDto.Description;
            if (patchDto.IsCompleted != null) existingTask.IsCompleted = patchDto.IsCompleted.Value;
            if (patchDto.CategoryId != null) existingTask.CategoryId = patchDto.CategoryId.Value;

            await _repository.UpdateAsync(existingTask);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Deletes a task. Only accessible by the task owner.
        /// </summary>
        [EnableRateLimiting("write")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            var userId = GetCurrentUserId();
            var task = await _repository.GetByIdAsync(id, userId);

            if (task == null) return NotFound();

            await _repository.DeleteAsync(task);
            await _repository.SaveChangesAsync();

            return NoContent();
        }
    }
}