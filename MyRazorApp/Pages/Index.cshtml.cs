using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using MyRazorApp.Models;
#nullable disable

namespace MyRazorApp.Pages
{
    public class IndexModel : PageModel
    {
        public static List<ClassInformationModel> ClassList { get; set; } = new();
        public static int _idCounter = 1;

        [BindProperty]
        public ClassInformationModel NewClass { get; set; } = new ClassInformationModel();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public List<ClassInformationModel> FilteredClasses { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public void OnGet()
        {
            if (ClassList.Count == 0)
            {
                GenerateDummyData();
            }

            var query = ClassList.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.ToLower();
                query = query.Where(c =>
                    (!string.IsNullOrEmpty(c.ClassName) && c.ClassName.ToLower().Contains(lowerSearch)) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(lowerSearch)) ||
                    c.StudentCount.ToString().Contains(lowerSearch));
            }

            TotalPages = (int)System.Math.Ceiling(query.Count() / (double)PageSize);

            FilteredClasses = query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        private void GenerateDummyData()
        {
            var random = new Random();
            string[] classNames = { "Math", "Science", "History", "Physics", "Chemistry", "Biology", "Music", "Art", "Computer Science", "English" };

            for (int i = 1; i <= 100; i++)
            {
                ClassList.Add(new ClassInformationModel
                {
                    Id = _idCounter++,
                    ClassName = classNames[random.Next(classNames.Length)] + $" {i}",
                    StudentCount = random.Next(10, 51),
                    Description = $"This is a description for class {i}."
                });
            }
        }

        public IActionResult OnPostAdd()
        {
            if (NewClass == null || !ModelState.IsValid)
            {
                return Page();
            }

            if (NewClass.Id == 0)
            {
                NewClass.Id = _idCounter++;
                ClassList.Add(new ClassInformationModel
                {
                    Id = NewClass.Id,
                    ClassName = NewClass.ClassName,
                    StudentCount = NewClass.StudentCount,
                    Description = NewClass.Description
                });
            }
            else
            {
                var existingClass = ClassList.FirstOrDefault(c => c.Id == NewClass.Id);
                if (existingClass != null)
                {
                    existingClass.ClassName = NewClass.ClassName;
                    existingClass.StudentCount = NewClass.StudentCount;
                    existingClass.Description = NewClass.Description;
                }
            }

            NewClass = new ClassInformationModel();
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            var classToDelete = ClassList.FirstOrDefault(c => c.Id == id);
            if (classToDelete != null)
            {
                ClassList.Remove(classToDelete);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostEdit(int id)
        {
            var classToEdit = ClassList.FirstOrDefault(c => c.Id == id);
            if (classToEdit != null)
            {
                NewClass = new ClassInformationModel
                {
                    Id = classToEdit.Id,
                    ClassName = classToEdit.ClassName,
                    StudentCount = classToEdit.StudentCount,
                    Description = classToEdit.Description
                };
            }

            return Page();
        }

        public IActionResult OnPostUpdate()
        {
            var existingClass = ClassList.FirstOrDefault(c => c.Id == NewClass.Id);
            if (existingClass != null)
            {
                existingClass.ClassName = NewClass.ClassName;
                existingClass.StudentCount = NewClass.StudentCount;
                existingClass.Description = NewClass.Description;
            }

            return RedirectToPage();
        }
    }
}
