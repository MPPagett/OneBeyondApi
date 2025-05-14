using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;
using OneBeyondApi.Services.Interfaces;

namespace OneBeyondApi.Services
{
    public class FineService : IFineService
    {
        private readonly LibraryContext _context;

        public FineService(LibraryContext context)
        {
            _context = context;
        }

        public Task CreateFineAsync(Fine fine)
        {
            throw new NotImplementedException();
        }
    }
}
