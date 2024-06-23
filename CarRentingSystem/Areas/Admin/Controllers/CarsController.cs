namespace CarRentingSystem.Areas.Admin.Controllers
{
    using CarRentingSystem.Services.Cars;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    public class CarsController : AdminController
    {
        private readonly ICarService cars;

        public CarsController(ICarService cars) => this.cars = cars;

        public async Task<IActionResult> All()
        {
            var cars = (await this.cars.AllAsync(publicOnly: false)).Cars;

            return View(cars);
        }

        public async Task<IActionResult> ChangeVisibility(int id)
        {
            await this.cars.ChangeVisilityAsync(id);

            return RedirectToAction(nameof(All));
        }
    }
}
