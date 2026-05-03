using Shared;

namespace Frontend.Services
{
    public interface IWorkRepository
    {
        Task<List<WorkDto>> GetAllWorksAsync();

        Task<WorkDto> GetWorkByIdAsync(int id);

        Task AddWorkAsync(AddWorkRequest request);

        Task UpdateWorkAsync(int id, UpdateWorkRequest request);

        Task DeleteWorkAsync(int id);

        Task ChangeStatusAsync(int id);

        Task<List<WorkDto>> GetMyWorksAsync();
    }
}
