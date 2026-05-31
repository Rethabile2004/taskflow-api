using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Data;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        // AppDbContext is injected by the DI container
        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        // Fetch all tasks from the Tasks table
        public async Task<(IEnumerable<TaskItem>, int TotalCount)> GetAllAsync(TaskQueryParameters queryParameters, int userId)
        {
            var query = _context.Tasks.Include(c => c.Category).AsQueryable();
            // users never see each others tasks
            query = query.Where(t => t.UserId == userId);
            if (queryParameters.IsCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == queryParameters.IsCompleted);
            }
            if (queryParameters.CategoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == queryParameters.CategoryId);
            }
            if (!string.IsNullOrWhiteSpace(queryParameters.SearchString))
            {
                query = query.Where(t => t.Title.Contains(queryParameters.SearchString));
            }
            var totalCount = await query.CountAsync();

            query = queryParameters.SortBy?.ToLower() switch
            {
                "title" => query.OrderBy(t => t.Title),
                "iscompleted" => query.OrderBy(t => t.IsCompleted),
                "createdat" => query.OrderByDescending(t => t.CreatedAt),
                _ => query.OrderBy(t => t.Id)
            };
            // Apply pagination after sort
            var tasks = query.Skip((queryParameters.Page - 1) * queryParameters.PageSize).Take(queryParameters.PageSize).ToList();

            return (tasks, totalCount);
        }

        // Fetch a single task — returns null if not found
        public async Task<TaskItem?> GetByIdAsync(int id, int userId)
        {
            return await _context.Tasks.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        // Add the new task to the context (not saved yet — SaveChangesAsync does that)
        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(task);
            return task;
        }

        // EF Core tracks the entity automatically — just mark it as modified
        public async Task UpdateAsync(TaskItem task)
        {
            _context.Tasks.Update(task);
            await Task.CompletedTask;
        }

        // Remove the entity from the context
        public async Task DeleteAsync(TaskItem task)
        {
            _context.Tasks.Remove(task);
            await Task.CompletedTask;
        }

        // Persist all pending changes to the database
        // Returns true if at least one row was affected
        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}