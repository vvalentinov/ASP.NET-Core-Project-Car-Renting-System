namespace CarRentingSystem.Services.Dealers
{
    using System.Threading.Tasks;

    public interface IDealerService
    {
        Task<bool> IsDealerAsync(string userId);

        Task<int> IdByUserAsync(string userId);
    }
}
