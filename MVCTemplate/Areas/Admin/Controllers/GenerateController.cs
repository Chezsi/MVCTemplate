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

            // Simulated monthly dataset
            var data = months.Select(m => new
            {
                Month = m,
                Volt = rnd.Next(5, 10),
                Thun = rnd.Next(5, 10),
                Bio = rnd.Next(4, 8),
                Kero = rnd.Next(3, 7),
                LY2024 = rnd.Next(20, 30),
                Oplan = 25
            }).ToList();

            using var wb = new XLWorkbook();

            // ----------------------------
            // 1) Monthly sheet (Sheet 1)
            // ----------------------------
            var wsMonthly = wb.Worksheets.Add("Monthly");

            wsMonthly.Cell(1, 1).Value = "FY";
            wsMonthly.Cell(1, 2).Value = "VOLT";
            wsMonthly.Cell(1, 3).Value = "THUN";
            wsMonthly.Cell(1, 4).Value = "BIO";
            wsMonthly.Cell(1, 5).Value = "KERO";
            wsMonthly.Cell(1, 6).Value = "FORECAST 2025";
            wsMonthly.Cell(1, 7).Value = "LY 2024";
            wsMonthly.Cell(1, 8).Value = "OPLAN";
            wsMonthly.Cell(1, 9).Value = "vs. LY";
            wsMonthly.Cell(1, 10).Value = "% vs LY";
            wsMonthly.Cell(1, 11).Value = "vs. OPLAN";
            wsMonthly.Cell(1, 12).Value = "% vs OPLAN";

            for (int i = 0; i < data.Count; i++)
            {
                int r = 2 + i;
                var d = data[i];

                wsMonthly.Cell(r, 1).Value = d.Month;
                wsMonthly.Cell(r, 2).Value = d.Volt;
                wsMonthly.Cell(r, 3).Value = d.Thun;
                wsMonthly.Cell(r, 4).Value = d.Bio;
                wsMonthly.Cell(r, 5).Value = d.Kero;

                // ✅ Forecast2025 as formula (instead of static C# sum)
                wsMonthly.Cell(r, 6).FormulaA1 = $"=B{r}+C{r}+D{r}+E{r}";

                wsMonthly.Cell(r, 7).Value = d.LY2024;
                wsMonthly.Cell(r, 8).Value = d.Oplan;

                wsMonthly.Cell(r, 9).FormulaA1 = $"=F{r}-G{r}";
                wsMonthly.Cell(r, 10).FormulaA1 = $"=IF(G{r}=0,0,I{r}/G{r})";
                wsMonthly.Cell(r, 11).FormulaA1 = $"=F{r}-H{r}";
                wsMonthly.Cell(r, 12).FormulaA1 = $"=IF(H{r}=0,0,K{r}/H{r})";
            }

            int totalsRow = 14;
            wsMonthly.Cell(totalsRow, 1).Value = "TOTAL";
            wsMonthly.Cell(totalsRow, 2).FormulaA1 = "=SUM(B2:B13)";
            wsMonthly.Cell(totalsRow, 3).FormulaA1 = "=SUM(C2:C13)";
            wsMonthly.Cell(totalsRow, 4).FormulaA1 = "=SUM(D2:D13)";
            wsMonthly.Cell(totalsRow, 5).FormulaA1 = "=SUM(E2:E13)";
            wsMonthly.Cell(totalsRow, 6).FormulaA1 = "=SUM(F2:F13)";
            wsMonthly.Cell(totalsRow, 7).FormulaA1 = "=SUM(G2:G13)";
            wsMonthly.Cell(totalsRow, 8).FormulaA1 = "=SUM(H2:H13)";
            wsMonthly.Cell(totalsRow, 9).FormulaA1 = "=F14-G14";
            wsMonthly.Cell(totalsRow, 10).FormulaA1 = "=IF(G14=0,0,I14/G14)";
            wsMonthly.Cell(totalsRow, 11).FormulaA1 = "=F14-H14";
            wsMonthly.Cell(totalsRow, 12).FormulaA1 = "=IF(H14=0,0,K14/H14)";

            int avg1HRow = 15;
            wsMonthly.Cell(avg1HRow, 1).Value = "1H AVG";
            wsMonthly.Cell(avg1HRow, 2).FormulaA1 = "=AVERAGE(B2:B7)";
            wsMonthly.Cell(avg1HRow, 3).FormulaA1 = "=AVERAGE(C2:C7)";
            wsMonthly.Cell(avg1HRow, 4).FormulaA1 = "=AVERAGE(D2:D7)";
            wsMonthly.Cell(avg1HRow, 5).FormulaA1 = "=AVERAGE(E2:E7)";
            wsMonthly.Cell(avg1HRow, 6).FormulaA1 = "=AVERAGE(F2:F7)";
            wsMonthly.Cell(avg1HRow, 7).FormulaA1 = "=AVERAGE(G2:G7)";
            wsMonthly.Cell(avg1HRow, 8).FormulaA1 = "=AVERAGE(H2:H7)";
            wsMonthly.Cell(avg1HRow, 9).FormulaA1 = "=F15-G15";
            wsMonthly.Cell(avg1HRow, 10).FormulaA1 = "=IF(G15=0,0,I15/G15)";
            wsMonthly.Cell(avg1HRow, 11).FormulaA1 = "=F15-H15";
            wsMonthly.Cell(avg1HRow, 12).FormulaA1 = "=IF(H15=0,0,K15/H15)";

            int avg2HRow = 16;
            wsMonthly.Cell(avg2HRow, 1).Value = "2H AVG";
            wsMonthly.Cell(avg2HRow, 2).FormulaA1 = "=AVERAGE(B8:B13)";
            wsMonthly.Cell(avg2HRow, 3).FormulaA1 = "=AVERAGE(C8:C13)";
            wsMonthly.Cell(avg2HRow, 4).FormulaA1 = "=AVERAGE(D8:D13)";
            wsMonthly.Cell(avg2HRow, 5).FormulaA1 = "=AVERAGE(E8:E13)";
            wsMonthly.Cell(avg2HRow, 6).FormulaA1 = "=AVERAGE(F8:F13)";
            wsMonthly.Cell(avg2HRow, 7).FormulaA1 = "=AVERAGE(G8:G13)";
            wsMonthly.Cell(avg2HRow, 8).FormulaA1 = "=AVERAGE(H8:H13)";
            wsMonthly.Cell(avg2HRow, 9).FormulaA1 = "=F16-G16";
            wsMonthly.Cell(avg2HRow, 10).FormulaA1 = "=IF(G16=0,0,I16/G16)";
            wsMonthly.Cell(avg2HRow, 11).FormulaA1 = "=F16-H16";
            wsMonthly.Cell(avg2HRow, 12).FormulaA1 = "=IF(H16=0,0,K16/H16)";

            wsMonthly.Range("J2:J16").Style.NumberFormat.Format = "0%";
            wsMonthly.Range("L2:L16").Style.NumberFormat.Format = "0%";

            // ----------------------------
            // 2) Summary sheet (Sheet 2)
            // ----------------------------
            var wsSummary = wb.Worksheets.Add("Summary");

            wsSummary.Cell(1, 1).Value = "ALCALA";
            wsSummary.Cell(2, 1).Value = "PRODUCT";
            wsSummary.Cell(2, 2).Value = "1H 2025";
            wsSummary.Cell(2, 3).Value = "2H 2025";
            wsSummary.Cell(2, 4).Value = "TOTAL 2025";
            wsSummary.Cell(2, 5).Value = "LY 2024 VOLUME";
            wsSummary.Cell(2, 6).Value = "INC/DEC";
            wsSummary.Cell(2, 7).Value = "% vs LY";
            wsSummary.Cell(2, 8).Value = "2025 OPLAN";
            wsSummary.Cell(2, 9).Value = "ATTAIN.";
            wsSummary.Cell(2, 10).Value = "% vs OPLAN";

            var productCol = new Dictionary<string, string>
            {
                ["Volt"] = "B",
                ["Thun"] = "C",
                ["Bio"] = "D",
                ["Kero"] = "E"
            };

            int summaryStartRow = 3;
            var products = new[] { "Volt", "Thun", "Bio", "Kero" };

            for (int i = 0; i < products.Length; i++)
            {
                string p = products[i];
                string col = productCol[p];
                int r = summaryStartRow + i;

                wsSummary.Cell(r, 1).Value = p;
                wsSummary.Cell(r, 2).FormulaA1 = $"=SUM(Monthly!{col}2:Monthly!{col}7)";
                wsSummary.Cell(r, 3).FormulaA1 = $"=SUM(Monthly!{col}8:Monthly!{col}13)";
                wsSummary.Cell(r, 4).FormulaA1 = $"=B{r}+C{r}";

                wsSummary.Cell(r, 5).FormulaA1 =
                    $"=IF(SUM(Monthly!F2:F7)=0,0,SUM(Monthly!G2:G7)*(SUM(Monthly!{col}2:Monthly!{col}7)/SUM(Monthly!F2:F7)))" +
                    $" + IF(SUM(Monthly!F8:F13)=0,0,SUM(Monthly!G8:G13)*(SUM(Monthly!{col}8:Monthly!{col}13)/SUM(Monthly!F8:F13)))";

                wsSummary.Cell(r, 6).FormulaA1 = $"=D{r}-E{r}";
                wsSummary.Cell(r, 7).FormulaA1 = $"=IF(E{r}=0,0,F{r}/E{r})";

                wsSummary.Cell(r, 8).FormulaA1 =
                    $"=IF(SUM(Monthly!F2:F7)=0,0,SUM(Monthly!H2:H7)*(SUM(Monthly!{col}2:Monthly!{col}7)/SUM(Monthly!F2:F7)))" +
                    $" + IF(SUM(Monthly!F8:F13)=0,0,SUM(Monthly!H8:H13)*(SUM(Monthly!{col}8:Monthly!{col}13)/SUM(Monthly!F8:F13)))";

                wsSummary.Cell(r, 9).FormulaA1 = $"=D{r}-H{r}";
                wsSummary.Cell(r, 10).FormulaA1 = $"=IF(H{r}=0,0,I{r}/H{r})";
            }

            wsSummary.Range("G3:G6").Style.NumberFormat.Format = "0%";
            wsSummary.Range("J3:J6").Style.NumberFormat.Format = "0%";

            // ----------------------------
            // Return file
            // ----------------------------
            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Alcala_Report.xlsx");
        }

    }
}