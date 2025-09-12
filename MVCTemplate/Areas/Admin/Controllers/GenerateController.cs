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
using ClosedXML.Excel;

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
            QuestPDF.Settings.License = LicenseType.Community;

            var categories = await _context.Categorys.ToListAsync();
            var contracts = await _context.Contracts.ToListAsync();
            var packages = await _context.Packages.ToListAsync();
            var persons = await _context.Persons.ToListAsync();
            var reports = await _context.Reports.ToListAsync();
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reports");

            byte[] pdfBytes = Document.Create(container =>
            {
                void CreatePage(string title, Action<IContainer> content)
                {
                    container.Page(page =>
                    {
                        page.Margin(20);
                        page.Size(PageSizes.A4);

                        // Header
                        page.Header().AlignRight().Text(text =>
                        {
                            text.Span("Page ").FontSize(10).FontColor(Colors.Grey.Medium);
                            text.CurrentPageNumber().FontSize(10).FontColor(Colors.Grey.Medium);
                            text.Span(" of ").FontSize(10).FontColor(Colors.Grey.Medium);
                            text.TotalPages().FontSize(10).FontColor(Colors.Grey.Medium);
                        });

                        // Footer
                        page.Footer().AlignRight()
                            .Text($"Report generated on {DateTime.Now:MMMM-dd-yyyy}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);

                        // Content
                        page.Content().Column(col =>
                        {
                            col.Item().PaddingBottom(10).AlignCenter().Text(title).FontSize(16).Bold().Underline();
                            col.Item().Element(content);
                        });
                    });
                }

                // Categories
                CreatePage("Categories Data", content =>
                {
                    content.Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Id").Bold();
                            header.Cell().Element(CellStyle).Text("Name").Bold();
                            header.Cell().Element(CellStyle).Text("Code").Bold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").Bold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").Bold();
                        });

                        foreach (var c in categories)
                        {
                            table.Cell().Element(CellStyle).Text(c.IdCategory.ToString());
                            table.Cell().Element(CellStyle).Text(c.NameCategory);
                            table.Cell().Element(CellStyle).Text(c.CodeCategory);
                            table.Cell().Element(CellStyle).Text(c.CreatedAt.ToString("MMMM-dd-yyyy"));
                            table.Cell().Element(CellStyle).Text(c.UpdatedAt.ToString("MMMM-dd-yyyy"));
                        }
                    });
                });

                // Contracts
                CreatePage("Contracts Data", content =>
                {
                    content.Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Id").Bold();
                            header.Cell().Element(CellStyle).Text("Name").Bold();
                            header.Cell().Element(CellStyle).Text("Description").Bold();
                            header.Cell().Element(CellStyle).Text("Validity").Bold();
                            header.Cell().Element(CellStyle).Text("PersonId").Bold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").Bold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").Bold();
                        });

                        foreach (var c in contracts)
                        {
                            table.Cell().Element(CellStyle).Text(c.Id.ToString());
                            table.Cell().Element(CellStyle).Text(c.Name);
                            table.Cell().Element(CellStyle).Text(c.Description);
                            table.Cell().Element(CellStyle).Text(c.Validity?.ToString("MMMM-dd-yyyy") ?? "");
                            table.Cell().Element(CellStyle).Text(c.PersonId.ToString());
                            table.Cell().Element(CellStyle).Text(c.CreatedAt.ToString("MMMM-dd-yyyy"));
                            table.Cell().Element(CellStyle).Text(c.UpdatedAt.ToString("MMMM-dd-yyyy"));
                        }
                    });
                });

                // Packages
                CreatePage("Packages Data", content =>
                {
                    content.Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Id").Bold();
                            header.Cell().Element(CellStyle).Text("Name").Bold();
                            header.Cell().Element(CellStyle).Text("Description").Bold();
                            header.Cell().Element(CellStyle).Text("Priority").Bold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").Bold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").Bold();
                        });

                        foreach (var p in packages)
                        {
                            table.Cell().Element(CellStyle).Text(p.Id.ToString());
                            table.Cell().Element(CellStyle).Text(p.Name);
                            table.Cell().Element(CellStyle).Text(p.Description);
                            table.Cell().Element(CellStyle).Text(p.Priority.ToString());
                            table.Cell().Element(CellStyle).Text(p.CreatedAt.ToString("MMMM-dd-yyyy"));
                            table.Cell().Element(CellStyle).Text(p.UpdatedAt.ToString("MMMM-dd-yyyy"));
                        }
                    });
                });

                // Persons
                CreatePage("Persons Data", content =>
                {
                    content.Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Id").Bold();
                            header.Cell().Element(CellStyle).Text("Name").Bold();
                            header.Cell().Element(CellStyle).Text("Position").Bold();
                            header.Cell().Element(CellStyle).Text("CategoryId").Bold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").Bold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").Bold();
                        });

                        foreach (var p in persons)
                        {
                            table.Cell().Element(CellStyle).Text(p.Id.ToString());
                            table.Cell().Element(CellStyle).Text(p.Name);
                            table.Cell().Element(CellStyle).Text(p.Position);
                            table.Cell().Element(CellStyle).Text(p.CategoryId.ToString());
                            table.Cell().Element(CellStyle).Text(p.CreatedAt.ToString("MMMM-dd-yyyy"));
                            table.Cell().Element(CellStyle).Text(p.UpdatedAt.ToString("MMMM-dd-yyyy"));
                        }
                    });
                });

                // Reports
                CreatePage("Reports Data", content =>
                {
                    content.Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Title").Bold();
                            header.Cell().Element(CellStyle).Text("Description").Bold();
                            header.Cell().Element(CellStyle).Text("CreatedAt").Bold();
                            header.Cell().Element(CellStyle).Text("UpdatedAt").Bold();
                            header.Cell().Element(CellStyle).Text("Image").Bold();
                        });

                        foreach (var r in reports)
                        {
                            table.Cell().Element(CellStyle).Text(r.Title);
                            table.Cell().Element(CellStyle).Text(r.Description);
                            table.Cell().Element(CellStyle).Text(r.CreatedAt.ToString("MMMM-dd-yyyy"));
                            table.Cell().Element(CellStyle).Text(r.UpdatedAt.ToString("MMMM-dd-yyyy"));
                            table.Cell().Element(cell =>
                            {
                                if (!string.IsNullOrEmpty(r.ImageName))
                                {
                                    var imagePath = Path.Combine(filePath, r.ImageName);
                                    if (System.IO.File.Exists(imagePath))
                                        cell.MinimalBox().Image(imagePath, ImageScaling.FitWidth);
                                    else
                                        cell.Text("[Image not found]");
                                }
                                else
                                {
                                    cell.Text("[No image]");
                                }
                            });
                        }
                    });
                });

            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "AllData.pdf");

            // Reusable cell styling
            IContainer CellStyle(IContainer container) =>
                container
                    .Border(1)
                    .BorderColor(Colors.Grey.Medium)
                    .Padding(4)
                    .AlignMiddle()
                    .AlignLeft();
        }

        public IActionResult ExportToExcelSimulated()
        {
            var months = new[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
            var rnd = new Random();

            // ✅ Simulated dataset
            var data = months.Select(m => new
            {
                Month = m,
                Volt = rnd.Next(5, 10),
                Thun = rnd.Next(5, 10),
                Bio = rnd.Next(4, 8),
                Kero = rnd.Next(3, 7),
                LY2024 = rnd.Next(20, 30),  // LY = actual previous year numbers
                Oplan = 25                  // Fixed demo OPLAN
            }).ToList();

            var forecast = data.Select(d => new
            {
                d.Month,
                d.Volt,
                d.Thun,
                d.Bio,
                d.Kero,
                Forecast2025 = d.Volt + d.Thun + d.Bio + d.Kero,
                d.LY2024,
                d.Oplan
            }).ToList();

            var firstHalf = forecast.Take(6).ToList();
            var secondHalf = forecast.Skip(6).ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Report");

            // ================= SUMMARY =================
            ws.Cell(1, 1).Value = "ALCALA";
            ws.Cell(2, 1).Value = "PRODUCT";
            ws.Cell(2, 2).Value = "1H 2025";
            ws.Cell(2, 3).Value = "2H 2025";
            ws.Cell(2, 4).Value = "TOTAL 2025";
            ws.Cell(2, 5).Value = "LY 2024 VOLUME";
            ws.Cell(2, 6).Value = "INC/DEC";
            ws.Cell(2, 7).Value = "% vs LY";
            ws.Cell(2, 8).Value = "2025 OPLAN";
            ws.Cell(2, 9).Value = "ATTAIN.";
            ws.Cell(2, 10).Value = "% vs OPLAN";

            string[] products = { "Thun", "Volt", "Bio", "Kero" };
            int row = 3;

            foreach (var p in products)
            {
                Func<dynamic, int> selector = p switch
                {
                    "Thun" => x => x.Thun,
                    "Volt" => x => x.Volt,
                    "Bio" => x => x.Bio,
                    "Kero" => x => x.Kero,
                    _ => x => 0
                };

                var firstHalfSum = firstHalf.Sum(selector);
                var secondHalfSum = secondHalf.Sum(selector);
                var total = firstHalfSum + secondHalfSum;

                // ✅ Compare total Forecast vs total LY for this product
                var ly = forecast.Sum(selector);   // sum LY values for same product
                var oplan = forecast.Sum(x => x.Oplan);  // Oplan sum (can also scale by product if needed)

                var incDec = total - ly;
                double perc = ly > 0 ? (double)incDec / ly : 0;
                var attain = total - oplan;
                double perc2 = oplan > 0 ? (double)total / oplan : 0;

                ws.Cell(row, 1).Value = p.ToUpper();
                ws.Cell(row, 2).Value = firstHalfSum;
                ws.Cell(row, 3).Value = secondHalfSum;
                ws.Cell(row, 4).Value = total;
                ws.Cell(row, 5).Value = ly;
                ws.Cell(row, 6).Value = incDec;
                ws.Cell(row, 7).Value = perc;
                ws.Cell(row, 8).Value = oplan;
                ws.Cell(row, 9).Value = attain;
                ws.Cell(row, 10).Value = perc2;
                row++;
            }

            // ================= MONTHLY BREAKDOWN =================
            int startRow = row + 2;
            ws.Cell(startRow, 1).Value = "FY";
            ws.Cell(startRow, 2).Value = "VOLT";
            ws.Cell(startRow, 3).Value = "THUN";
            ws.Cell(startRow, 4).Value = "BIO";
            ws.Cell(startRow, 5).Value = "KERO";
            ws.Cell(startRow, 6).Value = "FORECAST 2025";
            ws.Cell(startRow, 7).Value = "LY 2024";
            ws.Cell(startRow, 8).Value = "OPLAN";
            ws.Cell(startRow, 9).Value = "vs. LY";
            ws.Cell(startRow, 10).Value = "% vs LY";
            ws.Cell(startRow, 11).Value = "vs. OPLAN";
            ws.Cell(startRow, 12).Value = "% vs OPLAN";

            row = startRow + 1;
            foreach (var d in forecast)
            {
                int vsLy = d.Forecast2025 - d.LY2024;
                double vsLyPerc = d.LY2024 > 0 ? (double)vsLy / d.LY2024 : 0;
                int vsOplan = d.Forecast2025 - d.Oplan;
                double vsOplanPerc = d.Oplan > 0 ? (double)d.Forecast2025 / d.Oplan : 0;

                ws.Cell(row, 1).Value = d.Month;
                ws.Cell(row, 2).Value = d.Volt;
                ws.Cell(row, 3).Value = d.Thun;
                ws.Cell(row, 4).Value = d.Bio;
                ws.Cell(row, 5).Value = d.Kero;
                ws.Cell(row, 6).Value = d.Forecast2025;
                ws.Cell(row, 7).Value = d.LY2024;
                ws.Cell(row, 8).Value = d.Oplan;
                ws.Cell(row, 9).Value = vsLy;
                ws.Cell(row, 10).Value = vsLyPerc;
                ws.Cell(row, 11).Value = vsOplan;
                ws.Cell(row, 12).Value = vsOplanPerc;
                row++;
            }

            // ✅ Totals row
            int totalVolt = forecast.Sum(x => x.Volt);
            int totalThun = forecast.Sum(x => x.Thun);
            int totalBio = forecast.Sum(x => x.Bio);
            int totalKero = forecast.Sum(x => x.Kero);
            int totalForecast = forecast.Sum(x => x.Forecast2025);
            int totalLY = forecast.Sum(x => x.LY2024);
            int totalOplan = forecast.Sum(x => x.Oplan);

            ws.Cell(row, 1).Value = "TOTAL";
            ws.Cell(row, 2).Value = totalVolt;
            ws.Cell(row, 3).Value = totalThun;
            ws.Cell(row, 4).Value = totalBio;
            ws.Cell(row, 5).Value = totalKero;
            ws.Cell(row, 6).Value = totalForecast;
            ws.Cell(row, 7).Value = totalLY;
            ws.Cell(row, 8).Value = totalOplan;
            ws.Cell(row, 9).Value = totalForecast - totalLY;
            ws.Cell(row, 10).Value = totalLY > 0 ? (double)(totalForecast - totalLY) / totalLY : 0;
            ws.Cell(row, 11).Value = totalForecast - totalOplan;
            ws.Cell(row, 12).Value = totalOplan > 0 ? (double)totalForecast / totalOplan : 0;
            row++;

            // 1H Average row
            ws.Cell(row, 1).Value = "1H AVG";
            ws.Cell(row, 2).Value = firstHalf.Average(x => x.Volt);
            ws.Cell(row, 3).Value = firstHalf.Average(x => x.Thun);
            ws.Cell(row, 4).Value = firstHalf.Average(x => x.Bio);
            ws.Cell(row, 5).Value = firstHalf.Average(x => x.Kero);
            ws.Cell(row, 6).Value = firstHalf.Average(x => x.Forecast2025);
            ws.Cell(row, 7).Value = firstHalf.Average(x => x.LY2024);
            ws.Cell(row, 8).Value = firstHalf.Average(x => x.Oplan);

            double vsLy1H = ws.Cell(row, 6).GetDouble() - ws.Cell(row, 7).GetDouble();
            ws.Cell(row, 9).Value = vsLy1H;
            ws.Cell(row, 10).Value = ws.Cell(row, 7).GetDouble() > 0
                ? vsLy1H / ws.Cell(row, 7).GetDouble()
                : 0;

            double vsOplan1H = ws.Cell(row, 6).GetDouble() - ws.Cell(row, 8).GetDouble();
            ws.Cell(row, 11).Value = vsOplan1H;
            ws.Cell(row, 12).Value = ws.Cell(row, 8).GetDouble() > 0
                ? ws.Cell(row, 6).GetDouble() / ws.Cell(row, 8).GetDouble()
                : 0;
            row++;

            // 2H Average row
            ws.Cell(row, 1).Value = "2H AVG";
            ws.Cell(row, 2).Value = secondHalf.Average(x => x.Volt);
            ws.Cell(row, 3).Value = secondHalf.Average(x => x.Thun);
            ws.Cell(row, 4).Value = secondHalf.Average(x => x.Bio);
            ws.Cell(row, 5).Value = secondHalf.Average(x => x.Kero);
            ws.Cell(row, 6).Value = secondHalf.Average(x => x.Forecast2025);
            ws.Cell(row, 7).Value = secondHalf.Average(x => x.LY2024);
            ws.Cell(row, 8).Value = secondHalf.Average(x => x.Oplan);

            double vsLy2H = ws.Cell(row, 6).GetDouble() - ws.Cell(row, 7).GetDouble();
            ws.Cell(row, 9).Value = vsLy2H;
            ws.Cell(row, 10).Value = ws.Cell(row, 7).GetDouble() > 0
                ? vsLy2H / ws.Cell(row, 7).GetDouble()
                : 0;

            double vsOplan2H = ws.Cell(row, 6).GetDouble() - ws.Cell(row, 8).GetDouble();
            ws.Cell(row, 11).Value = vsOplan2H;
            ws.Cell(row, 12).Value = ws.Cell(row, 8).GetDouble() > 0
                ? ws.Cell(row, 6).GetDouble() / ws.Cell(row, 8).GetDouble()
                : 0;
            row++;

            // ✅ Format percentages
            //ws.Range(3, 7, row, 7).Style.NumberFormat.Format = "0%";
            ws.Range(3, 10, row, 10).Style.NumberFormat.Format = "0%";
            ws.Range(3, 12, row, 12).Style.NumberFormat.Format = "0%";

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Alcala_Report.xlsx");
        }

    }
}