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

        public async Task CreateFineAsync(Fine fine)
        {
            if (fine == null)
                throw new ArgumentNullException(nameof(fine));

            _context.Fines.Add(fine);
            await _context.SaveChangesAsync();
        }
    }
}
