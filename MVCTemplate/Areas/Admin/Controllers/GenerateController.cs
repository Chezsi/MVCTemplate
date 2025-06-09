using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCtemplate.DataAccess.Data;
using MVCTemplate.Models;
using MVCTemplate.Util;
using MVCTemplate.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing;
using System.Drawing;
using DocumentFormat.OpenXml.InkML;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using QuestPDF.Previewer; // for export pdf
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.OpenXml4Net.OPC;
using NPOI.XSSF.UserModel.Helpers;
using System.Drawing.Imaging; // for import excel

namespace MVCTemplate.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}, {Roles.User}")]
    [Area("Admin")]
    public class GenerateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GenerateController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        //ExcelPackage.License.SetNonCommercialPersonal("My Name");
        [HttpGet]
        public IActionResult ExportToExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            using var package = new ExcelPackage();

            void ApplyBorders(ExcelWorksheet sheet, int fromRow, int fromCol, int toRow, int toCol)
            {
                using var range = sheet.Cells[fromRow, fromCol, toRow, toCol];
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            void LockSheet(ExcelWorksheet sheet)
            {
                sheet.Cells.Style.Locked = true;
                sheet.Protection.IsProtected = true;
                sheet.Protection.SetPassword("YourSheetPassword");
            }

            string FormatDateTime(DateTime dt) => dt.ToString("MMMM-dd-yyyy HH:mm:ss"); // with time
            string FormatDate(DateTime dt) => dt.ToString("MMMM-dd-yyyy"); // without time

            // CATEGORY SHEET
            var categorySheet = package.Workbook.Worksheets.Add("Categories");
            var categories = _context.Categorys.ToList();

            categorySheet.Cells[1, 1].Value = "Categories Data";
            categorySheet.Cells[1, 1, 1, 5].Merge = true;
            categorySheet.Cells[1, 1].Style.Font.Bold = true;
            categorySheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            categorySheet.Cells[2, 1].Value = "Id";
            categorySheet.Cells[2, 2].Value = "Name";
            categorySheet.Cells[2, 3].Value = "Code";
            categorySheet.Cells[2, 4].Value = "CreatedAt";
            categorySheet.Cells[2, 5].Value = "UpdatedAt";

            for (int i = 0; i < categories.Count; i++)
            {
                var c = categories[i];
                int row = i + 3;
                categorySheet.Cells[row, 1].Value = c.IdCategory;
                categorySheet.Cells[row, 2].Value = c.NameCategory;
                categorySheet.Cells[row, 3].Value = c.CodeCategory;
                categorySheet.Cells[row, 4].Value = FormatDateTime(c.CreatedAt);
                categorySheet.Cells[row, 5].Value = FormatDateTime(c.UpdatedAt);
            }

            ApplyBorders(categorySheet, 1, 1, categories.Count + 2, 5);
            categorySheet.Cells[categorySheet.Dimension.Address].AutoFitColumns();
            LockSheet(categorySheet);

            // CONTRACT SHEET
            var contractSheet = package.Workbook.Worksheets.Add("Contracts");
            var contracts = _context.Contracts.ToList();

            contractSheet.Cells[1, 1].Value = "Contracts Data";
            contractSheet.Cells[1, 1, 1, 7].Merge = true;
            contractSheet.Cells[1, 1].Style.Font.Bold = true;
            contractSheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            contractSheet.Cells[2, 1].Value = "Id";
            contractSheet.Cells[2, 2].Value = "Name";
            contractSheet.Cells[2, 3].Value = "Description";
            contractSheet.Cells[2, 4].Value = "Validity";
            contractSheet.Cells[2, 5].Value = "PersonId";
            contractSheet.Cells[2, 6].Value = "CreatedAt";
            contractSheet.Cells[2, 7].Value = "UpdatedAt";

            for (int i = 0; i < contracts.Count; i++)
            {
                var c = contracts[i];
                int row = i + 3;
                contractSheet.Cells[row, 1].Value = c.Id;
                contractSheet.Cells[row, 2].Value = c.Name;
                contractSheet.Cells[row, 3].Value = c.Description;
                contractSheet.Cells[row, 4].Value = c.Validity?.ToString("MMMM-dd-yyyy"); // no time for Validity
                contractSheet.Cells[row, 5].Value = c.PersonId;
                contractSheet.Cells[row, 6].Value = FormatDateTime(c.CreatedAt);
                contractSheet.Cells[row, 7].Value = FormatDateTime(c.UpdatedAt);
            }

            ApplyBorders(contractSheet, 1, 1, contracts.Count + 2, 7);
            contractSheet.Cells[contractSheet.Dimension.Address].AutoFitColumns();
            LockSheet(contractSheet);

            // PACKAGE SHEET
            var packageSheet = package.Workbook.Worksheets.Add("Packages");
            var packages = _context.Packages.ToList();

            packageSheet.Cells[1, 1].Value = "Packages Data";
            packageSheet.Cells[1, 1, 1, 6].Merge = true;
            packageSheet.Cells[1, 1].Style.Font.Bold = true;
            packageSheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            packageSheet.Cells[2, 1].Value = "Id";
            packageSheet.Cells[2, 2].Value = "Name";
            packageSheet.Cells[2, 3].Value = "Description";
            packageSheet.Cells[2, 4].Value = "Priority";
            packageSheet.Cells[2, 5].Value = "CreatedAt";
            packageSheet.Cells[2, 6].Value = "UpdatedAt";

            for (int i = 0; i < packages.Count; i++)
            {
                var p = packages[i];
                int row = i + 3;
                packageSheet.Cells[row, 1].Value = p.Id;
                packageSheet.Cells[row, 2].Value = p.Name;
                packageSheet.Cells[row, 3].Value = p.Description;
                packageSheet.Cells[row, 4].Value = p.Priority;
                packageSheet.Cells[row, 5].Value = FormatDateTime(p.CreatedAt);
                packageSheet.Cells[row, 6].Value = FormatDateTime(p.UpdatedAt);
            }

            ApplyBorders(packageSheet, 1, 1, packages.Count + 2, 6);
            packageSheet.Cells[packageSheet.Dimension.Address].AutoFitColumns();
            LockSheet(packageSheet);

            // PERSON SHEET
            var personSheet = package.Workbook.Worksheets.Add("Persons");
            var persons = _context.Persons.ToList();

            personSheet.Cells[1, 1].Value = "Persons Data";
            personSheet.Cells[1, 1, 1, 6].Merge = true;
            personSheet.Cells[1, 1].Style.Font.Bold = true;
            personSheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            personSheet.Cells[2, 1].Value = "Id";
            personSheet.Cells[2, 2].Value = "Name";
            personSheet.Cells[2, 3].Value = "Position";
            personSheet.Cells[2, 4].Value = "CategoryId";
            personSheet.Cells[2, 5].Value = "CreatedAt";
            personSheet.Cells[2, 6].Value = "UpdatedAt";

            for (int i = 0; i < persons.Count; i++)
            {
                var p = persons[i];
                int row = i + 3;
                personSheet.Cells[row, 1].Value = p.Id;
                personSheet.Cells[row, 2].Value = p.Name;
                personSheet.Cells[row, 3].Value = p.Position;
                personSheet.Cells[row, 4].Value = p.CategoryId;
                personSheet.Cells[row, 5].Value = FormatDateTime(p.CreatedAt);
                personSheet.Cells[row, 6].Value = FormatDateTime(p.UpdatedAt);
            }

            ApplyBorders(personSheet, 1, 1, persons.Count + 2, 6);
            personSheet.Cells[personSheet.Dimension.Address].AutoFitColumns();
            LockSheet(personSheet);

            // REPORTS SHEET
            var reportsSheet = package.Workbook.Worksheets.Add("Reports");
            var reports = _context.Reports.ToList();
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reports");

            reportsSheet.Cells[1, 1].Value = "Reports Data";
            reportsSheet.Cells[1, 1, 1, 5].Merge = true;
            reportsSheet.Cells[1, 1].Style.Font.Bold = true;
            reportsSheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            reportsSheet.Cells[2, 1].Value = "Title";
            reportsSheet.Cells[2, 2].Value = "Description";
            reportsSheet.Cells[2, 3].Value = "CreatedAt";
            reportsSheet.Cells[2, 4].Value = "UpdatedAt";
            reportsSheet.Cells[2, 5].Value = "Image";

            int reportRow = 3;
            foreach (var report in reports)
            {
                reportsSheet.Cells[reportRow, 1].Value = report.Title;
                reportsSheet.Cells[reportRow, 2].Value = report.Description;
                reportsSheet.Cells[reportRow, 3].Value = FormatDateTime(report.CreatedAt);
                reportsSheet.Cells[reportRow, 4].Value = FormatDateTime(report.UpdatedAt);

                if (!string.IsNullOrEmpty(report.ImageName))
                {
                    var imagePath = Path.Combine(filePath, report.ImageName);
                    if (System.IO.File.Exists(imagePath))
                    {
                        using var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                        using var image = System.Drawing.Image.FromStream(imageStream);

                        int targetWidth = 80, targetHeight = 80;
                        double ratioX = (double)targetWidth / image.Width;
                        double ratioY = (double)targetHeight / image.Height;
                        double ratio = Math.Min(ratioX, ratioY);

                        int finalWidth = (int)(image.Width * ratio);
                        int finalHeight = (int)(image.Height * ratio);
                        reportsSheet.Column(5).Width = targetWidth / 7.5;
                        reportsSheet.Row(reportRow).Height = finalHeight * 0.75;

                        imageStream.Position = 0;
                        var excelImage = reportsSheet.Drawings.AddPicture($"img_{reportRow}", imageStream);
                        excelImage.SetSize(finalWidth, finalHeight);
                        excelImage.SetPosition(reportRow - 1, 0, 4, 0);

                        // Make image not movable/resizable:
                        excelImage.EditAs = eEditAs.OneCell;
                        excelImage.Locked = true;
                        excelImage.LockAspectRatio = true;
                    }
                }

                reportRow++;
            }

            ApplyBorders(reportsSheet, 1, 1, reportRow - 1, 5);
            reportsSheet.Cells[reportsSheet.Dimension.Address].AutoFitColumns();

            // Protect the sheet and disallow editing/moving objects:
            LockSheet(reportsSheet);
            reportsSheet.Protection.AllowEditObject = false;  // << ADDED HERE


            // Protect entire workbook structure to prevent adding/removing sheets
            package.Workbook.Protection.LockStructure = true;
            package.Workbook.Protection.SetPassword("YourWorkbookPassword");

            var excelBytes = package.GetAsByteArray();
            string fileDate = DateTime.Now.ToString("MMMM-dd-yyyy HH-mm-ss");
            string filename = $"AllData_{fileDate}.xlsx";

            return File(excelBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        filename);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var categories = await _context.Categorys.ToListAsync();
            var contracts = await _context.Contracts.ToListAsync();
            var packages = await _context.Packages.ToListAsync();
            var persons = await _context.Persons.ToListAsync();
            var reports = await _context.Reports.ToListAsync();
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reports");

            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);

                    // Header with page number
                    page.Header()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span("Page ").FontSize(10).FontColor(Colors.Grey.Medium);
                            text.CurrentPageNumber().FontSize(10).FontColor(Colors.Grey.Medium);
                            text.Span(" of ").FontSize(10).FontColor(Colors.Grey.Medium);
                            text.TotalPages().FontSize(10).FontColor(Colors.Grey.Medium);
                        });

                    // Footer with report generation date
                    page.Footer()
                        .AlignRight()
                        .Text($"Report generated on {DateTime.Now:MMMM-dd-yyyy}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);

                    // Main content
                    page.Content().Column(column =>
                    {
                        // Categories table
                        column.Item().Element(container =>
                            container.PaddingBottom(10).AlignCenter().Text("Categories Data").FontSize(16).Bold().Underline());
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(3);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("Id").Bold();
                                header.Cell().Text("Name").Bold();
                                header.Cell().Text("Code").Bold();
                                header.Cell().Text("CreatedAt").Bold();
                                header.Cell().Text("UpdatedAt").Bold();
                            });
                            foreach (var c in categories)
                            {
                                table.Cell().Text(c.IdCategory.ToString());
                                table.Cell().Text(c.NameCategory);
                                table.Cell().Text(c.CodeCategory);
                                table.Cell().Text(c.CreatedAt.ToString("MMMM-dd-yyyy"));
                                table.Cell().Text(c.UpdatedAt.ToString("MMMM-dd-yyyy"));
                            }
                        });

                        // Contracts table
                        column.Item().Element(container =>
                            container.PaddingBottom(10).AlignCenter().Text("Contracts Data").FontSize(16).Bold().Underline());
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(3);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("Id").Bold();
                                header.Cell().Text("Name").Bold();
                                header.Cell().Text("Description").Bold();
                                header.Cell().Text("Validity").Bold();
                                header.Cell().Text("PersonId").Bold();
                                header.Cell().Text("CreatedAt").Bold();
                                header.Cell().Text("UpdatedAt").Bold();
                            });
                            foreach (var c in contracts)
                            {
                                table.Cell().Text(c.Id.ToString());
                                table.Cell().Text(c.Name);
                                table.Cell().Text(c.Description);
                                table.Cell().Text(c.Validity?.ToString("MMMM-dd-yyyy") ?? "");
                                table.Cell().Text(c.PersonId.ToString());
                                table.Cell().Text(c.CreatedAt.ToString("MMMM-dd-yyyy"));
                                table.Cell().Text(c.UpdatedAt.ToString("MMMM-dd-yyyy"));
                            }
                        });

                        // Packages table
                        column.Item().Element(container =>
                            container.PaddingBottom(10).AlignCenter().Text("Packages Data").FontSize(16).Bold().Underline());
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(3);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("Id").Bold();
                                header.Cell().Text("Name").Bold();
                                header.Cell().Text("Description").Bold();
                                header.Cell().Text("Priority").Bold();
                                header.Cell().Text("CreatedAt").Bold();
                                header.Cell().Text("UpdatedAt").Bold();
                            });
                            foreach (var p in packages)
                            {
                                table.Cell().Text(p.Id.ToString());
                                table.Cell().Text(p.Name);
                                table.Cell().Text(p.Description);
                                table.Cell().Text(p.Priority.ToString());
                                table.Cell().Text(p.CreatedAt.ToString("MMMM-dd-yyyy"));
                                table.Cell().Text(p.UpdatedAt.ToString("MMMM-dd-yyyy"));
                            }
                        });

                        // Persons table
                        column.Item().Element(container =>
                            container.PaddingBottom(10).AlignCenter().Text("Persons Data").FontSize(16).Bold().Underline());
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(3);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("Id").Bold();
                                header.Cell().Text("Name").Bold();
                                header.Cell().Text("Position").Bold();
                                header.Cell().Text("CategoryId").Bold();
                                header.Cell().Text("CreatedAt").Bold();
                                header.Cell().Text("UpdatedAt").Bold();
                            });
                            foreach (var p in persons)
                            {
                                table.Cell().Text(p.Id.ToString());
                                table.Cell().Text(p.Name);
                                table.Cell().Text(p.Position);
                                table.Cell().Text(p.CategoryId.ToString());
                                table.Cell().Text(p.CreatedAt.ToString("MMMM-dd-yyyy"));
                                table.Cell().Text(p.UpdatedAt.ToString("MMMM-dd-yyyy"));
                            }
                        });

                        // Reports table
                        column.Item().Element(container =>
                            container.PaddingBottom(10).AlignCenter().Text("Reports Data").FontSize(16).Bold().Underline());
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(5);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("Title").Bold();
                                header.Cell().Text("Description").Bold();
                                header.Cell().Text("CreatedAt").Bold();
                                header.Cell().Text("UpdatedAt").Bold();
                                header.Cell().Text("Image").Bold();
                            });
                            foreach (var r in reports)
                            {
                                table.Cell().Text(r.Title);
                                table.Cell().Text(r.Description);
                                table.Cell().Text(r.CreatedAt.ToString("MMMM-dd-yyyy"));
                                table.Cell().Text(r.UpdatedAt.ToString("MMMM-dd-yyyy"));
                                table.Cell().Element(cell =>
                                {
                                    if (!string.IsNullOrEmpty(r.ImageName))
                                    {
                                        var imagePath = Path.Combine(filePath, r.ImageName);
                                        if (System.IO.File.Exists(imagePath))
                                        {
                                            cell.Image(imagePath, ImageScaling.FitWidth);
                                        }
                                        else
                                        {
                                            cell.Text("[Image not found]");
                                        }
                                    }
                                    else
                                    {
                                        cell.Text("[No image]");
                                    }
                                });
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "AllData.pdf");
        }





    }
}