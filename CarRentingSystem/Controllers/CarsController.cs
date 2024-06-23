namespace CarRentingSystem.Controllers
{
    using AutoMapper;
    using CarRentingSystem.Infrastructure.Extensions;
    using CarRentingSystem.Models.Cars;
    using CarRentingSystem.Services.Cars;
    using CarRentingSystem.Services.Dealers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using static WebConstants;

    public class CarsController : Controller
    {
        private readonly ICarService cars;
        private readonly IDealerService dealers;
        private readonly IMapper mapper;

        public CarsController(
            ICarService cars,
            IDealerService dealers,
            IMapper mapper)
        {
            this.cars = cars;
            this.dealers = dealers;
            this.mapper = mapper;
        }

        public async Task<IActionResult> All([FromQuery] AllCarsQueryModel query)
        {
            var queryResult = await this.cars.AllAsync(
                query.Brand,
                query.SearchTerm,
                query.Sorting,
                query.CurrentPage,
                AllCarsQueryModel.CarsPerPage);

            var carBrands = await this.cars.AllBrandsAsync();

            query.Brands = carBrands;
            query.TotalCars = queryResult.TotalCars;
            query.Cars = queryResult.Cars;

            return View(query);
        }

        [Authorize]
        public async Task<IActionResult> Mine()
        {
            var myCars = await this.cars.ByUserAsync(this.User.Id());

            return View(myCars);
        }

        public async Task<IActionResult> Details(int id, string information)
        {
            var car = await this.cars.DetailsAsync(id);

            if (information != car.GetInformation())
            {
                return BadRequest();
            }

            return View(car);
        }

        [Authorize]
        public async Task<IActionResult> Add()
        {
            if (!await this.dealers.IsDealerAsync(this.User.Id()))
            {
                return RedirectToAction(nameof(DealersController.Become), "Dealers");
            }

            var categories = await this.cars.AllCategoriesAsync();

            return View(new CarFormModel { Categories = categories });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(CarFormModel car)
        {
            var dealerId = await this.dealers.IdByUserAsync(this.User.Id());

            if (dealerId == 0)
            {
                return RedirectToAction(nameof(DealersController.Become), "Dealers");
            }

            if (!await this.cars.CategoryExistsAsync(car.CategoryId))
            {
                this.ModelState.AddModelError(nameof(car.CategoryId), "Category does not exist.");
            }

            if (!ModelState.IsValid)
            {
                car.Categories = await this.cars.AllCategoriesAsync();

                return View(car);
            }

            var carId = await this.cars.CreateAsync(
                car.Brand,
                car.Model,
                car.Description,
                car.ImageUrl,
                car.Year,
                car.CategoryId,
                dealerId);

            TempData[GlobalMessageKey] = "You car was added and is awaiting for approval!";

            return RedirectToAction(nameof(Details), new { id = carId, information = car.GetInformation() });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = this.User.Id();

            if (!await this.dealers.IsDealerAsync(userId) && !User.IsAdmin())
            {
                return RedirectToAction(nameof(DealersController.Become), "Dealers");
            }

            var car = await this.cars.DetailsAsync(id);

            if (car.UserId != userId && !User.IsAdmin())
            {
                return Unauthorized();
            }

            var carForm = this.mapper.Map<CarFormModel>(car);

            carForm.Categories = await this.cars.AllCategoriesAsync();

            return View(carForm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, CarFormModel car)
        {
            var dealerId = await this.dealers.IdByUserAsync(this.User.Id());

            if (dealerId == 0 && !User.IsAdmin())
            {
                return RedirectToAction(nameof(DealersController.Become), "Dealers");
            }

            if (!await this.cars.CategoryExistsAsync(car.CategoryId))
            {
                this.ModelState.AddModelError(nameof(car.CategoryId), "Category does not exist.");
            }

            if (!ModelState.IsValid)
            {
                car.Categories = await this.cars.AllCategoriesAsync();

                return View(car);
            }

            if (!await this.cars.IsByDealerAsync(id, dealerId) && !User.IsAdmin())
            {
                return BadRequest();
            }

            var edited = await this.cars.EditAsync(
                id,
                car.Brand,
                car.Model,
                car.Description,
                car.ImageUrl,
                car.Year,
                car.CategoryId,
                this.User.IsAdmin());

            if (!edited)
            {
                return BadRequest();
            }

            TempData[GlobalMessageKey] = $"You car was edited{(this.User.IsAdmin() ? string.Empty : " and is awaiting for approval")}!";

            return RedirectToAction(nameof(Details), new { id, information = car.GetInformation() });
        }
    }
}
