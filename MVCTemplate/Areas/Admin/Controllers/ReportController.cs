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
using Microsoft.Extensions.Caching.Memory;
using ClosedXML.Excel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc; // for excel no license needed
using System.Text.RegularExpressions;

namespace MVCTemplate.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}, {Roles.User}")]
    [Area("Admin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _memoryCache;

        public ReportController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _memoryCache = memoryCache;
        }

        [HttpPost]
        public IActionResult GenerateDownloadToken()
        {
            var token = Guid.NewGuid().ToString();
            _memoryCache.Set(token, true, TimeSpan.FromMinutes(5));
            return Json(new { token });
        }

        private bool TryValidateAndConsumeToken(string token)
        {
            if (string.IsNullOrEmpty(token) || !_memoryCache.TryGetValue(token, out bool valid) || !valid)
            {
                return false;
            }

            // Remove the token to enforce one-time use
            _memoryCache.Remove(token);
            return true;
        }

        [HttpGet]
        public async Task<IActionResult> ExportToPdf(string token)
        {

            if (!TryValidateAndConsumeToken(token))
                return Unauthorized("Invalid or expired download token.");

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

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
                        .Text($"Report generated on {DateTime.Now:MM-dd-yyyy hh:mm tt}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);

                    // Main content
                    page.Content().Column(column =>
                    {
                        // Table title
                        column.Item().Element(container =>
                            container
                                .PaddingBottom(10)
                                .AlignCenter()
                                .Text("Reports Data")
                                .FontSize(16)
                                .Bold()
                                .Underline()
                        );

                        // Reports data table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);  // Title
                                columns.RelativeColumn(5);  // Description
                                columns.RelativeColumn(3);  // Image
                            });

                            // Table header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Title").Bold();
                                header.Cell().Element(CellStyle).Text("Description").Bold();
                                header.Cell().Element(CellStyle).Text("Image").Bold();
                            });

                            // Table rows
                            foreach (var report in reports)
                            {
                                // Title cell
                                table.Cell().Element(CellStyle).Element(cell =>
                                    cell.MinimalBox().ShowOnce().Text(report.Title ?? "")
                                );

                                // Description cell
                                table.Cell().Element(CellStyle).Element(cell =>
                                    cell.MinimalBox().ShowOnce().Text(report.Description ?? "")
                                );

                                // Image cell
                                table.Cell().Element(CellStyle).Element(cell =>
                                {
                                    if (!string.IsNullOrEmpty(report.ImageName))
                                    {
                                        var imagePath = Path.Combine(filePath, report.ImageName);
                                        if (System.IO.File.Exists(imagePath))
                                        {
                                            cell.MinimalBox()
                                                .ShowOnce()
                                                .Image(imagePath, ImageScaling.FitWidth);
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

            return File(pdfBytes, "application/pdf", "Reports.pdf");

            // Helper for consistent cell styling
            IContainer CellStyle(IContainer container) =>
                container
                    .Border(1)
                    .BorderColor(Colors.Grey.Medium)
                    .Padding(5)
                    .AlignMiddle()
                    .AlignCenter();
        }

        /*[HttpGet]     NO DELIMETER
        public async Task<IActionResult> ExportToExcel(string token)
        {

            if (!TryValidateAndConsumeToken(token))
                return Unauthorized("Invalid or expired download token.");

            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            var reports = await _context.Reports.ToListAsync();
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reports");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Reports");

                // Row 1 - Title
                worksheet.Cells["A1:C1"].Merge = true;
                worksheet.Cells["A1"].Value = "Reports Data";
                worksheet.Cells["A1"].Style.Font.Size = 18;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Height = 25;

                // Row 2 - Generated on date & time
                worksheet.Cells["A2:C2"].Merge = true;
                worksheet.Cells["A2"].Value = $"Generated on: {DateTime.Now:MM-dd-yyyy hh:mm tt}";
                worksheet.Cells["A2"].Style.Font.Size = 12;
                worksheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(2).Height = 20;

                // Row 3 - Headers
                worksheet.Cells[3, 1].Value = "Title";
                worksheet.Cells[3, 2].Value = "Description";
                worksheet.Cells[3, 3].Value = "Image";

                // Style first three rows: blue background and white text
                var blueBackground = System.Drawing.Color.FromArgb(0, 51, 102);
                worksheet.Cells["A1:C3"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:C3"].Style.Fill.BackgroundColor.SetColor(blueBackground);
                worksheet.Cells["A1:C3"].Style.Font.Color.SetColor(System.Drawing.Color.White);

                worksheet.Row(3).Height = 22;
                worksheet.Cells["A3:C3"].Style.Font.Bold = true;
                worksheet.Cells["A3:C3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                // Set fixed column widths for Title and Description only
                worksheet.Column(1).Width = 30;
                worksheet.Column(2).Width = 50;

                int row = 4;
                var lightGreen = System.Drawing.Color.FromArgb(198, 239, 206);
                var darkGreen = System.Drawing.Color.FromArgb(155, 187, 89);

                foreach (var report in reports)
                {
                    worksheet.Cells[row, 1].Value = report.Title;
                    worksheet.Cells[row, 2].Value = report.Description;

                    // Set alternating row colors
                    var fillColor = (row % 2 == 0) ? lightGreen : darkGreen;
                    worksheet.Cells[row, 1, row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 3].Style.Fill.BackgroundColor.SetColor(fillColor);

                    // Insert image if exists
                    if (!string.IsNullOrEmpty(report.ImageName))
                    {
                        var imagePath = Path.Combine(filePath, report.ImageName);
                        if (System.IO.File.Exists(imagePath))
                        {
                            using (var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                            using (var image = System.Drawing.Image.FromStream(imageStream))
                            {
                                int targetWidth = 80;
                                int targetHeight = 80;

                                double ratioX = (double)targetWidth / image.Width;
                                double ratioY = (double)targetHeight / image.Height;
                                double ratio = Math.Min(ratioX, ratioY);

                                int finalWidth = (int)(image.Width * ratio);
                                int finalHeight = (int)(image.Height * ratio);

                                double excelColWidth = targetWidth / 7.5;
                                worksheet.Column(3).Width = excelColWidth;

                                worksheet.Row(row).Height = finalHeight * 0.75;

                                imageStream.Position = 0;

                                var excelImage = worksheet.Drawings.AddPicture($"img_{row}", imageStream);
                                excelImage.SetSize(finalWidth, finalHeight);
                                excelImage.SetPosition(row - 1, 0, 2, 0);
                                excelImage.EditAs = eEditAs.OneCell;
                                excelImage.Locked = true;
                                excelImage.LockAspectRatio = true;
                            }
                        }
                    }
                    else
                    {
                        // No image: set default row height (optional)
                        worksheet.Row(row).Height = 15;
                    }

                    row++;
                }

                // Borders
                using (var range = worksheet.Cells[3, 1, row - 1, 3])
                {
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    var thick = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
                    worksheet.Cells["A1:C3"].Style.Border.Top.Style = thick;
                    worksheet.Cells["A1:C3"].Style.Border.Bottom.Style = thick;
                    worksheet.Cells["A1:C3"].Style.Border.Left.Style = thick;
                    worksheet.Cells["A1:C3"].Style.Border.Right.Style = thick;

                    worksheet.Cells[3, 1, row - 1, 3].Style.Border.Top.Style = thick;
                    worksheet.Cells[3, 1, row - 1, 3].Style.Border.Bottom.Style = thick;
                    worksheet.Cells[3, 1, row - 1, 3].Style.Border.Left.Style = thick;
                    worksheet.Cells[3, 1, row - 1, 3].Style.Border.Right.Style = thick;
                }

                // AutoFilter
                worksheet.Cells["A3:C3"].AutoFilter = true;

                // Lock and protect worksheet
                worksheet.Cells.Style.Locked = true;
                worksheet.Protection.SetPassword("YourPassword");
                worksheet.Protection.AllowSelectLockedCells = true;
                worksheet.Protection.AllowSelectUnlockedCells = true;
                worksheet.Protection.AllowAutoFilter = true;
                worksheet.Protection.AllowEditObject = false;
                worksheet.Protection.AllowEditScenarios = false;
                worksheet.Protection.IsProtected = true;

                package.Workbook.Protection.SetPassword("YourPassword");
                package.Workbook.Protection.LockStructure = true;
                package.Workbook.Protection.LockWindows = true;

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"Reports.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }   NO DELIMETER*/

        /* [HttpGet]        HAS DELIMTER, NO DESCRIPTION COUNT
        public async Task<IActionResult> ExportToExcel(string token)
        {
            if (!TryValidateAndConsumeToken(token))
                return Unauthorized("Invalid or expired download token.");

            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            var reports = await _context.Reports.ToListAsync();
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reports");

            // Determine max number of description parts across all reports
            int maxDescriptionParts = reports
                .Select(r => r.Description?.Split('-').Length ?? 1)
                .DefaultIfEmpty(1)
                .Max();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Reports");

                // Row 1 - Title
                worksheet.Cells["A1"].Value = "Reports Data";
                worksheet.Cells[1, 1, 1, 2 + maxDescriptionParts].Merge = true;
                worksheet.Cells["A1"].Style.Font.Size = 18;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Height = 25;

                // Row 2 - Timestamp
                worksheet.Cells[2, 1, 2, 2 + maxDescriptionParts].Merge = true;
                worksheet.Cells["A2"].Value = $"Generated on: {DateTime.Now:MM-dd-yyyy hh:mm tt}";
                worksheet.Cells["A2"].Style.Font.Size = 12;
                worksheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(2).Height = 20;

                // Row 3 - Headers
                worksheet.Cells[3, 1].Value = "Title";
                for (int i = 0; i < maxDescriptionParts; i++)
                {
                    worksheet.Cells[3, 2 + i].Value = $"Description {i + 1}";
                }
                worksheet.Cells[3, 2 + maxDescriptionParts].Value = "Image";

                // Style header and title rows
                var totalCols = 2 + maxDescriptionParts;
                var headerRange = worksheet.Cells[1, 1, 3, totalCols];
                var blueBackground = System.Drawing.Color.FromArgb(0, 51, 102);
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(blueBackground);
                headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
                worksheet.Row(3).Height = 22;
                worksheet.Cells[3, 1, 3, totalCols].Style.Font.Bold = true;
                worksheet.Cells[3, 1, 3, totalCols].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                worksheet.Column(1).Width = 30;
                for (int i = 0; i < maxDescriptionParts; i++)
                {
                    worksheet.Column(2 + i).Width = 25;
                }

                int row = 4;
                var lightGreen = System.Drawing.Color.FromArgb(198, 239, 206);
                var darkGreen = System.Drawing.Color.FromArgb(155, 187, 89);

                foreach (var report in reports)
                {
                    worksheet.Cells[row, 1].Value = report.Title;

                    // Split description into parts by dash
                    var descriptionParts = (report.Description ?? "").Split('-');

                    for (int i = 0; i < maxDescriptionParts; i++)
                    {
                        worksheet.Cells[row, 2 + i].Value =
                            i < descriptionParts.Length ? descriptionParts[i].Trim().ToUpper() : "";
                    }

                    var fillColor = (row % 2 == 0) ? lightGreen : darkGreen;
                    worksheet.Cells[row, 1, row, totalCols].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, totalCols].Style.Fill.BackgroundColor.SetColor(fillColor);

                    if (!string.IsNullOrEmpty(report.ImageName))
                    {
                        var imagePath = Path.Combine(filePath, report.ImageName);
                        if (System.IO.File.Exists(imagePath))
                        {
                            using (var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                            using (var image = System.Drawing.Image.FromStream(imageStream))
                            {
                                int targetWidth = 80;
                                int targetHeight = 80;

                                double ratio = Math.Min((double)targetWidth / image.Width, (double)targetHeight / image.Height);
                                int finalWidth = (int)(image.Width * ratio);
                                int finalHeight = (int)(image.Height * ratio);

                                worksheet.Column(2 + maxDescriptionParts).Width = targetWidth / 7.5;
                                worksheet.Row(row).Height = finalHeight * 0.75;

                                imageStream.Position = 0;
                                var excelImage = worksheet.Drawings.AddPicture($"img_{row}", imageStream);
                                excelImage.SetSize(finalWidth, finalHeight);
                                excelImage.SetPosition(row - 1, 0, 1 + maxDescriptionParts, 0);
                                excelImage.EditAs = eEditAs.OneCell;
                                excelImage.Locked = true;
                                excelImage.LockAspectRatio = true;
                            }
                        }
                    }
                    else
                    {
                        worksheet.Row(row).Height = 15;
                    }

                    row++;
                }

                // Borders
                using (var range = worksheet.Cells[3, 1, row - 1, totalCols])
                {
                    var thin = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    var thick = OfficeOpenXml.Style.ExcelBorderStyle.Medium;

                    range.Style.Border.Left.Style = thin;
                    range.Style.Border.Right.Style = thin;
                    range.Style.Border.Top.Style = thin;
                    range.Style.Border.Bottom.Style = thin;

                    worksheet.Cells[1, 1, 3, totalCols].Style.Border.Top.Style = thick;
                    worksheet.Cells[1, 1, 3, totalCols].Style.Border.Bottom.Style = thick;
                    worksheet.Cells[1, 1, 3, totalCols].Style.Border.Left.Style = thick;
                    worksheet.Cells[1, 1, 3, totalCols].Style.Border.Right.Style = thick;

                    worksheet.Cells[3, 1, row - 1, totalCols].Style.Border.Top.Style = thick;
                    worksheet.Cells[3, 1, row - 1, totalCols].Style.Border.Bottom.Style = thick;
                    worksheet.Cells[3, 1, row - 1, totalCols].Style.Border.Left.Style = thick;
                    worksheet.Cells[3, 1, row - 1, totalCols].Style.Border.Right.Style = thick;
                }

                worksheet.Cells[3, 1, 3, totalCols].AutoFilter = true;

                worksheet.Cells.Style.Locked = true;
                worksheet.Protection.SetPassword("YourPassword");
                worksheet.Protection.AllowSelectLockedCells = true;
                worksheet.Protection.AllowSelectUnlockedCells = true;
                worksheet.Protection.AllowAutoFilter = true;
                worksheet.Protection.AllowEditObject = false;
                worksheet.Protection.AllowEditScenarios = false;
                worksheet.Protection.IsProtected = true;

                package.Workbook.Protection.SetPassword("YourPassword");
                package.Workbook.Protection.LockStructure = true;
                package.Workbook.Protection.LockWindows = true;

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = "Reports.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }       HAS DELIMTER, NO DESCRIPTION COUNT*/

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string token)
        {
            if (!TryValidateAndConsumeToken(token))
                return Unauthorized("Invalid or expired download token.");

            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            var reports = await _context.Reports.ToListAsync();
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reports");

            int maxDescriptionParts = reports
                .Select(r => Regex.Split(r.Description ?? "", @"[\\|,\/\-\.]+").Length)
                .DefaultIfEmpty(1)
                .Max();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Reports");

                // Row 1 - Title
                worksheet.Cells["A1"].Value = "Reports Data";
                worksheet.Cells[1, 1, 1, 3 + maxDescriptionParts].Merge = true;
                worksheet.Cells["A1"].Style.Font.Size = 18;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Height = 25;

                // Row 2 - Timestamp
                worksheet.Cells[2, 1, 2, 3 + maxDescriptionParts].Merge = true;
                worksheet.Cells["A2"].Value = $"Generated on: {DateTime.Now:MM-dd-yyyy hh:mm tt}";
                worksheet.Cells["A2"].Style.Font.Size = 12;
                worksheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(2).Height = 20;

                // Row 4 - Headers
                worksheet.Cells[4, 1].Value = "Title";
                worksheet.Cells[4, 2].Value = "Desc Count";

                for (int i = 0; i < maxDescriptionParts; i++)
                {
                    worksheet.Cells[4, 3 + i].Value = $"Description {i + 1}";
                }

                int imageColIndex = 3 + maxDescriptionParts;
                worksheet.Cells[4, imageColIndex].Value = "Image";

                int totalCols = imageColIndex;

                // Style headers
                var blueBackground = System.Drawing.Color.FromArgb(0, 51, 102);
                var headerRange = worksheet.Cells[1, 1, 4, totalCols];
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(blueBackground);
                headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
                headerRange.Style.Font.Bold = true;
                worksheet.Row(4).Height = 22;
                worksheet.Cells[4, 1, 4, totalCols].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                // Column widths
                worksheet.Column(1).Width = 30;
                worksheet.Column(2).Width = 15;
                for (int i = 0; i < maxDescriptionParts; i++)
                {
                    worksheet.Column(3 + i).Width = 25;
                }
                worksheet.Column(imageColIndex).Width = 15;

                int row = 5;
                var lightGreen = System.Drawing.Color.FromArgb(198, 239, 206);
                var darkGreen = System.Drawing.Color.FromArgb(155, 187, 89);

                foreach (var report in reports)
                {
                    worksheet.Cells[row, 1].Value = report.Title;

                    var descriptionParts = Regex.Split(report.Description ?? "", @"[\\|,\/\-\.]+")
                            .Where(p => !string.IsNullOrWhiteSpace(p))
                            .ToArray();

                    worksheet.Cells[row, 2].Value = descriptionParts.Length;

                    for (int i = 0; i < maxDescriptionParts; i++)
                    {
                        worksheet.Cells[row, 3 + i].Value =
                            i < descriptionParts.Length ? descriptionParts[i].Trim().ToUpper() : "";
                    }

                    var fillColor = (row % 2 == 0) ? lightGreen : darkGreen;
                    worksheet.Cells[row, 1, row, totalCols].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, totalCols].Style.Fill.BackgroundColor.SetColor(fillColor);

                    // Insert image if exists
                    if (!string.IsNullOrEmpty(report.ImageName))
                    {
                        var imagePath = Path.Combine(filePath, report.ImageName);
                        if (System.IO.File.Exists(imagePath))
                        {
                            using (var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                            using (var image = System.Drawing.Image.FromStream(imageStream))
                            {
                                int targetWidth = 80;
                                int targetHeight = 80;
                                double ratio = Math.Min((double)targetWidth / image.Width, (double)targetHeight / image.Height);
                                int finalWidth = (int)(image.Width * ratio);
                                int finalHeight = (int)(image.Height * ratio);

                                worksheet.Column(imageColIndex).Width = targetWidth / 7.5;
                                worksheet.Row(row).Height = finalHeight * 0.75;

                                imageStream.Position = 0;
                                var excelImage = worksheet.Drawings.AddPicture($"img_{row}", imageStream);
                                excelImage.SetSize(finalWidth, finalHeight);
                                excelImage.SetPosition(row - 1, 0, imageColIndex - 1, 0);
                                excelImage.EditAs = eEditAs.OneCell;
                                excelImage.Locked = true;
                                excelImage.LockAspectRatio = true;
                            }
                        }
                    }
                    else
                    {
                        worksheet.Row(row).Height = 18;
                    }

                    row++;
                }

                // Add borders
                using (var range = worksheet.Cells[4, 1, row - 1, totalCols])
                {
                    var thin = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    var thick = OfficeOpenXml.Style.ExcelBorderStyle.Medium;

                    range.Style.Border.Left.Style = thin;
                    range.Style.Border.Right.Style = thin;
                    range.Style.Border.Top.Style = thin;
                    range.Style.Border.Bottom.Style = thin;

                    worksheet.Cells[1, 1, 4, totalCols].Style.Border.Top.Style = thick;
                    worksheet.Cells[1, 1, 4, totalCols].Style.Border.Bottom.Style = thick;
                    worksheet.Cells[1, 1, 4, totalCols].Style.Border.Left.Style = thick;
                    worksheet.Cells[1, 1, 4, totalCols].Style.Border.Right.Style = thick;

                    worksheet.Cells[4, 1, row - 1, totalCols].Style.Border.Top.Style = thick;
                    worksheet.Cells[4, 1, row - 1, totalCols].Style.Border.Bottom.Style = thick;
                    worksheet.Cells[4, 1, row - 1, totalCols].Style.Border.Left.Style = thick;
                    worksheet.Cells[4, 1, row - 1, totalCols].Style.Border.Right.Style = thick;
                }

                // AutoFilter
                worksheet.Cells[4, 1, row - 1, totalCols].AutoFilter = true;

                // Protection
                worksheet.Cells.Style.Locked = true;
                worksheet.Protection.SetPassword("YourPassword");
                worksheet.Protection.AllowSelectLockedCells = true;
                worksheet.Protection.AllowSelectUnlockedCells = true;
                worksheet.Protection.AllowAutoFilter = true;
                worksheet.Protection.AllowEditObject = false;
                worksheet.Protection.AllowEditScenarios = false;
                worksheet.Protection.IsProtected = true;

                package.Workbook.Protection.SetPassword("YourPassword");
                package.Workbook.Protection.LockStructure = true;
                package.Workbook.Protection.LockWindows = true;

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = "Reports.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpGet] // for testing only
        public async Task<IActionResult> ExcelSample()
        {
            var reports = await _context.Reports.ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reports");

            int totalCols = 3; // Title + Desc Count + Combined Description

            // Row 1 - Title
            worksheet.Cell("A1").Value = "Reports Data";
            worksheet.Range(1, 1, 1, totalCols).Merge();
            worksheet.Cell("A1").Style.Font.FontSize = 18;
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(1).Height = 25;

            // Row 2 - Timestamp
            worksheet.Range(2, 1, 2, totalCols).Merge();
            worksheet.Cell("A2").Value = $"Generated on: {DateTime.Now:MM-dd-yyyy hh:mm tt}";
            worksheet.Cell("A2").Style.Font.FontSize = 12;
            worksheet.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(2).Height = 20;

            // Row 4 - Headers
            worksheet.Cell(4, 1).Value = "Title";
            worksheet.Cell(4, 2).Value = "Desc Count";
            worksheet.Cell(4, 3).Value = "Description Details";

            // Style headers
            var blueBackground = XLColor.FromArgb(0, 51, 102);
            var headerRange = worksheet.Range(1, 1, 4, totalCols);
            headerRange.Style.Fill.BackgroundColor = blueBackground;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Font.Bold = true;
            worksheet.Row(4).Height = 22;
            worksheet.Range(4, 1, 4, totalCols).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // Column widths
            worksheet.Column(1).Width = 30;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 50;

            // Colors for rows
            var lightGreen = XLColor.FromArgb(198, 239, 206);
            var darkGreen = XLColor.FromArgb(155, 187, 89);

            int row = 5;
            foreach (var report in reports)
            {
                worksheet.Cell(row, 1).Value = report.Title;

                var rawParts = Regex.Split(report.Description ?? "", @"[\\|,\/\-\.]+")
                                    .Where(p => !string.IsNullOrWhiteSpace(p))
                                    .ToList();

                worksheet.Cell(row, 2).Value = rawParts.Count;

                var combinedDescription = rawParts.Count > 0
                    ? string.Join("  ", rawParts.Select((part, index) => $"{index + 1}.) {part.Trim().ToUpper()}"))
                    : "None";

                worksheet.Cell(row, 3).Value = combinedDescription;
                worksheet.Row(row).Height = 18;

                // Alternate row colors
                var fillColor = (row % 2 == 0) ? lightGreen : darkGreen;
                worksheet.Range(row, 1, row, totalCols).Style.Fill.BackgroundColor = fillColor;

                row++;
            }

            // Add borders
            var dataRange = worksheet.Range(4, 1, row - 1, totalCols);

            dataRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

            var headerBorderRange = worksheet.Range(1, 1, 4, totalCols);
            headerBorderRange.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            headerBorderRange.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            headerBorderRange.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
            headerBorderRange.Style.Border.RightBorder = XLBorderStyleValues.Medium;

            // AutoFilter
            worksheet.Range(4, 1, row - 1, totalCols).SetAutoFilter();

            // Save to MemoryStream
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = "Reports.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        // for testing only

        [HttpGet]
        public async Task<IActionResult> ExportFilteredToExcel(string? titleFilter, string? descriptionFilter, string token)
        {
            if (!TryValidateAndConsumeToken(token))
                return Unauthorized("Invalid or expired download token.");

            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            var reportsQuery = _context.Reports.AsQueryable();

            if (!string.IsNullOrWhiteSpace(titleFilter))
            {
                reportsQuery = reportsQuery.Where(r => r.Title != null && r.Title.Contains(titleFilter));
            }
            if (!string.IsNullOrWhiteSpace(descriptionFilter))
            {
                reportsQuery = reportsQuery.Where(r => r.Description != null && r.Description.Contains(descriptionFilter));
            }

            var reports = await reportsQuery.ToListAsync();
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reports");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Filtered Reports");

                // Row 1 - Title
                worksheet.Cells["A1:C1"].Merge = true;
                worksheet.Cells["A1"].Value = "Filtered Reports Data";
                worksheet.Cells["A1"].Style.Font.Size = 18;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Height = 25;

                // Row 2 - Name filter
                worksheet.Cells["A2:C2"].Merge = true;
                string namePart = string.IsNullOrWhiteSpace(titleFilter) ? "" : titleFilter;
                worksheet.Cells["A2"].Value = $"With Name: {namePart}";
                worksheet.Cells["A2"].Style.Font.Size = 12;
                worksheet.Cells["A2"].Style.Font.Italic = true;
                worksheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(2).Height = 18;

                // Row 3 - Description filter
                worksheet.Cells["A3:C3"].Merge = true;
                string descPart = string.IsNullOrWhiteSpace(descriptionFilter) ? "" : descriptionFilter;
                worksheet.Cells["A3"].Value = $"With Description: {descPart}";
                worksheet.Cells["A3"].Style.Font.Size = 12;
                worksheet.Cells["A3"].Style.Font.Italic = true;
                worksheet.Cells["A3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(3).Height = 18;

                // Row 4 - Generated on
                worksheet.Cells["A4:C4"].Merge = true;
                worksheet.Cells["A4"].Value = $"Generated on: {DateTime.Now:MM-dd-yyyy hh:mm tt}";
                worksheet.Cells["A4"].Style.Font.Size = 12;
                worksheet.Cells["A4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(4).Height = 20;

                // Row 5 - Headers
                worksheet.Cells[5, 1].Value = "Title";
                worksheet.Cells[5, 2].Value = "Description";
                worksheet.Cells[5, 3].Value = "Image";

                var blueBackground = System.Drawing.Color.FromArgb(0, 51, 102);
                worksheet.Cells["A1:C5"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1:C5"].Style.Fill.BackgroundColor.SetColor(blueBackground);
                worksheet.Cells["A1:C5"].Style.Font.Color.SetColor(System.Drawing.Color.White);

                worksheet.Row(5).Height = 22;
                worksheet.Cells["A5:C5"].Style.Font.Bold = true;
                worksheet.Cells["A5:C5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                worksheet.Column(1).Width = 30;
                worksheet.Column(2).Width = 50;

                int row = 6;
                var lightGreen = System.Drawing.Color.FromArgb(198, 239, 206);
                var darkGreen = System.Drawing.Color.FromArgb(155, 187, 89);

                foreach (var report in reports)
                {
                    worksheet.Cells[row, 1].Value = report.Title;
                    worksheet.Cells[row, 2].Value = report.Description;

                    var fillColor = (row % 2 == 0) ? lightGreen : darkGreen;
                    worksheet.Cells[row, 1, row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 3].Style.Fill.BackgroundColor.SetColor(fillColor);

                    if (!string.IsNullOrEmpty(report.ImageName))
                    {
                        var imagePath = Path.Combine(filePath, report.ImageName);
                        if (System.IO.File.Exists(imagePath))
                        {
                            using (var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                            using (var image = System.Drawing.Image.FromStream(imageStream))
                            {
                                int targetWidth = 80;
                                int targetHeight = 80;

                                double ratioX = (double)targetWidth / image.Width;
                                double ratioY = (double)targetHeight / image.Height;
                                double ratio = Math.Min(ratioX, ratioY);

                                int finalWidth = (int)(image.Width * ratio);
                                int finalHeight = (int)(image.Height * ratio);

                                double excelColWidth = targetWidth / 7.5;
                                worksheet.Column(3).Width = excelColWidth;

                                worksheet.Row(row).Height = finalHeight * 0.75;

                                imageStream.Position = 0;

                                var excelImage = worksheet.Drawings.AddPicture($"img_{row}", imageStream);
                                excelImage.SetSize(finalWidth, finalHeight);
                                excelImage.SetPosition(row - 1, 0, 2, 0);
                                excelImage.EditAs = eEditAs.OneCell;
                                excelImage.Locked = true;
                                excelImage.LockAspectRatio = true;
                            }
                        }
                    }
                    else
                    {
                        worksheet.Row(row).Height = 15;
                    }

                    row++;
                }

                using (var range = worksheet.Cells[5, 1, row - 1, 3])
                {
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    var thick = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
                    worksheet.Cells["A1:C5"].Style.Border.Top.Style = thick;
                    worksheet.Cells["A1:C5"].Style.Border.Bottom.Style = thick;
                    worksheet.Cells["A1:C5"].Style.Border.Left.Style = thick;
                    worksheet.Cells["A1:C5"].Style.Border.Right.Style = thick;

                    worksheet.Cells[5, 1, row - 1, 3].Style.Border.Top.Style = thick;
                    worksheet.Cells[5, 1, row - 1, 3].Style.Border.Bottom.Style = thick;
                    worksheet.Cells[5, 1, row - 1, 3].Style.Border.Left.Style = thick;
                    worksheet.Cells[5, 1, row - 1, 3].Style.Border.Right.Style = thick;
                }

                worksheet.Cells["A5:C5"].AutoFilter = true;

                worksheet.Cells.Style.Locked = true;
                worksheet.Protection.SetPassword("YourPassword");
                worksheet.Protection.AllowSelectLockedCells = true;
                worksheet.Protection.AllowSelectUnlockedCells = true;
                worksheet.Protection.AllowAutoFilter = true;
                worksheet.Protection.AllowEditObject = false;
                worksheet.Protection.AllowEditScenarios = false;
                worksheet.Protection.IsProtected = true;

                package.Workbook.Protection.SetPassword("YourPassword");
                package.Workbook.Protection.LockStructure = true;
                package.Workbook.Protection.LockWindows = true;

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"FilteredReports.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportFilteredToPdf(string? titleFilter, string? descriptionFilter, string token)
        {
            if (!TryValidateAndConsumeToken(token))
                return Unauthorized("Invalid or expired download token.");

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var reportsQuery = _context.Reports.AsQueryable();

            if (!string.IsNullOrWhiteSpace(titleFilter))
                reportsQuery = reportsQuery.Where(r => r.Title != null && r.Title.Contains(titleFilter));

            if (!string.IsNullOrWhiteSpace(descriptionFilter))
                reportsQuery = reportsQuery.Where(r => r.Description != null && r.Description.Contains(descriptionFilter));

            var reports = await reportsQuery.ToListAsync();
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

                    // Footer with generation date
                    page.Footer()
                        .AlignRight()
                        .Text($"Report generated on {DateTime.Now:MM-dd-yyyy hh:mm tt}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);

                    // Page content
                    page.Content().Column(column =>
                    {
                        // Table title
                        column.Item()
                            .PaddingBottom(5)
                            .AlignCenter()
                            .Text("Filtered Reports Data")
                            .FontSize(16)
                            .Bold()
                            .Underline();

                        // Filter description
                        column.Item()
                            .PaddingBottom(10)
                            .Text(text =>
                            {
                                if (!string.IsNullOrWhiteSpace(titleFilter) && !string.IsNullOrWhiteSpace(descriptionFilter))
                                    text.Span($"Filter - Title: \"{titleFilter}\", Description: \"{descriptionFilter}\"")
                                        .FontSize(12)
                                        .FontColor(Colors.Grey.Darken2);
                                else if (!string.IsNullOrWhiteSpace(titleFilter))
                                    text.Span($"Filter - Title: \"{titleFilter}\"")
                                        .FontSize(12)
                                        .FontColor(Colors.Grey.Darken2);
                                else if (!string.IsNullOrWhiteSpace(descriptionFilter))
                                    text.Span($"Filter - Description: \"{descriptionFilter}\"")
                                        .FontSize(12)
                                        .FontColor(Colors.Grey.Darken2);
                                else
                                    text.Span("No filters applied.")
                                        .FontSize(12)
                                        .FontColor(Colors.Grey.Darken2);
                            });

                        // Reports Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(3);
                            });

                            // Table header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Title").Bold();
                                header.Cell().Element(CellStyle).Text("Description").Bold();
                                header.Cell().Element(CellStyle).Text("Image").Bold();
                            });

                            // Data rows
                            foreach (var report in reports)
                            {
                                // Title cell
                                table.Cell().Element(CellStyle).Element(cell =>
                                    cell.MinimalBox().ShowOnce().Text(report.Title ?? "")
                                );

                                // Description cell
                                table.Cell().Element(CellStyle).Element(cell =>
                                    cell.MinimalBox().ShowOnce().Text(report.Description ?? "")
                                );

                                // Image cell
                                table.Cell().Element(CellStyle).Element(cell =>
                                {
                                    if (!string.IsNullOrEmpty(report.ImageName))
                                    {
                                        var imagePath = Path.Combine(filePath, report.ImageName);
                                        if (System.IO.File.Exists(imagePath))
                                        {
                                            cell.MinimalBox()
                                                .ShowOnce()
                                                .Image(imagePath, ImageScaling.FitWidth);
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

                                    return cell;
                                });
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "FilteredReports.pdf");

            // Helper to style cells
            IContainer CellStyle(IContainer container)
            {
                return container
                    .Border(1)
                    .BorderColor(Colors.Grey.Medium)
                    .Padding(5)
                    .AlignMiddle()
                    .AlignCenter();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile excelFile)
        {
            Console.WriteLine($"ImportExcel started at {DateTime.Now}");

            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid Excel file.");
                return RedirectToAction("Index");
            }

            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "reports");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            using var stream = excelFile.OpenReadStream();
            IWorkbook workbook = null;

            try
            {
                var extension = Path.GetExtension(excelFile.FileName).ToLower();
                if (extension == ".xls")
                    workbook = new NPOI.HSSF.UserModel.HSSFWorkbook(stream);
                else if (extension == ".xlsx")
                    workbook = new XSSFWorkbook(stream);
                else
                    throw new Exception("Invalid file format");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error reading Excel file: " + ex.Message);
                return RedirectToAction("Index");
            }

            var sheet = workbook.GetSheetAt(0);

            var drawing = sheet.CreateDrawingPatriarch() as XSSFDrawing;
            var shapes = drawing?.GetShapes();

            var picturesByRow = new Dictionary<int, (byte[] Data, string Extension)>();

            if (shapes != null)
            {
                foreach (var shape in shapes)
                {
                    if (shape is XSSFPicture pic)
                    {
                        var anchor = pic.ClientAnchor as XSSFClientAnchor;
                        if (anchor == null)
                            continue;

                        int row1 = anchor.Row1;

                        var pictureData = pic.PictureData;
                        var data = pictureData.Data;
                        var ext = pictureData.SuggestFileExtension();

                        picturesByRow[row1] = (data, ext);
                    }
                }
            }

            for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null) continue;

                var titleCell = row.GetCell(0);
                string title = titleCell?.ToString().Trim() ?? string.Empty;

                // Corrected: description is in column 2, index 1
                var descCell = row.GetCell(1);
                string description = string.Empty;
                if (descCell != null)
                {
                    switch (descCell.CellType)
                    {
                        case NPOI.SS.UserModel.CellType.String:
                            description = descCell.StringCellValue.Trim();
                            break;
                        case NPOI.SS.UserModel.CellType.Numeric:
                            description = descCell.NumericCellValue.ToString();
                            break;
                        case NPOI.SS.UserModel.CellType.Formula:
                            var evaluator = WorkbookFactory.CreateFormulaEvaluator(descCell.Sheet.Workbook);
                            var evaluatedValue = evaluator.Evaluate(descCell);
                            if (evaluatedValue != null)
                            {
                                switch (evaluatedValue.CellType)
                                {
                                    case NPOI.SS.UserModel.CellType.String:
                                        description = evaluatedValue.StringValue.Trim();
                                        break;
                                    case NPOI.SS.UserModel.CellType.Numeric:
                                        description = evaluatedValue.NumberValue.ToString();
                                        break;
                                    default:
                                        description = evaluatedValue.FormatAsString().Trim('"');
                                        break;
                                }
                            }
                            break;
                        case NPOI.SS.UserModel.CellType.Boolean:
                            description = descCell.BooleanCellValue.ToString();
                            break;
                        default:
                            description = descCell.ToString().Trim();
                            break;
                    }
                }

                if (string.IsNullOrEmpty(title)) continue;

                Console.WriteLine($"Processing row {rowIndex} - Title: {title}");

                string savedFileName = null;

                if (picturesByRow.TryGetValue(rowIndex, out var picData))
                {
                    var hash = ComputeHash(picData.Data);
                    var fileName = $"{hash}.{picData.Extension}";
                    var filePath = Path.Combine(uploadFolder, fileName);

                    if (!System.IO.File.Exists(filePath))
                    {
                        await System.IO.File.WriteAllBytesAsync(filePath, picData.Data);
                    }

                    savedFileName = fileName;
                }

                if (string.IsNullOrEmpty(savedFileName))
                {
                    // Skip rows without image (or handle differently if image is optional)
                    continue;
                }

                // Check if report with same Title and ImageName already exists
                var existingReport = await _context.Reports
                    .FirstOrDefaultAsync(r => r.Title == title && r.ImageName == savedFileName);

                if (existingReport == null)
                {
                    var report = new Report
                    {
                        Title = title,
                        Description = description,
                        ImageName = savedFileName,
                        CreatedAt = DateTime.Now
                    };

                    _context.Reports.Add(report);
                }
                else
                {
                    Console.WriteLine($"Duplicate skipped: Title={title}, Image={savedFileName}");
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Reports imported successfully.";
            return RedirectToAction("Index");
        }

        private string ComputeHash(byte[] data)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(data);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
        #region CRUD
        // GET: /Report/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Report/Create
        [HttpPost]
        public async Task<IActionResult> Create(ReportVM model)
        {
            bool hasRequiredFieldErrors = false;

            // 1. Title required
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError("Title", "Title is required.");
                hasRequiredFieldErrors = true;
            }

            // 2. ImageFile required
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Image is required.");
                hasRequiredFieldErrors = true;
            }

            // 3. Title uniqueness
            //var duplicateTitle = await _context.Reports.FirstOrDefaultAsync(r => r.Title == model.Title);
            //if (duplicateTitle != null)
            //{
            //    ModelState.AddModelError("Title", "A report with this title already exists.");
            //    hasRequiredFieldErrors = true;
            //}

            if (hasRequiredFieldErrors)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? []
                );
                return BadRequest(new { success = false, message = "Please fill required fields", errors });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? []
                );
                return BadRequest(new { success = false, message = "Something went wrong", errors });
            }

            string fileName = null;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reports");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
            }

            var report = new Report
            {
                Title = model.Title,
                ImageName = fileName,
                Description = model.Description,
                CreatedAt = DateTime.Now
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Report created successfully." });
        }

        // POST: /Report/Update             // AGGRESSIVE WAY TO ALLOW imageFile to be empty
        [HttpPost]
        public async Task<IActionResult> Update(ReportVM model)
        {
            // Remove all validation errors related to ImageFile so image is optional during update
            if (ModelState.ContainsKey(nameof(model.ImageFile)))
            {
                ModelState[nameof(model.ImageFile)].Errors.Clear();
                ModelState[nameof(model.ImageFile)].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }

            // Check for Title uniqueness excluding current report
            //var duplicateTitle = await _context.Reports
            //    .Where(r => r.Id != model.Id && r.Title == model.Title)
            //    .FirstOrDefaultAsync();

            //if (duplicateTitle != null)
            //{
            //    ModelState.AddModelError("Title", "A report with this title already exists.");

            //    var validationErrors = ModelState.ToDictionary(
            //        kvp => kvp.Key,
            //        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>());

            //    return BadRequest(new { success = false, message = "Invalid Update.", errors = validationErrors });
            //}

            if (ModelState.IsValid)
            {
                var report = await _context.Reports.FindAsync(model.Id);
                if (report == null)
                    return NotFound(new { success = false, message = "Report not found." });

                report.Title = model.Title;
                report.Description = model.Description;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(report.ImageName))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reports", report.ImageName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reports");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    var newFilePath = Path.Combine(uploadsFolder, newFileName);

                    using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    report.ImageName = newFileName;
                }

                report.GenerateUpdatedAt();

                _context.Reports.Update(report);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Report updated successfully." });
            }

            // Return other validation errors
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>());

            return BadRequest(new { success = false, message = "Validation failed", errors });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return NotFound(new { success = false, message = "Report not found." });

            // Delete image file if exists
            if (!string.IsNullOrEmpty(report.ImageName))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reports", report.ImageName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Report deleted successfully." });
        }
        #endregion
        #region API CALLS
        // GET: /Report/GetAllReports (for DataTable ajax)
        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
            {
                return Unauthorized(); // to prevent the raw json from being seen 
            }

            var reports = await _context.Reports
                .Select(r => new {
                    r.Id,
                    r.Title,
                    r.ImageName,
                    r.Description
                }).ToListAsync();

            return Json(new { data = reports });
        }
        #endregion
    }


}
