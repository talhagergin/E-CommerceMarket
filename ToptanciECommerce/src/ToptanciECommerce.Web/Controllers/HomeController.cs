using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Web.Models;

namespace ToptanciECommerce.Web.Controllers;

public class HomeController : Controller
{
    private readonly IUnitOfWork _uow;

    public HomeController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IActionResult> Index()
    {
        var featuredProducts = await _uow.Products.GetFeaturedAsync(8);
        var categories = await _uow.Categories.GetCategoriesWithSubsAsync();

        ViewBag.FeaturedProducts = featuredProducts;
        ViewBag.Categories = categories;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(string? rid = null)
    {
        var requestId = rid ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View(new ErrorViewModel { RequestId = requestId });
    }
}
