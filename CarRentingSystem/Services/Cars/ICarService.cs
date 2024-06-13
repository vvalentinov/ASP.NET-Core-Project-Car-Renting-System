namespace CarRentingSystem.Services.Cars
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CarRentingSystem.Models;
    using CarRentingSystem.Services.Cars.Models;

    public interface ICarService
    {
        Task<CarQueryServiceModel> AllAsync(
            string brand = null,
            string searchTerm = null,
            CarSorting sorting = CarSorting.DateCreated, 
            int currentPage = 1,
            int carsPerPage = int.MaxValue,
            bool publicOnly = true);

        Task<IEnumerable<LatestCarServiceModel>> LatestAsync();

        Task<CarDetailsServiceModel> DetailsAsync(int carId);

        Task<int> CreateAsync(
            string brand,
            string model,
            string description,
            string imageUrl,
            int year,
            int categoryId,
            int dealerId);

        Task<bool> EditAsync(
            int carId,
            string brand,
            string model,
            string description,
            string imageUrl,
            int year,
            int categoryId,
            bool isPublic);

        Task<IEnumerable<CarServiceModel>> ByUserAsync(string userId);

        Task<bool> IsByDealerAsync(int carId, int dealerId);

        Task ChangeVisilityAsync(int carId);

        Task<IEnumerable<string>> AllBrandsAsync();

        Task<IEnumerable<CarCategoryServiceModel>> AllCategoriesAsync();

        Task<bool> CategoryExistsAsync(int categoryId);
    }
}
