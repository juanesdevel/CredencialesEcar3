using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECARTemplate.Controllers
{
    [Authorize(AuthenticationSchemes = "Custom")]
    public class DocsController : Controller
    {
        public IActionResult Buildnotes() => View();
        public IActionResult CommunitySupport() => View();
        public IActionResult FlavorsEditions() => View();
        public IActionResult General() => View();
        public IActionResult Licensing() => View();
    }
}
