namespace CarRentingSystem.Controllers.Api
{
    using CarRentingSystem.Models.Api.Cars;
    using CarRentingSystem.Services.Cars;
    using CarRentingSystem.Services.Cars.Models;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/cars")]
    public class CarsApiController : ControllerBase
    {
        private readonly ICarService cars;

        public CarsApiController(ICarService cars) 
            => this.cars = cars;

        [HttpGet]
        public async Task<CarQueryServiceModel> All([FromQuery] AllCarsApiRequestModel query) 
            => await this.cars.AllAsync(
                query.Brand,
                query.SearchTerm,
                query.Sorting,
                query.CurrentPage,
                query.CarsPerPage);
    }
}
