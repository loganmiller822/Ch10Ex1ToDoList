using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private ToDoContext context;
        public HomeController(ToDoContext ctx) => context = ctx;

        public IActionResult Index(string id)
        {
            var viewModel = new ToDoViewModel();

            viewModel.Filters = new Filters(id);
            viewModel.Categories = context.Categories.ToList();
            viewModel.Statuses = context.Statuses.ToList();
            viewModel.DueFilters = Filters.DueFilterValues;

            IQueryable<ToDo> query = context.ToDos
                .Include(t => t.Category).Include(t => t.Status);

            if (viewModel.Filters.HasCategory)
            {
                query = query.Where(t => t.CategoryId == viewModel.Filters.CategoryId);
            }
            if (viewModel.Filters.HasStatus)
            {
                query = query.Where(t => t.StatusId == viewModel.Filters.StatusId);
            }
            if (viewModel.Filters.HasDue)
            {
                var today = DateTime.Today;
                if (viewModel.Filters.IsPast)
                    query = query.Where(t => t.DueDate < today);
                else if (viewModel.Filters.IsFuture)
                    query = query.Where(t => t.DueDate > today);
                else if (viewModel.Filters.IsToday)
                    query = query.Where(t => t.DueDate == today);
            }

            viewModel.Tasks = query.OrderBy(t => t.DueDate).ToList();

            return View(viewModel);
        }


        public IActionResult Add()
        {
            ViewBag.Categories = context.Categories.ToList();
            ViewBag.Statuses = context.Statuses.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Add(ToDo task)
        {
            if (ModelState.IsValid)
            {
                context.ToDos.Add(task);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Categories = context.Categories.ToList();
                ViewBag.Statuses = context.Statuses.ToList();
                return View(task);
            }
        }

        [HttpPost]
        public IActionResult Filter(string[] filter)
        {
            string id = string.Join('-', filter);
            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult Edit([FromRoute]string id, ToDo selected)
        {
            if (selected.StatusId == null) {
                context.ToDos.Remove(selected);
            }
            else {
                string newStatusId = selected.StatusId;
                selected = context.ToDos.Find(selected.Id);
                selected.StatusId = newStatusId;
                context.ToDos.Update(selected);
            }
            context.SaveChanges();

            return RedirectToAction("Index", new { ID = id });
        }
    }
}