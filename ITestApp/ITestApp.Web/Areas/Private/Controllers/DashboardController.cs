﻿using ITestApp.Common.Providers;
using ITestApp.Data.Models;
using ITestApp.Services.Contracts;
using ITestApp.Web.Areas.Private.Models.DashboardViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace ITestApp.Web.Areas.Private.Controllers
{
    [Authorize]
    [Area("Private")]
    public class DashboardController : Controller
    {
        private readonly IMappingProvider mapper;
        private readonly ITestsService tests;
        private readonly IAnswersService answers;
        private readonly ICategoryService categories;
        private readonly IResultService results;
        private readonly UserManager<User> userManager;

        public DashboardController(IResultService results, 
            IMappingProvider mapper, 
            ITestsService tests, 
            IAnswersService answers, 
            ICategoryService categories, 
            UserManager<User> userManager)
        {
            this.mapper = mapper ?? throw new ArgumentNullException("Mapper can not be null");
            this.tests = tests ?? throw new ArgumentNullException("Tests service cannot be null");
            this.answers = answers ?? throw new ArgumentNullException("Answers service cannot be null");
            this.categories = categories ?? throw new ArgumentNullException("Categories service cannot be null");
            this.userManager = userManager ?? throw new ArgumentNullException("User manager cannot be null");
            this.results = results;
        }

        [HttpGet]
        [Authorize]
        public IActionResult All()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { Area = "Administration"});
            }

            var userId = this.userManager.GetUserId(HttpContext.User);
            var categories = this.categories.GetAll();
            
            var model = new DashboardViewModel()
            {
                Categories = this.mapper.ProjectTo<CategoryViewModel>(categories).ToList(),
            };

            foreach (var category in model.Categories)
            {
                var randomTest = tests.GetRandomTestByCategory(category.Name, userId);
                category.Test = mapper.MapTo<TestViewModel>(randomTest);
            }

            return View(model);
        }
    }
}
