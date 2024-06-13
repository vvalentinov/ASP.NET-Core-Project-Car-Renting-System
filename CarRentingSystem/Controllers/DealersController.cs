namespace CarRentingSystem.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using CarRentingSystem.Data;
    using CarRentingSystem.Data.Models;
    using CarRentingSystem.Infrastructure.Extensions;
    using CarRentingSystem.Models.Dealers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using static WebConstants;

    public class DealersController : Controller
    {
        private readonly CarRentingDbContext data;

        public DealersController(CarRentingDbContext data) 
            => this.data = data;

        [Authorize]
        public IActionResult Become() => View();

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Become(BecomeDealerFormModel dealer)
        {
            var userId = this.User.Id();

            var userIdAlreadyDealer = await this.data
                .Dealers
                .AnyAsync(d => d.UserId == userId);

            if (userIdAlreadyDealer)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(dealer);
            }

            var dealerData = new Dealer
            {
                Name = dealer.Name,
                PhoneNumber = dealer.PhoneNumber,
                UserId = userId
            };

            await this.data.Dealers.AddAsync(dealerData);
            await this.data.SaveChangesAsync();

            TempData[GlobalMessageKey] = "Thank you for becomming a dealer!";

            return RedirectToAction(nameof(CarsController.All), "Cars");
        }
    }
}
