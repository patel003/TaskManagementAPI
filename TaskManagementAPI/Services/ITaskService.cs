using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Services
{
    public interface ITaskService
    {
        Task<TaskResponseDto>CreateTaskAsync(CreateTaskdto Createtaskdto, int userId);
        Task<List<TaskResponseDto>> GetUserTaskAsync(int userId);
        Task<TaskResponseDto> GetTaskByIdAsync(int taskId, int userId);
        Task<TaskResponseDto> UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto, int userId);
        Task<bool> DeleteTaskAsync(int taskId ,int userId);
        Task<TaskResponseDto> ToggelTaskComletionAsync(int taskId , int userId);
    }
}
