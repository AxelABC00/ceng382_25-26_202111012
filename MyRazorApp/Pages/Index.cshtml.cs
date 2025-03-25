using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using MyRazorApp.Models;

namespace MyRazorApp.Pages
{
    public class IndexModel : PageModel
    {
        public static List<ClassInformationModel> ClassList { get; set; } = new();
        public static int _idCounter = 1;

        [BindProperty]
        public ClassInformationModel NewClass { get; set; } = new ClassInformationModel();

        [BindProperty]
        public int EditId { get; set; }

        public void OnGet()
        {
            if (!ClassList.Any())
            {
                ClassList.Add(new ClassInformationModel
                {
                    Id = _idCounter++,
                    ClassName = "Computer Science 101",
                    StudentCount = 30,
                    Description = "Introduction to Computer Science"
                });
            }
        }

        public ClassInformationModel GetNewClass()
        {
            return NewClass;
        }

        public IActionResult OnPostAdd(ClassInformationModel newClass)
        {
            if (NewClass == null || !ModelState.IsValid)
            {
                return Page();
            }

            newClass.Id = _idCounter++;
            ClassList.Add(NewClass);

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
