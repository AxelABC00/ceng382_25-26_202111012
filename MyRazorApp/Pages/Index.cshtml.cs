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
        public ClassInformationModel NewClass { get; set; } = new();

        public void OnGet()
        {
        }

        public IActionResult OnPostSave()
        {
            if (!ModelState.IsValid)
                return Page();

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
                
                var existing = ClassList.FirstOrDefault(c => c.Id == NewClass.Id);
                if (existing != null)
                {
                    existing.ClassName = NewClass.ClassName;
                    existing.StudentCount = NewClass.StudentCount;
                    existing.Description = NewClass.Description;
                }
            }

            
            NewClass = new ClassInformationModel();
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

        public IActionResult OnPostDelete(int id)
        {
            var classToDelete = ClassList.FirstOrDefault(c => c.Id == id);
            if (classToDelete != null)
            {
                ClassList.Remove(classToDelete);
            }

            return RedirectToPage();
        }
    }
}
