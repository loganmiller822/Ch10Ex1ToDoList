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
            var model = new ToDoViewModel();

            model.Filters = new Filters(id);
            model.Categories = context.Categories.ToList();
            model.Statuses = context.Statuses.ToList();
            model.DueFilters = Filters.DueFilterValues;

            IQueryable<ToDo> query = context.ToDos
                .Include(t => t.Category).Include(t => t.Status);

            if (model.Filters.HasCategory)
            {
                query = query.Where(t => t.CategoryId == model.Filters.CategoryId);
            }
            if (model.Filters.HasStatus)
            {
                query = query.Where(t => t.StatusId == model.Filters.StatusId);
            }
            if (model.Filters.HasDue)
            {
                var today = DateTime.Today;
                if (model.Filters.IsPast)
                    query = query.Where(t => t.DueDate < today);
                else if (model.Filters.IsFuture)
                    query = query.Where(t => t.DueDate > today);
                else if (model.Filters.IsToday)
                    query = query.Where(t => t.DueDate == today);
            }

            model.Tasks = query.OrderBy(t => t.DueDate).ToList();

            return View(model);
        }


        public IActionResult Add()
        {
            var model = new ToDoViewModel();
            model.Categories = context.Categories.ToList();
            model.Statuses = context.Statuses.ToList();
            return View(model);
        }

        [HttpPost]
        public IActionResult Add(ToDoViewModel model)
        {
            if (ModelState.IsValid)
            {
                context.ToDos.Add(model.CurrentTask);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                model.Categories = context.Categories.ToList();
                model.Statuses = context.Statuses.ToList();
                return View(model);
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