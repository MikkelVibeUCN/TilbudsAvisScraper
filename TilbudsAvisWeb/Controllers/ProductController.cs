using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TilbudsAvisWeb.Controllers
{
    public class ProductController : Controller
    {
        // GET: ProductController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ProductController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }
    }
}
