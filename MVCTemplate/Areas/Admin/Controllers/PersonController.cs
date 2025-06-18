using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MVCtemplate.DataAccess.Data;
using MVCTemplate.DataAccess.Repository.IRepository;
using MVCTemplate.Models;
using MVCTemplate.Util;
using MVCTemplate.ViewModels;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Diagnostics;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}")]
    [Area("Admin")]
    public class PersonController : Controller
    {
        public IActionResult Index()
        {
            PersonVM personVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.NameCategory,
                    Value = u.IdCategory.ToString()
                }),
                Person = new Person()
            };
            return View(personVM);
        }

        private IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public PersonController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Update()
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category
               .GetAll().Select(u => new SelectListItem
               {
                   Text = u.NameCategory,
                   Value = u.IdCategory.ToString()
               });

            //ViewBag.CategoryList = CategoryList;
            //not working
            return View();
        }

        [HttpPost]
        public IActionResult GetPersonsData()
        {
            // Fetch categories where there is at least one person linked by CategoryId
            var data = _context.Categorys
                .Where(c => _context.Persons.Any(p => p.CategoryId == c.IdCategory))
                .Select(c => new
                {
                    CategoryName = c.NameCategory, // Changed here
                    EmployeeCount = _context.Persons.Count(p => p.CategoryId == c.IdCategory) // Changed here
                })
                .ToList();

            var labels = data.Select(d => d.CategoryName).ToList();
            var counts = data.Select(d => d.EmployeeCount).ToList();

            return Json(new object[] { labels, counts });
        }



        [HttpPost]
        public IActionResult Create(Person person)
        {
            try
            {
                // Check required fields
                if (string.IsNullOrWhiteSpace(person.Name))
                {
                    return BadRequest(new { field = "Name", message = "Name is required." });
                }

                if (string.IsNullOrWhiteSpace(person.Position))
                {
                    return BadRequest(new { field = "Position", message = "Position is required." });
                }

                if (person.CategoryId == null)
                {
                    return BadRequest(new { field = "CategoryId", message = "CategoryId is required." });
                }

                // Check for duplicate Name
                Models.Person? personCheck = _unitOfWork.Person.CheckIfUnique(person.Name);
                if (personCheck != null)
                {
                    return BadRequest(new { field = "Name", message = "Person already exists." });
                }

                // If model state is valid, save
                if (ModelState.IsValid)
                {
                    _unitOfWork.Person.Add(person);
                    _unitOfWork.Save();
                    return Ok(new { message = "Added Successfully" });
                }

                // Handle any other validation errors
                var errors = ModelState.ToDictionary(
                     kvp => kvp.Key,
                     kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage)?.ToArray() ?? []
                  );
                return BadRequest(new { errors, message = "Something went wrong" });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Error occurred while saving to database" });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { message = "Invalid Operation" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "An unexpected error occurred" });
            }
        }

        [HttpPut]
        public IActionResult Update(Person obj)
        {
            try
            {
                obj.GenerateUpdatedAt();
                Person? person = _unitOfWork.Person.ContinueIfNoChangeOnUpdate(obj.Name, obj.Id);

                if (person != null)
                {
                    return BadRequest(new { field = "Name", message = "Person Name already exists" });

                }
                if (ModelState.IsValid)
                {
                    _unitOfWork.Person.Update(obj);
                    _unitOfWork.Save();
                    return Ok(new { message = "Updated Successfully" });
                }
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage)?.ToArray() ?? []);

                return BadRequest(new { errors, message = "Something went wrong!" });

            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Error occurred while saving to database" });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { message = "Invalid operation" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "An unexpected error occurred" });
            }
        }

        [HttpDelete]

        public IActionResult Delete(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest(new { message = "Person Id not found" });
                }

                Person person = _unitOfWork.Person.Get(u => u.Id == id);
                if (person == null)
                {
                    return BadRequest(new { message = "Person Id not found" });
                }

                _unitOfWork.Person.Remove(person);
                //_unitOfWork.Save();
                return Ok(new { message = "Person deleted successfully" });
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547) // DELETE statement conflicted with the reference constraint \"FK-Contracts_Persons_PersonId\"
            {
                return BadRequest(new { message = "Unable to delete data because it is being used in another table" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet] // probably did not had to use contracts
        public async Task<IActionResult> ExportToExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            var persons = await _context.Persons.ToListAsync();
            var allContracts = await _context.Contracts.ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Person");

            // Colors
            var blueBackground = System.Drawing.Color.FromArgb(0, 51, 102);
            var whiteFont = System.Drawing.Color.White;
            var lightGreen = System.Drawing.Color.FromArgb(198, 239, 206);
            var darkGreen = System.Drawing.Color.FromArgb(155, 187, 89);

            var borderStyle = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            // Row 1 - Title
            worksheet.Cells["A1:D1"].Merge = true;
            worksheet.Cells["A1"].Value = "Person Data";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Row 2 - Timestamp
            worksheet.Cells["A2:D2"].Merge = true;
            worksheet.Cells["A2"].Value = $"Generated at: {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Style for Title and Timestamp rows
            worksheet.Cells["A1:D2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1:D2"].Style.Fill.BackgroundColor.SetColor(blueBackground);
            worksheet.Cells["A1:D2"].Style.Font.Color.SetColor(whiteFont);
            worksheet.Cells["A1:D3"].Style.Border.Top.Style = borderStyle;
            worksheet.Cells["A1:D3"].Style.Border.Bottom.Style = borderStyle;
            worksheet.Cells["A1:D3"].Style.Border.Left.Style = borderStyle;
            worksheet.Cells["A1:D3"].Style.Border.Right.Style = borderStyle;

            // Row 3 - Headers
            string[] headers = new[] { "Person ID", "Name", "Position", "Category ID" };

            for (int i = 1; i <= headers.Length; i++)
            {
                var cell = worksheet.Cells[3, i];
                cell.Value = headers[i - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor((i % 2 == 0) ? lightGreen : darkGreen);
                cell.Style.Border.Top.Style = borderStyle;
                cell.Style.Border.Bottom.Style = borderStyle;
                cell.Style.Border.Left.Style = borderStyle;
                cell.Style.Border.Right.Style = borderStyle;
            }

            int row = 4;

            foreach (var person in persons)
            {
                var contractsForPerson = allContracts.Where(c => c.PersonId == person.Id);

                // Export one row per contract/person combo but only person data (up to Category ID)
                foreach (var contract in contractsForPerson)
                {
                    worksheet.Cells[row, 1].Value = person.Id;
                    worksheet.Cells[row, 2].Value = person.Name;
                    worksheet.Cells[row, 3].Value = person.Position;
                    worksheet.Cells[row, 4].Value = person.CategoryId;

                    // Style each cell in this row (only 4 columns)
                    for (int col = 1; col <= 4; col++)
                    {
                        var cell = worksheet.Cells[row, col];
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor((col % 2 == 0) ? lightGreen : darkGreen);
                        cell.Style.Border.Top.Style = borderStyle;
                        cell.Style.Border.Bottom.Style = borderStyle;
                        cell.Style.Border.Left.Style = borderStyle;
                        cell.Style.Border.Right.Style = borderStyle;
                    }
                    row++;
                }
            }

            // Apply autofilter to the header row (only 4 columns)
            worksheet.Cells[3, 1, row - 1, 4].AutoFilter = true;

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Return Excel file
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Person.xlsx");
        }



        [HttpGet]
        public async Task<IActionResult> ExportCurrentContractsExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            var persons = await _context.Persons.ToListAsync();
            var currentContracts = await _context.Contracts
                .Where(c => c.Validity >= DateTime.Today)
                .ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("CurrentContracts");

            // Colors
            var blueBackground = System.Drawing.Color.FromArgb(0, 51, 102);
            var whiteFont = System.Drawing.Color.White;
            var lightGreen = System.Drawing.Color.FromArgb(198, 239, 206);
            var darkGreen = System.Drawing.Color.FromArgb(155, 187, 89);

            var borderStyle = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            // Row 1 - Title
            worksheet.Cells["A1:H1"].Merge = true;
            worksheet.Cells["A1"].Value = "Current Contracts Export";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            // Row 2 - Timestamp
            worksheet.Cells["A2:H2"].Merge = true;
            worksheet.Cells["A2"].Value = $"Generated at: {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
            worksheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            // Style for Title and Timestamp rows
            worksheet.Cells["A1:H2"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells["A1:H2"].Style.Fill.BackgroundColor.SetColor(blueBackground);
            worksheet.Cells["A1:H2"].Style.Font.Color.SetColor(whiteFont);
            worksheet.Cells["A1:H3"].Style.Border.Top.Style = borderStyle;
            worksheet.Cells["A1:H3"].Style.Border.Bottom.Style = borderStyle;
            worksheet.Cells["A1:H3"].Style.Border.Left.Style = borderStyle;
            worksheet.Cells["A1:H3"].Style.Border.Right.Style = borderStyle;

            // Row 3 - Headers
            string[] headers = new[] {
        "Person ID", "Name", "Position", "Category ID",
        "Contract ID", "Contract Name", "Description", "Validity"
    };

            for (int i = 1; i <= headers.Length; i++)
            {
                var cell = worksheet.Cells[3, i];
                cell.Value = headers[i - 1];
                cell.Style.Font.Bold = true;

                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor((i % 2 == 0) ? lightGreen : darkGreen);

                // Apply border to each header cell
                cell.Style.Border.Top.Style = borderStyle;
                cell.Style.Border.Bottom.Style = borderStyle;
                cell.Style.Border.Left.Style = borderStyle;
                cell.Style.Border.Right.Style = borderStyle;
            }

            int row = 4;

            foreach (var person in persons)
            {
                var contractsForPerson = currentContracts.Where(c => c.PersonId == person.Id);
                foreach (var contract in contractsForPerson)
                {
                    worksheet.Cells[row, 1].Value = person.Id;
                    worksheet.Cells[row, 2].Value = person.Name;
                    worksheet.Cells[row, 3].Value = person.Position;
                    worksheet.Cells[row, 4].Value = person.CategoryId;
                    worksheet.Cells[row, 5].Value = contract.Id;
                    worksheet.Cells[row, 6].Value = contract.Name;
                    worksheet.Cells[row, 7].Value = contract.Description;
                    worksheet.Cells[row, 8].Value = contract.Validity?.ToString("MMMM dd, yyyy") ?? "N/A";

                    for (int col = 1; col <= 8; col++)
                    {
                        var cell = worksheet.Cells[row, col];
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor((col % 2 == 0) ? lightGreen : darkGreen);

                        // Apply border to each cell
                        cell.Style.Border.Top.Style = borderStyle;
                        cell.Style.Border.Bottom.Style = borderStyle;
                        cell.Style.Border.Left.Style = borderStyle;
                        cell.Style.Border.Right.Style = borderStyle;
                    }

                    row++;
                }
            }

            // Apply autofilter to header row
            worksheet.Cells[3, 1, row - 1, 8].AutoFilter = true;

            // Auto fit columns
            worksheet.Cells.AutoFitColumns();

            // Save to stream
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CurrentContracts.xlsx");
        }





        #region API Calls
        [HttpGet]

        public IActionResult GetAllPersons()
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
            {
                return Unauthorized(); // to prevent the raw json from being seen 
            }

            List<Person>? personList = _unitOfWork.Person.GetAll().ToList();
            return Json(new { data = personList });
        }

        [HttpGet] // for exporting data from two tables (JS)
        public IActionResult ExportPersonsWithCurrentContracts()
        {

            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
            {
                return Unauthorized(); // to prevent the raw json from being seen 
            }

            var now = DateTime.Now;

            var data = from person in _context.Persons
                       join contract in _context.Contracts on person.Id equals contract.PersonId
                       where contract.Validity > now
                       select new
                       {
                           Person_Id = person.Id,
                           Person_Name = person.Name,
                           Person_Position = person.Position,
                           Person_CategoryId = person.CategoryId,

                           Contract_Id = contract.Id,
                           Contract_Name = contract.Name,
                           Contract_Description = contract.Description,
                           Contract_Validity = contract.Validity
                       };

            var list = data.ToList();

            return Json(list);
        }
        #endregion

    }
}
