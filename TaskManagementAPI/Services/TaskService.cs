using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Services
{
    public class TaskService:ITaskService
    {
        private readonly ApplicationDbContext _context;
        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskdto createTaskdto, int userId)
        {
            var task = new TaskItem
            {
                Title = createTaskdto.Title,
                Description = createTaskdto.Description,
                Priority = createTaskdto.Priority,
                DueDate = createTaskdto.DueDate,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            }
            ;
            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate
            };
        }
        public async Task<List<TaskResponseDto>> GetUserTaskAsync(int userId)
        {
            var tasks = await _context.TaskItems
                .Where(t => t.UserId == userId)
                .OrderByDescending(t=> t.CreatedAt)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UserId = t.UserId
                })
                .ToListAsync();

            return tasks;
        }

        public async Task<TaskResponseDto> GetTaskByIdAsync(int taskId, int userId)
        {
            var task = await _context.TaskItems
                .Where(t => t.Id == taskId && t.UserId == userId)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UserId = t.UserId
                })
                .FirstOrDefaultAsync();
            if (task == null)
            {
                throw new KeyNotFoundException("Task not found");
            }
            return task;
        }
    }
}
