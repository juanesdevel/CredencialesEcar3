using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECARTemplate.Controllers
{
    [Authorize(AuthenticationSchemes = "Custom")]
    public class MiscellaneousController : Controller
    {
        public IActionResult Fullcalendar() => View();
        public IActionResult Lightgallery() => View();
        public IActionResult Treeview() => View();
    }
}
