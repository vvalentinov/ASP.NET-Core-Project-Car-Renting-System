namespace CarRentingSystem.Services.Dealers
{
    using System.Linq;
    using System.Threading.Tasks;
    using CarRentingSystem.Data;
    using Microsoft.EntityFrameworkCore;

    public class DealerService : IDealerService
    {
        private readonly CarRentingDbContext data;

        public DealerService(CarRentingDbContext data) 
            => this.data = data;

        public async Task<bool> IsDealerAsync(string userId)
            => await this.data.Dealers.AnyAsync(d => d.UserId == userId);

        public async Task<int> IdByUserAsync(string userId)
            => await this.data
                .Dealers
                .Where(d => d.UserId == userId)
                .Select(d => d.Id)
                .FirstOrDefaultAsync();
    }
}
