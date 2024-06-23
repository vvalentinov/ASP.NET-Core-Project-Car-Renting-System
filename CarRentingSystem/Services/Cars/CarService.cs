namespace CarRentingSystem.Services.Cars
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using CarRentingSystem.Data;
    using CarRentingSystem.Data.Models;
    using CarRentingSystem.Models;
    using CarRentingSystem.Services.Cars.Models;
    using Microsoft.EntityFrameworkCore;

    public class CarService : ICarService
    {
        private readonly CarRentingDbContext data;
        private readonly IConfigurationProvider mapper;

        public CarService(CarRentingDbContext data, IMapper mapper) 
		{
			this.data = data;
			this.mapper = mapper.ConfigurationProvider;
		}

        public async Task<CarQueryServiceModel> AllAsync(
            string brand = null,
            string searchTerm = null,
            CarSorting sorting = CarSorting.DateCreated,
            int currentPage = 1,
            int carsPerPage = int.MaxValue,
            bool publicOnly = true)
        {
            var carsQuery = this.data.Cars
                .Where(c => !publicOnly || c.IsPublic);

            if (!string.IsNullOrWhiteSpace(brand))
            {
                carsQuery = carsQuery.Where(c => c.Brand == brand);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                carsQuery = carsQuery.Where(c =>
                    (c.Brand + " " + c.Model).ToLower().Contains(searchTerm.ToLower()) ||
                    c.Description.ToLower().Contains(searchTerm.ToLower()));
            }

            carsQuery = sorting switch
            {
                CarSorting.Year => carsQuery.OrderByDescending(c => c.Year),
                CarSorting.BrandAndModel => carsQuery.OrderBy(c => c.Brand).ThenBy(c => c.Model),
                CarSorting.DateCreated or _ => carsQuery.OrderByDescending(c => c.Id)
            };

            var totalCars = await carsQuery.CountAsync();

            var cars = await GetCarsAsync(carsQuery
                .Skip((currentPage - 1) * carsPerPage)
                .Take(carsPerPage));

            return new CarQueryServiceModel
            {
                TotalCars = totalCars,
                CurrentPage = currentPage,
                CarsPerPage = carsPerPage,
                Cars = cars
            };
        }

        public async Task<IEnumerable<LatestCarServiceModel>> LatestAsync()
            => await this.data
                .Cars
                .Where(c => c.IsPublic)
                .OrderByDescending(c => c.Id)
                .ProjectTo<LatestCarServiceModel>(this.mapper)
                .Take(3)
                .ToListAsync();

        public async Task<CarDetailsServiceModel> DetailsAsync(int id)
            => await this.data
                .Cars
                .Where(c => c.Id == id)
                .ProjectTo<CarDetailsServiceModel>(this.mapper)
                .FirstOrDefaultAsync();

        public async Task<int> CreateAsync(
            string brand,
            string model,
            string description,
            string imageUrl,
            int year,
            int categoryId,
            int dealerId)
        {
            var carData = new Car
            {
                Brand = brand,
                Model = model,
                Description = description,
                ImageUrl = imageUrl,
                Year = year,
                CategoryId = categoryId,
                DealerId = dealerId,
                IsPublic = false
            };

            await this.data.Cars.AddAsync(carData);
            await this.data.SaveChangesAsync();

            return carData.Id;
        }

        public async Task<bool> EditAsync(
            int id, 
            string brand, 
            string model, 
            string description, 
            string imageUrl, 
            int year, 
            int categoryId,
            bool isPublic)
        {
            var carData = await this.data.Cars.FindAsync(id);

            if (carData == null)
            {
                return false;
            }

            carData.Brand = brand;
            carData.Model = model;
            carData.Description = description;
            carData.ImageUrl = imageUrl;
            carData.Year = year;
            carData.CategoryId = categoryId;
            carData.IsPublic = isPublic;

            await this.data.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<CarServiceModel>> ByUserAsync(string userId)
            => await GetCarsAsync(this.data.Cars.Where(c => c.Dealer.UserId == userId));

        public async Task<bool> IsByDealerAsync(int carId, int dealerId)
            => await this.data.Cars.AnyAsync(c => c.Id == carId && c.DealerId == dealerId);

        public async Task ChangeVisilityAsync(int carId)
        {
            var car = await this.data.Cars.FindAsync(carId);

            car.IsPublic = !car.IsPublic;

            await this.data.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> AllBrandsAsync()
            => await this.data
                .Cars
                .Select(c => c.Brand)
                .Distinct()
                .OrderBy(br => br)
                .ToListAsync();

        public async Task<IEnumerable<CarCategoryServiceModel>> AllCategoriesAsync()
            => await this.data.Categories.ProjectTo<CarCategoryServiceModel>(this.mapper).ToListAsync();

        public async Task<bool> CategoryExistsAsync(int categoryId)
            => await this.data.Categories.AnyAsync(c => c.Id == categoryId);

        private async Task<IEnumerable<CarServiceModel>> GetCarsAsync(IQueryable<Car> carQuery)
            => await carQuery.ProjectTo<CarServiceModel>(this.mapper).ToListAsync();
    }
}
