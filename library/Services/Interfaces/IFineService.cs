using OneBeyondApi.Model;

namespace OneBeyondApi.Services.Interfaces
{
    public interface IFineService
    {
        Task CreateFineAsync(Fine fine);
    }
}
