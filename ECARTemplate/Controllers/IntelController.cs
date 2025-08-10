using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECARTemplate.Controllers
{
    [Authorize(AuthenticationSchemes = "Custom")]

    public class IntelController : Controller
    {
        public IActionResult AnalyticsDashboard() => View();

        public IActionResult Introduction() => View();

        public IActionResult MarketingDashboard() => View();

        public IActionResult Privacy() => View();
    }
}
