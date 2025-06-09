using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCtemplate.DataAccess.Data;
using MVCTemplate.DataAccess.Repository.IRepository;
using MVCTemplate.Models;
using MVCTemplate.Util;
using OfficeOpenXml;
using System.Diagnostics;
using System.Text;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using System.Data;
using System.IO;
using ClosedXML.Excel;
//using System.IO.Packaging;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}")]
    [Area("Admin")]

    public class PackageController : Controller
    {
        db dbop = new db();
        private readonly ApplicationDbContext _context;

        public IActionResult ShowPackagesData()
        {
            return View();
        } // for charts

        [HttpPost]
        public List<object> GetPackagesData()
        {
            List<object> data = new List<object>();

            // Group by priority and count how many packages have each priority
            var priorityCounts = _context.Packages
                .GroupBy(p => p.Priority)
                .Select(g => new {
                    Priority = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Priority) // Optional: sort by priority
                .ToList();

            // X-axis labels: Priority values (e.g., 1, 2, 3)
            List<string> labels = priorityCounts.Select(p => p.Priority.ToString()).ToList();
            data.Add(labels);

            // Y-axis values: number of records with that priority
            List<int> counts = priorityCounts.Select(p => p.Count).ToList();
            data.Add(counts);

            return data;
        }

        [HttpPost]
        [Route("/Admin/Package/GetPackagesCreatedPerDay")]
        public IActionResult GetPackagesCreatedPerDay()
        {
            var grouped = _context.Packages
                .AsEnumerable()
                .GroupBy(p => p.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key.ToString("MM/dd/yyyy"),
                    Count = g.Count()
                })
                .ToList();

            List<string> labels = grouped.Select(g => g.Date).ToList();
            List<int> counts = grouped.Select(g => g.Count).ToList();

            return Json(new List<object> { labels, counts });
        }

        [HttpPost]
        public IActionResult ExportFilteredToExcel([FromBody] List<Package> filteredPackages)
        {
            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Filtered Packages");

            // Row 1: Title
            worksheet.Cells[1, 1].Value = "FIltered Package Data";
            worksheet.Cells[1, 1, 1, 5].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Row 2: Generated At
            var now = DateTime.Now;
            string generatedAt = $"Generated at: {now:MMMM-dd-yyyy hh:mm tt}";
            worksheet.Cells[2, 1].Value = generatedAt;
            worksheet.Cells[2, 1, 2, 5].Merge = true;
            worksheet.Cells[2, 1].Style.Font.Italic = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Row 3: Header
            worksheet.Cells[3, 1].Value = "Name";
            worksheet.Cells[3, 2].Value = "Description";
            worksheet.Cells[3, 3].Value = "Priority";
            worksheet.Cells[3, 4].Value = "Created At";
            worksheet.Cells[3, 5].Value = "Updated At";

            // Row 4+: Data
            for (int i = 0; i < filteredPackages.Count; i++)
            {
                var p = filteredPackages[i];
                int row = i + 4;

                worksheet.Cells[row, 1].Value = p.Name;
                worksheet.Cells[row, 2].Value = p.Description;
                worksheet.Cells[row, 3].Value = p.Priority;
                worksheet.Cells[row, 4].Value = p.CreatedAt.ToString("MMMM-dd-yyyy"); // Updated format
                worksheet.Cells[row, 5].Value = p.UpdatedAt.ToString("MMMM-dd-yyyy"); // Updated format
            }

            // Apply AutoFilter to header row
            worksheet.Cells[3, 1, 3, 5].AutoFilter = true;

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Return Excel file
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "FilteredPackages.xlsx");
        }



        public IActionResult PDF() 
        {
            var document = new Document
            {
                PageInfo = new PageInfo { Margin=new MarginInfo(28,28,28,40)}
            };
            var pdfpage = document.Pages.Add();
            Table table = new Table
            {
                ColumnWidths = "17% 17% 17% 17% 17% 17%",
                DefaultCellPadding = new MarginInfo(10, 5, 10, 5),
                Border = new BorderInfo(BorderSide.All, .5f, Color.Black),
                DefaultCellBorder = new BorderInfo(BorderSide.All, .2f, Color.Black),
            };

            DataTable dt = dbop.Getrecord();
            table.ImportDataTable(dt, true, 0, 0);
            document.Pages[1].Paragraphs.Add(table);

            using (var streamOut = new MemoryStream())
            {
                document.Save(streamOut);
                return File(streamOut.ToArray(), "application/pdf", "Packages.pdf");
            }
        } //used db.cs and query from server explorer

        public IActionResult Index()
        {
            return View();
        }

        private IUnitOfWork _unitOfWork;

        public PackageController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context; //avoid double constructors
        }

        public IActionResult Import()
        {
            return View();
        } // for the import modal

        [HttpPost]
        public IActionResult Import(IFormFile file) // this code assumes there is no ID column in the excel file
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("My Name");

                using var package = new ExcelPackage(file.OpenReadStream());
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return BadRequest(new { message = "Invalid Excel file." });

                var packagesToAdd = new List<Package>();
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // assuming row 1 is header
                {
                    string? name = worksheet.Cells[row, 1].Text.Trim();
                    string? description = worksheet.Cells[row, 2].Text.Trim();
                    string? priorityText = worksheet.Cells[row, 3].Text.Trim();

                    if (string.IsNullOrEmpty(name))
                        continue; // skip empty rows

                    if (!int.TryParse(priorityText, out int priority))
                        continue; // skip rows with invalid priority
                    
                    var existing = _unitOfWork.Package.CheckIfUnique(name);
                    if (existing != null)
                        continue; // skip duplicates

                    var newPackage = new Package
                    {
                        Name = name,
                        Description = description,
                        Priority = priority
                    };

                    packagesToAdd.Add(newPackage);
                }

                if (packagesToAdd.Count == 0)
                    return BadRequest(new { message = "No valid or unique packages found." });

                foreach (var pkg in packagesToAdd)
                {
                    _unitOfWork.Package.Add(pkg);
                }

                _unitOfWork.Save();

                return Ok(new { message = $"{packagesToAdd.Count} packages imported successfully." });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Error occurred while saving to database." });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { message = "Invalid Operation." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        } 


        public IActionResult Create(Package package)
        {
            try
            {
                Models.Package? packageCheck = _unitOfWork.Package.CheckIfUnique(package.Name);
                if (packageCheck != null)
                {
                    ModelState.AddModelError("Name", "Package already exists");
                }

                if (ModelState.IsValid)
                {
                    _unitOfWork.Package.Add(package);
                    _unitOfWork.Save();
                    return Ok(new { message = "Added Successfully" });
                }
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
        public IActionResult Update(Package obj)
        {
            try
            {
                obj.GenerateUpdatedAt();
                Package? package = _unitOfWork.Package.ContinueIfNoChangeOnUpdate(obj.Name, obj.Id);

                if (package != null)
                {
                    ModelState.AddModelError("Name", "Package Name Already exists");
                }
                if (ModelState.IsValid)
                {
                    _unitOfWork.Package.Update(obj);
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
                    return BadRequest(new { message = "Package Id not found" });
                }

                Package package = _unitOfWork.Package.Get(u => u.Id == id);
                if (package == null)
                {
                    return BadRequest(new { message = "Package Id not found" });
                }

                _unitOfWork.Package.Remove(package);
                _unitOfWork.Save();
                return Ok(new { message = "Package deleted successfully" });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Unable to delete data because it is being used in another table" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            var packages = _unitOfWork.Package.GetAll().ToList();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Packages");

            // Row 1: Title
            worksheet.Cells[1, 1].Value = "Package Data";
            worksheet.Cells[1, 1, 1, 5].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Row 2: Generated At
            var now = DateTime.Now;
            worksheet.Cells[2, 1].Value = $"Generated at: {now:MMMM-dd-yyyy hh:mm tt}";
            worksheet.Cells[2, 1, 2, 5].Merge = true;
            worksheet.Cells[2, 1].Style.Font.Italic = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Row 3: Headers
            worksheet.Cells[3, 1].Value = "Name";
            worksheet.Cells[3, 2].Value = "Description";
            worksheet.Cells[3, 3].Value = "Priority";
            worksheet.Cells[3, 4].Value = "Created At";
            worksheet.Cells[3, 5].Value = "Updated At";

            // Rows 4+: Data
            for (int i = 0; i < packages.Count; i++)
            {
                var row = i + 4;
                var p = packages[i];

                worksheet.Cells[row, 1].Value = p.Name;
                worksheet.Cells[row, 2].Value = p.Description;
                worksheet.Cells[row, 3].Value = p.Priority;
                worksheet.Cells[row, 4].Value = p.CreatedAt.ToString("MMMM-dd-yyyy");
                worksheet.Cells[row, 5].Value = p.UpdatedAt.ToString("MMMM-dd-yyyy");
            }

            // Apply AutoFilter to header row (Row 3)
            worksheet.Cells[3, 1, 3, 5].AutoFilter = true;

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Return Excel file
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Packages.xlsx");
        }



        #region API Calls
        [HttpGet]

        public IActionResult GetAllPackages()
        {
            List<Package>? packageList = _unitOfWork.Package.GetAll().ToList();
            return Json(new { data = packageList });
        }
       
        #endregion

    }
}
