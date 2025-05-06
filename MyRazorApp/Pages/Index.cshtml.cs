

/*@*
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyRazorApp.Models;
using MyRazorApp.Helpers;
using MyRazorApp.Data; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace MyRazorApp.Pages.Classes
{
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Class NewClass { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public List<Class> FilteredClasses { get; set; } = new();

        [BindProperty]
        public string? selectedColumns { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            await ApplyFilteringAndPagingAsync();
            return Page();
        }

        private async Task ApplyFilteringAndPagingAsync()
        {
            var query = _context.Classes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.ToLower();
                query = query.Where(c =>
                    (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(lowerSearch)) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(lowerSearch)) ||
                    c.PersonCount.ToString().Contains(lowerSearch));
            }

            TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)PageSize);

            FilteredClasses = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            if (!ModelState.IsValid)
                return Page();

            if (NewClass.Id == 0)
            {
                _context.Classes.Add(NewClass);
            }
            else
            {
                var existingClass = await _context.Classes.FindAsync(NewClass.Id);
                if (existingClass != null)
                {
                    existingClass.Name = NewClass.Name;
                    existingClass.PersonCount = NewClass.PersonCount;
                    existingClass.Description = NewClass.Description;
                    existingClass.IsActive = NewClass.IsActive;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var classToDelete = await _context.Classes.FindAsync(id);
            if (classToDelete != null)
            {
                _context.Classes.Remove(classToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var classToEdit = await _context.Classes.FindAsync(id);
            if (classToEdit != null)
            {
                NewClass = new Class
                {
                    Id = classToEdit.Id,
                    Name = classToEdit.Name,
                    PersonCount = classToEdit.PersonCount,
                    Description = classToEdit.Description,
                    IsActive = classToEdit.IsActive
                };
            }

            await ApplyFilteringAndPagingAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var existingClass = await _context.Classes.FindAsync(NewClass.Id);
            if (existingClass != null)
            {
                existingClass.Name = NewClass.Name;
                existingClass.PersonCount = NewClass.PersonCount;
                existingClass.Description = NewClass.Description;
                existingClass.IsActive = NewClass.IsActive;

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportJsonAsync(bool isFiltered)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var columnIndexes = new List<int>();
            if (!string.IsNullOrWhiteSpace(selectedColumns))
            {
                columnIndexes = selectedColumns
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => int.TryParse(x, out _))
                    .Select(int.Parse)
                    .ToList();
            }

            List<Class> dataToExport;

            if (isFiltered)
            {
                var query = _context.Classes.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    var lowerSearch = SearchTerm.ToLower();
                    query = query.Where(c =>
                        (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(lowerSearch)) ||
                        (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(lowerSearch)) ||
                        c.PersonCount.ToString().Contains(lowerSearch));
                }

                dataToExport = await query
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
            }
            else
            {
                dataToExport = await _context.Classes.ToListAsync();
            }

            var reducedData = dataToExport
                .Where(item => item != null)
                .Select(item =>
                {
                    var dict = new Dictionary<string, object>();
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(0))
                        dict["Name"] = item.Name;
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(1))
                        dict["PersonCount"] = item.PersonCount;
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(2))
                        dict["Description"] = item.Description;
                    return dict;
                }).ToList();

            var json = JsonExportUtils.Instance.SerializeToJson(reducedData);
            var fileName = isFiltered ? "filtered_classes.json" : "all_classes.json";
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(fileBytes, "application/json", fileName);
        }

        public IActionResult OnPostLogout()
        {
            Response.Cookies.Delete("username");
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("session_id");

            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }

        private bool IsAuthenticated()
        {
            var sessionUsername = HttpContext.Session.GetString("username");
            var sessionToken = HttpContext.Session.GetString("token");
            var sessionId = HttpContext.Session.GetString("session_id");

            var cookieUsername = Request.Cookies["username"];
            var cookieToken = Request.Cookies["token"];
            var cookieSessionId = Request.Cookies["session_id"];

            return sessionUsername == cookieUsername && sessionToken == cookieToken && sessionId == cookieSessionId;
        }
    }
}



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyRazorApp.Models;
using MyRazorApp.Helpers;
using MyRazorApp.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace MyRazorApp.Pages.Classes
{
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Class NewClass { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public List<Class> FilteredClasses { get; set; } = new();

        [BindProperty]
        public string? selectedColumns { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            await ApplyFilteringAndPagingAsync();
            return Page();
        }

        private async Task ApplyFilteringAndPagingAsync()
        {
            var query = _context.Classes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.ToLower();
                query = query.Where(c =>
                    (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(lowerSearch)) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(lowerSearch)) ||
                    c.PersonCount.ToString().Contains(lowerSearch));
            }

            TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)PageSize);

            FilteredClasses = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            if (!ModelState.IsValid)
                return Page();

            if (NewClass.Id == 0)
            {
                _context.Classes.Add(NewClass);
            }
            else
            {
                var existingClass = await _context.Classes.FindAsync(NewClass.Id);
                if (existingClass != null)
                {
                    existingClass.Name = NewClass.Name;
                    existingClass.PersonCount = NewClass.PersonCount;
                    existingClass.Description = NewClass.Description;
                    existingClass.IsActive = NewClass.IsActive;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var classToDelete = await _context.Classes.FindAsync(id);
            if (classToDelete != null)
            {
                _context.Classes.Remove(classToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var classToEdit = await _context.Classes.FindAsync(id);
            if (classToEdit != null)
            {
                NewClass = new Class
                {
                    Id = classToEdit.Id,
                    Name = classToEdit.Name,
                    PersonCount = classToEdit.PersonCount,
                    Description = classToEdit.Description,
                    IsActive = classToEdit.IsActive
                };
            }

            await ApplyFilteringAndPagingAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var existingClass = await _context.Classes.FindAsync(NewClass.Id);
            if (existingClass != null)
            {
                existingClass.Name = NewClass.Name;
                existingClass.PersonCount = NewClass.PersonCount;
                existingClass.Description = NewClass.Description;
                existingClass.IsActive = NewClass.IsActive;

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportJsonAsync(bool isFiltered)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var columnIndexes = new List<int>();
            if (!string.IsNullOrWhiteSpace(selectedColumns))
            {
                columnIndexes = selectedColumns
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => int.TryParse(x, out _))
                    .Select(int.Parse)
                    .ToList();
            }

            List<Class> dataToExport;

            if (isFiltered)
            {
                var query = _context.Classes.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    var lowerSearch = SearchTerm.ToLower();
                    query = query.Where(c =>
                        (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(lowerSearch)) ||
                        (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(lowerSearch)) ||
                        c.PersonCount.ToString().Contains(lowerSearch));
                }

                dataToExport = await query
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();
            }
            else
            {
                dataToExport = await _context.Classes.ToListAsync();
            }

            var reducedData = dataToExport
                .Where(item => item != null)
                .Select(item =>
                {
                    var dict = new Dictionary<string, object>();
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(0))
                        dict["Name"] = item.Name;
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(1))
                        dict["PersonCount"] = item.PersonCount;
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(2))
                        dict["Description"] = item.Description;
                    return dict;
                }).ToList();

            var json = JsonExportUtils.Instance.SerializeToJson(reducedData);
            var fileName = isFiltered ? "filtered_classes.json" : "all_classes.json";
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(fileBytes, "application/json", fileName);
        }

        public IActionResult OnPostLogout()
        {
            Response.Cookies.Delete("username");
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("session_id");

            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }

        private bool IsAuthenticated()
        {
            var sessionUsername = HttpContext.Session.GetString("username");
            var sessionToken = HttpContext.Session.GetString("token");
            var sessionId = HttpContext.Session.GetString("session_id");

            var cookieUsername = Request.Cookies["username"];
            var cookieToken = Request.Cookies["token"];
            var cookieSessionId = Request.Cookies["session_id"];

            return sessionUsername == cookieUsername && sessionToken == cookieToken && sessionId == cookieSessionId;
        }
    }
}
*@*/

/*
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyRazorApp.Models;
using MyRazorApp.Helpers;
using MyRazorApp.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace MyRazorApp.Pages.Classes
{
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ClassInformationModel NewClass { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        [BindProperty]
        public string? selectedColumns { get; set; }

        public List<ClassInformationModel> FilteredClasses { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            await ApplyFilteringAndPagingAsync();
            return Page();
        }

        private async Task ApplyFilteringAndPagingAsync()
        {
            var query = _context.Classes
                .Where(c => c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.ToLower();
                query = query.Where(c =>
                    (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(lowerSearch)) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(lowerSearch)) ||
                    c.PersonCount.ToString().Contains(lowerSearch));
            }

            TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)PageSize);

            FilteredClasses = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new ClassInformationModel
                {
                    Id = c.Id,
                    ClassName = c.Name,
                    StudentCount = c.PersonCount,
                    Description = c.Description
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            if (!TryValidateModel(NewClass))
                return Page();

            var entity = new Class
            {
                Name = NewClass.ClassName,
                PersonCount = NewClass.StudentCount,
                Description = NewClass.Description,
                IsActive = true
            };

            _context.Classes.Add(entity);
            await _context.SaveChangesAsync();

            NewClass = new(); // formu sıfırla
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var classToEdit = await _context.Classes.FindAsync(id);
            if (classToEdit != null && classToEdit.IsActive)
            {
                NewClass = new ClassInformationModel
                {
                    Id = classToEdit.Id,
                    ClassName = classToEdit.Name,
                    StudentCount = classToEdit.PersonCount,
                    Description = classToEdit.Description
                };
            }

            await ApplyFilteringAndPagingAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            if (!TryValidateModel(NewClass))
                return Page();

            var existingClass = await _context.Classes.FindAsync(NewClass.Id);
            if (existingClass != null && existingClass.IsActive)
            {
                existingClass.Name = NewClass.ClassName;
                existingClass.PersonCount = NewClass.StudentCount;
                existingClass.Description = NewClass.Description;

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var classToDelete = await _context.Classes.FindAsync(id);
            if (classToDelete != null)
            {
                classToDelete.IsActive = false; // soft delete
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportJsonAsync(bool isFiltered)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var columnIndexes = new List<int>();
            if (!string.IsNullOrWhiteSpace(selectedColumns))
            {
                columnIndexes = selectedColumns
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => int.TryParse(x, out _))
                    .Select(int.Parse)
                    .ToList();
            }

            var query = _context.Classes.Where(c => c.IsActive).AsQueryable();

            if (isFiltered && !string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.ToLower();
                query = query.Where(c =>
                    (!string.IsNullOrEmpty(c.Name) && c.Name.ToLower().Contains(lowerSearch)) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(lowerSearch)) ||
                    c.PersonCount.ToString().Contains(lowerSearch));
            }

            var dataToExport = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new ClassInformationModel
                {
                    Id = c.Id,
                    ClassName = c.Name,
                    StudentCount = c.PersonCount,
                    Description = c.Description
                }).ToListAsync();

            var reducedData = dataToExport
                .Select(item =>
                {
                    var dict = new Dictionary<string, object>();
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(0))
                        dict["ClassName"] = item.ClassName;
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(1))
                        dict["StudentCount"] = item.StudentCount;
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(2))
                        dict["Description"] = item.Description;
                    return dict;
                }).ToList();

            var json = JsonExportUtils.Instance.SerializeToJson(reducedData);
            var fileName = isFiltered ? "filtered_classes.json" : "all_classes.json";
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(fileBytes, "application/json", fileName);
        }

        public IActionResult OnPostLogout()
        {
            Response.Cookies.Delete("username");
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("session_id");

            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }

        private bool IsAuthenticated()
        {
            var sessionUsername = HttpContext.Session.GetString("username");
            var sessionToken = HttpContext.Session.GetString("token");
            var sessionId = HttpContext.Session.GetString("session_id");

            var cookieUsername = Request.Cookies["username"];
            var cookieToken = Request.Cookies["token"];
            var cookieSessionId = Request.Cookies["session_id"];

            return sessionUsername == cookieUsername &&
                   sessionToken == cookieToken &&
                   sessionId == cookieSessionId;
        }
    }
}
*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyRazorApp.Models;
using MyRazorApp.Helpers;
using MyRazorApp.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace MyRazorApp.Pages.Classes
{
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ClassInformationModel NewClass { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public List<ClassInformationModel> FilteredClasses { get; set; } = new();

        [BindProperty]
        public string? selectedColumns { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAuthenticated())
                return RedirectToPage("/Login");

            await ApplyFilteringAndPagingAsync();
            return Page();
        }

        private async Task ApplyFilteringAndPagingAsync()
        {
            var query = _context.Classes
                .Where(c => c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.ToLower();
                query = query.Where(c =>
                    (!string.IsNullOrEmpty(c.ClassName) && c.ClassName.ToLower().Contains(lowerSearch)) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(lowerSearch)) ||
                    c.StudentCount.ToString().Contains(lowerSearch));
            }

            TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)PageSize);

            FilteredClasses = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new ClassInformationModel
                {
                    Id = c.Id,
                    ClassName = c.ClassName,
                    StudentCount = c.StudentCount,
                    Description = c.Description
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            if (!TryValidateModel(NewClass))
                return Page();

            var entity = new Class
            {
                ClassName = NewClass.ClassName,
                StudentCount = NewClass.StudentCount,
                Description = NewClass.Description,
                IsActive = true
            };

            _context.Classes.Add(entity);
            await _context.SaveChangesAsync();

            NewClass = new(); 
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var classToEdit = await _context.Classes.FindAsync(id);
            if (classToEdit != null && classToEdit.IsActive)
            {
                NewClass = new ClassInformationModel
                {
                    Id = classToEdit.Id,
                    ClassName = classToEdit.ClassName,
                    StudentCount = classToEdit.StudentCount,
                    Description = classToEdit.Description
                };
            }

            await ApplyFilteringAndPagingAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            if (!TryValidateModel(NewClass))
                return Page();

            var existingClass = await _context.Classes.FindAsync(NewClass.Id);
            if (existingClass != null && existingClass.IsActive)
            {
                existingClass.ClassName = NewClass.ClassName;
                existingClass.StudentCount = NewClass.StudentCount;
                existingClass.Description = NewClass.Description;

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var classToDelete = await _context.Classes.FindAsync(id);
            if (classToDelete != null)
            {
                classToDelete.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportJsonAsync(bool isFiltered)
        {
            if (!IsAuthenticated()) return RedirectToPage("/Login");

            var columnIndexes = new List<int>();
            if (!string.IsNullOrWhiteSpace(selectedColumns))
            {
                columnIndexes = selectedColumns
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => int.TryParse(x, out _))
                    .Select(int.Parse)
                    .ToList();
            }

            var query = _context.Classes.Where(c => c.IsActive).AsQueryable();

            if (isFiltered && !string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.ToLower();
                query = query.Where(c =>
                    (!string.IsNullOrEmpty(c.ClassName) && c.ClassName.ToLower().Contains(lowerSearch)) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.ToLower().Contains(lowerSearch)) ||
                    c.StudentCount.ToString().Contains(lowerSearch));
            }

            var dataToExport = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new ClassInformationModel
                {
                    Id = c.Id,
                    ClassName = c.ClassName,
                    StudentCount = c.StudentCount,
                    Description = c.Description
                }).ToListAsync();

            var reducedData = dataToExport
                .Select(item =>
                {
                    var dict = new Dictionary<string, object>();
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(0))
                        dict["ClassName"] = item.ClassName;
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(1))
                        dict["StudentCount"] = item.StudentCount;
                    if (columnIndexes.Count == 0 || columnIndexes.Contains(2))
                        dict["Description"] = item.Description;
                    return dict;
                }).ToList();

            var json = JsonExportUtils.Instance.SerializeToJson(reducedData);
            var fileName = isFiltered ? "filtered_classes.json" : "all_classes.json";
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(fileBytes, "application/json", fileName);
        }

        public IActionResult OnPostLogout()
        {
            Response.Cookies.Delete("username");
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("session_id");

            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }

        private bool IsAuthenticated()
        {
            var sessionUsername = HttpContext.Session.GetString("username");
            var sessionToken = HttpContext.Session.GetString("token");
            var sessionId = HttpContext.Session.GetString("session_id");

            var cookieUsername = Request.Cookies["username"];
            var cookieToken = Request.Cookies["token"];
            var cookieSessionId = Request.Cookies["session_id"];

            return sessionUsername == cookieUsername &&
                   sessionToken == cookieToken &&
                   sessionId == cookieSessionId;
        }
    }
}

