using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using my_demo_application.Application.Interfaces;
using my_demo_application.Models;

namespace my_demo_application.Controllers;

public class HomeController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IPatientService _patientService;

    public HomeController(IAdminService adminService, IPatientService patientService)
    {
        _adminService = adminService;
        _patientService = patientService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Departments = await _adminService.GetAllDepartmentsAsync();
        ViewBag.FeaturedDoctors = await _patientService.SearchDoctorsAsync();
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
