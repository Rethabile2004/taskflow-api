using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;
namespace TaskFlowAPI.Repositories
{
    // The contract — defines what operations are available
    // Controller depends on this interface, not the concrete class
    public interface ITaskRepository
    {
        // Returns all tasks from the database
        Task<IEnumerable<TaskItem>> GetAllAsync(TaskQueryParameters parameters);

        // Returns a single task by id, or null if not found
        Task<TaskItem?> GetByIdAsync(int id);

        // Adds a new task to the database
        Task<TaskItem> CreateAsync(TaskItem task);

        // Updates an existing task
        Task UpdateAsync(TaskItem task);

        // Deletes a task by id
        Task DeleteAsync(TaskItem task);

        // Persists all pending changes to the database
        Task<bool> SaveChangesAsync();
    }
}
