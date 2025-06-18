using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCTemplate.Util;
using MVCTemplate.DataAccess.Repository.IRepository;
using MVCTemplate.Models;
using MVCtemplate.DataAccess.Data;
using ClosedXML.Excel;
using System.IO;
using OfficeOpenXml.Style;
using OfficeOpenXml;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}, {Roles.User}")]
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public ProductController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context; // ✅ Fixed: assign _context
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            var dataToExport = _unitOfWork.Product.GetAll().ToList();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            var blueBackground = System.Drawing.Color.FromArgb(0, 51, 102);
            var whiteFont = System.Drawing.Color.White;
            var lightGreen = System.Drawing.Color.FromArgb(198, 239, 206);
            var darkGreen = System.Drawing.Color.FromArgb(155, 187, 89);
            var borderStyle = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            string FormatDateTimeWordMDY(DateTime dt) =>
                dt.ToString("MMMM dd, yyyy, hh:mm tt");

            worksheet.Cells[1, 1].Value = "Product Data";
            worksheet.Cells[1, 1, 1, 4].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            var now = DateTime.Now;
            worksheet.Cells[2, 1].Value = $"Generated at: {FormatDateTimeWordMDY(now)}";
            worksheet.Cells[2, 1, 2, 4].Merge = true;
            worksheet.Cells[2, 1].Style.Font.Italic = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            worksheet.Cells["A1:D2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1:D2"].Style.Fill.BackgroundColor.SetColor(blueBackground);
            worksheet.Cells["A1:D2"].Style.Font.Color.SetColor(whiteFont);
            worksheet.Cells["A1:D2"].Style.Border.BorderAround(borderStyle);

            string[] headers = { "ID", "Name", "Description", "Quantity" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[3, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                var fillColor = ((i + 1) % 2 == 0) ? lightGreen : darkGreen;
                cell.Style.Fill.BackgroundColor.SetColor(fillColor);
                cell.Style.Border.BorderAround(borderStyle);
            }

            int row = 4;
            foreach (var item in dataToExport)
            {
                worksheet.Cells[row, 1].Value = item.Id;
                worksheet.Cells[row, 2].Value = item.Name;
                worksheet.Cells[row, 3].Value = item.Description;
                worksheet.Cells[row, 4].Value = item.Quantity;

                for (int col = 1; col <= 4; col++)
                {
                    var cell = worksheet.Cells[row, col];
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    var fillColor = (col % 2 == 0) ? lightGreen : darkGreen;
                    cell.Style.Fill.BackgroundColor.SetColor(fillColor);
                    cell.Style.Border.BorderAround(borderStyle);
                }
                row++;
            }

            // Apply auto-filter per column on header row (row 3)
            worksheet.Cells[3, 1, row - 1, 4].AutoFilter = true;

            worksheet.Column(1).Width = 10;

            int GetMaxLength(int colIndex)
            {
                int maxLen = headers[colIndex - 1].Length;
                for (int r = 4; r < row; r++)
                {
                    var val = worksheet.Cells[r, colIndex].Text;
                    if (!string.IsNullOrEmpty(val))
                        maxLen = Math.Max(maxLen, val.Length);
                }
                return maxLen + 2;
            }

            worksheet.Column(2).Width = GetMaxLength(2);
            worksheet.Column(3).Width = GetMaxLength(3);
            worksheet.Column(4).Width = 10;

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Product.xlsx");
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            try
            {
                // Check for empty Name
                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    return BadRequest(new { field = "Name", message = "Product Name is required." });
                }

                // Check for zero or negative quantity
                if (product.Quantity <= 0)
                {
                    return BadRequest(new { field = "Quantity", message = "Quantity value is invalid." });
                }

                // Check for uniqueness
                var productCheck = _unitOfWork.Product.CheckIfUnique(product.Name);
                if (productCheck != null)
                {
                    return BadRequest(new { field = "Name", message = "Product already exists." });
                }

                // Return any other validation errors
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage)?.ToArray() ?? []
                    );

                    return BadRequest(new { errors, message = "Validation failed" });
                }

                // Save product
                _unitOfWork.Product.Add(product);
                _unitOfWork.Save();

                return Ok(new { message = "Added Successfully" });
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


        private List<Product> GetProducts()
        {
            return _unitOfWork.Product.ToList();
        }

        /*public async Task<ActionResult> ExportToExcel()
        {
            var products = GetProducts();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.AddWorksheet("Sheet 1");

                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Description";
                worksheet.Cell(1, 4).Value = "Quantity";

                int row = 2;
                foreach (var item in products)
                {
                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.Name;
                    worksheet.Cell(row, 3).Value = item.Description;
                    worksheet.Cell(row, 4).Value = item.Quantity;
                    row++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    Response.Headers.Add("Content-Disposition", "attachment; filename=ProductsExport.xlsx");
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    await memoryStream.CopyToAsync(Response.Body);
                    return new EmptyResult();
                }
            }
        }*/

        [HttpPost]
        public IActionResult GetProductsData()
        {
            var names = _context.Products.Select(p => p.Name).ToList();
            var quantities = _context.Products.Select(p => p.Quantity).ToList();

            return Json(new List<object> { names, quantities });
        }

        [HttpPut]
        public IActionResult Update(Product obj)
        {
            try
            {
                obj.GenerateUpdatedAt();
                var product = _unitOfWork.Product.ContinueIfNoChangeOnUpdate(obj.Name, obj.Id);

                if (product != null)
                {
                    return BadRequest(new { field = "Name", message = "Product name already exists" });
                }

                if (ModelState.IsValid)
                {
                    _unitOfWork.Product.Update(obj);
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
                    return BadRequest(new { message = "Product Id not found" });
                }

                var product = _unitOfWork.Product.Get(u => u.Id == id);
                if (product == null)
                {
                    return BadRequest(new { message = "Product Id not found" });
                }

                _unitOfWork.Product.Remove(product);
                _unitOfWork.Save();
                return Ok(new { message = "Product deleted successfully" });
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

        #region API Calls

        [HttpGet]
        public IActionResult GetAllProducts()
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
            {
                return Unauthorized(); // to prevent the raw json from being seen 
            }

            var productList = _unitOfWork.Product.GetAll().ToList();
            return Json(new { data = productList });
        }

        #endregion
    }
}
