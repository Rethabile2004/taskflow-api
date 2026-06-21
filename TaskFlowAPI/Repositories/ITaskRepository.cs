using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;
namespace TaskFlowAPI.Repositories
{
    // The contract — defines what operations are available
    // Controller depends on this interface, not the concrete class
    public interface ITaskRepository
    {
        // Now returns a tuple, tuples allow us to return two values without a wrapper class
        // user id added... reposotory filters all queries by the loggedin user
        Task<(IEnumerable<TaskItem>,int TotalCount)> GetAllAsync(TaskQueryParameters parameters, int userId);

        // Returns a single task by id, or null if not found
        // user id... ensures that user can only fetch their own task
        Task<TaskItem?> GetByIdAsync(int id, int userId);

        // Adds a new task to the database
        Task<TaskItem> CreateAsync(TaskItem task);

        // Updates an existing task
        Task UpdateAsync(TaskItem task);

        // Deletes a task by id
        Task DeleteAsync(TaskItem task);

        // Persists all pending changes to the database
        Task<bool> SaveChangesAsync();
        // Check if a category exists
        Task<bool> CategoryExistsAsync(int categoryId);
    }
}
