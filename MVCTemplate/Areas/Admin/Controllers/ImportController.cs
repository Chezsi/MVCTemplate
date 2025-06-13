using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCtemplate.DataAccess.Data;
using MVCTemplate.DataAccess.Repository.IRepository;
using MVCTemplate.Models;
using MVCTemplate.Util;
using System.Diagnostics;
using System.Text;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}")]
    [Area("Admin")]

    public class ImportController : Controller
    {


        private readonly ILogger<ImportController> _logger;
        private readonly ApplicationDbContext _context;

        public ImportController(ILogger<ImportController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult UploadExcel()
        {
            return View(); 
        }

        public IActionResult UploadExcelProduct()
        {
            //return View();
            return View("UploadExcel");
        } // for product

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        bool dataInserted = false;
                        do
                        {
                            bool isHeaderSkipped = false;
                            while (reader.Read())
                            {
                                if (!isHeaderSkipped)
                                {
                                    isHeaderSkipped = true;
                                    continue;
                                }

                                var name = reader.GetValue(1)?.ToString();
                                var description = reader.GetValue(2)?.ToString();
                                var priorityValue = reader.GetValue(3)?.ToString();

                                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(priorityValue))
                                {
                                    TempData["Warning"] = "Some rows had missing data and were skipped.";
                                    continue;
                                }

                                Package p = new Package
                                {
                                    Name = name,
                                    Description = description,
                                    Priority = Convert.ToInt32(priorityValue)
                                };

                                _context.Add(p);
                                await _context.SaveChangesAsync();
                                dataInserted = true;
                            }
                        } while (reader.NextResult());

                        if (dataInserted)
                            TempData["Success"] = "Excel data uploaded successfully.";
                        else
                            TempData["Warning"] = "No valid data found in the file.";
                    }
                }
            }
            else
            {
                TempData["Warning"] = "Please upload a valid Excel file.";
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> UploadExcelProduct(IFormFile file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        bool dataInserted = false;
                        bool invalidRowFound = false;

                        do
                        {
                            bool isHeaderSkipped = false;

                            while (reader.Read())
                            {
                                if (!isHeaderSkipped)
                                {
                                    isHeaderSkipped = true;
                                    continue;
                                }

                                var name = reader.GetValue(1)?.ToString();
                                var description = reader.GetValue(2)?.ToString();
                                var quantityStr = reader.GetValue(3)?.ToString();

                                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(quantityStr) || !int.TryParse(quantityStr, out int quantity))
                                {
                                    invalidRowFound = true;
                                    continue;
                                }

                                Product pr = new Product
                                {
                                    Name = name,
                                    Description = description,
                                    Quantity = quantity
                                };

                                _context.Add(pr);
                                await _context.SaveChangesAsync();
                                dataInserted = true;
                            }
                        } while (reader.NextResult());

                        if (dataInserted)
                        {
                            TempData["Success"] = "Product Excel data uploaded successfully.";
                        }

                        if (invalidRowFound)
                        {
                            TempData["Warning"] = "Some rows had missing or invalid data and were skipped.";
                        }

                        if (!dataInserted && !invalidRowFound)
                        {
                            TempData["Warning"] = "No valid data found in the uploaded file.";
                        }
                    }
                }
            }
            else
            {
                TempData["Warning"] = "Please upload a valid Excel file.";
            }

            return View("UploadExcel");
        }

    }
}
