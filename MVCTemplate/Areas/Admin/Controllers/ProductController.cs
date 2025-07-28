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
using Microsoft.Extensions.Caching.Memory;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}, {Roles.User}")]
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public ProductController(IUnitOfWork unitOfWork, ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _memoryCache = memoryCache;
        }

        #region EXPORT
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string token)
        {
            // Validate token existence and validity
            if (!TryValidateAndConsumeToken(token))
            {
                return Unauthorized();
            }

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
            await package.SaveAsAsync(stream);
            stream.Position = 0;

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Product.xlsx");
        }


        [HttpPost]
        public IActionResult GenerateDownloadToken()
        {
            var token = Guid.NewGuid().ToString();
            _memoryCache.Set(token, true, TimeSpan.FromMinutes(5)); // Cache it for 5 minutes
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
        #endregion
        #region CRUD
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            try
            {
                bool hasRequiredFieldErrors = false;

                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    ModelState.AddModelError("Name", "Product Name is required.");
                    hasRequiredFieldErrors = true;
                }

                if (product.Quantity <= 0)
                {
                    ModelState.AddModelError("Quantity", "Quantity value is invalid.");
                    hasRequiredFieldErrors = true;
                }

                if (_unitOfWork.Product.CheckIfUnique(product.Name) != null)
                {
                    ModelState.AddModelError("Name", "Product already exists.");
                    hasRequiredFieldErrors = true;
                }

                if (hasRequiredFieldErrors)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray() ?? []
                    );

                    return BadRequest(new { message = "Invalid Operation", errors });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray() ?? []
                    );

                    return BadRequest(new { errors, message = "Validation failed" });
                }

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

        [HttpPut]
        public IActionResult Update(Product obj)
        {
            try
            {
                obj.GenerateUpdatedAt();
                var product = _unitOfWork.Product.ContinueIfNoChangeOnUpdate(obj.Name, obj.Id);

                if (product != null)
                {
                    ModelState.AddModelError("Name", "Product name already exists");

                    var validationErrors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>());

                    return BadRequest(new { errors = validationErrors, message = "Invalid Update" });
                }

                if (ModelState.IsValid)
                {
                    _unitOfWork.Product.Update(obj);
                    _unitOfWork.Save();
                    return Ok(new { message = "Updated Successfully" });
                }

                var otherErrors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>());

                return BadRequest(new { errors = otherErrors, message = "Something went wrong!" });
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

        [Authorize(Roles = "Admin")]
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

        private List<Product> GetProducts()
        {
            return _unitOfWork.Product.ToList();
        }
        #endregion

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

        [HttpPost]
        public IActionResult GetProductsData()
        {
            var names = _context.Products.Select(p => p.Name).ToList();
            var quantities = _context.Products.Select(p => p.Quantity).ToList();

            return Json(new List<object> { names, quantities });
        }
        #endregion
    }
}
