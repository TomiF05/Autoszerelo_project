using Shared;

namespace Frontend.Services
{
    public interface IWorkRepostory
    {
        Task<List<WorkDto>> GetAllWorksAsync();

        Task<WorkDto> GetWorkByIdAsync(int id);

        Task AddWorkAsync(AddWorkRequest request);

        Task UpdateWorkAsync(int id, UpdateWorkRequest request);

        Task DeleteWorkAsync(int id);
    }
}
