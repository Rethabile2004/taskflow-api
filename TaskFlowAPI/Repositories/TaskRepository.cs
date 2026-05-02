using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Data;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Repositories
{
    // The concrete implementation — this is the only class that touches EF Core
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        // AppDbContext is injected by the DI container
        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        // Fetch all tasks from the Tasks table
        public async Task<IEnumerable<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks.Include(t=>t.Category).ToListAsync();
        }

        // Fetch a single task — returns null if not found
        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks.Include(t=>t.Category).FirstOrDefaultAsync(t => t.Id == id);
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