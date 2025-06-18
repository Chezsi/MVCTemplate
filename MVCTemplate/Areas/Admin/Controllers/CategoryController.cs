using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCTemplate.DataAccess.Repository.IRepository;
using MVCTemplate.Models;
using MVCTemplate.Util;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Diagnostics;
using MVCtemplate.DataAccess.Data;
using MVCTemplate.DataAccess.Repository;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}")]
    [Area("Admin")]
    public class CategoryController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        private IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            var dataToExport = _unitOfWork.Category.GetAll().ToList();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Categories");

            // Colors
            var blueBackground = System.Drawing.Color.FromArgb(0, 51, 102);
            var whiteFont = System.Drawing.Color.White;
            var lightGreen = System.Drawing.Color.FromArgb(198, 239, 206);
            var darkGreen = System.Drawing.Color.FromArgb(155, 187, 89);
            var borderStyle = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            // Row 1: Title
            worksheet.Cells[1, 1].Value = "Category Data";
            worksheet.Cells[1, 1, 1, 3].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Row 2: Generated At
            var now = DateTime.Now;
            worksheet.Cells[2, 1].Value = $"Generated at: {now:MMMM dd, yyyy, hh:mm tt}";
            worksheet.Cells[2, 1, 2, 3].Merge = true;
            worksheet.Cells[2, 1].Style.Font.Italic = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Style Title and Timestamp rows
            worksheet.Cells["A1:C2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A1:C2"].Style.Fill.BackgroundColor.SetColor(blueBackground);
            worksheet.Cells["A1:C2"].Style.Font.Color.SetColor(whiteFont);
            worksheet.Cells["A1:C3"].Style.Border.Top.Style = borderStyle;
            worksheet.Cells["A1:C3"].Style.Border.Bottom.Style = borderStyle;
            worksheet.Cells["A1:C3"].Style.Border.Left.Style = borderStyle;
            worksheet.Cells["A1:C3"].Style.Border.Right.Style = borderStyle;

            // Row 3: Headers
            string[] headers = { "ID", "Name", "Code" };
            for (int i = 0; i < headers.Length; i++)
            {
                int col = i + 1;
                var cell = worksheet.Cells[3, col];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                var fillColor = (col % 2 == 0) ? lightGreen : darkGreen;
                cell.Style.Fill.BackgroundColor.SetColor(fillColor);
                cell.Style.Border.Top.Style = borderStyle;
                cell.Style.Border.Bottom.Style = borderStyle;
                cell.Style.Border.Left.Style = borderStyle;
                cell.Style.Border.Right.Style = borderStyle;
            }

            // Rows 4+: Data rows
            int row = 4;
            foreach (var item in dataToExport)
            {
                worksheet.Cells[row, 1].Value = item.IdCategory;
                worksheet.Cells[row, 2].Value = item.NameCategory;
                worksheet.Cells[row, 3].Value = item.CodeCategory;

                for (int col = 1; col <= 3; col++)
                {
                    var cell = worksheet.Cells[row, col];
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    var fillColor = (col % 2 == 0) ? lightGreen : darkGreen;
                    cell.Style.Fill.BackgroundColor.SetColor(fillColor);
                    cell.Style.Border.Top.Style = borderStyle;
                    cell.Style.Border.Bottom.Style = borderStyle;
                    cell.Style.Border.Left.Style = borderStyle;
                    cell.Style.Border.Right.Style = borderStyle;
                }

                row++;
            }

            // Apply autofilter on the header row (Row 3)
            worksheet.Cells[3, 1, row - 1, 3].AutoFilter = true;

            // Merge title and timestamp rows across columns A:C
            worksheet.Cells[1, 1, 1, 3].Merge = true;
            worksheet.Cells[2, 1, 2, 3].Merge = true;

            // Auto fit columns, with dynamic width for Name and Code columns
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
                return maxLen + 2; // padding
            }

            worksheet.Column(2).Width = GetMaxLength(2);
            worksheet.Column(3).Width = GetMaxLength(3);

            // Save to stream and return Excel file
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Category.xlsx");
        }

        public IActionResult Create(Category category)
        {
            try
            {
                bool hasRequiredFieldErrors = false;

                if (string.IsNullOrWhiteSpace(category.CodeCategory))
                {
                    ModelState.AddModelError("CodeCategory", "Category code is required.");
                    hasRequiredFieldErrors = true;
                }

                if (string.IsNullOrWhiteSpace(category.NameCategory))
                {
                    ModelState.AddModelError("NameCategory", "Category name is required.");
                    hasRequiredFieldErrors = true;
                }

                Models.Category? categoryCheck = _unitOfWork.Category.CheckIfUnique(category.NameCategory);
                if (categoryCheck != null)
                {
                    ModelState.AddModelError("NameCategory", "Category already exists.");
                    hasRequiredFieldErrors = true;
                }

                if (hasRequiredFieldErrors)
                {
                    var errors = ModelState.ToDictionary(
                         kvp => kvp.Key,
                         kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray() ?? []
                    );
                    return BadRequest(new { message = "Please Fill Required Fields", errors });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                         kvp => kvp.Key,
                         kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray() ?? []
                    );
                    return BadRequest(new { errors, message = "Something went wrong" });
                }

                _unitOfWork.Category.Add(category);
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
        public IActionResult Update(Category obj)
        {
            try
            {
                obj.GenerateUpdatedAt();
                Category? category = _unitOfWork.Category.ContinueIfNoChangeOnUpdate(obj.NameCategory, obj.IdCategory);

                if (category != null)
                {
                    return BadRequest(new { field = "NameCategory", message = "Category Name already exists" });
                }
                if (ModelState.IsValid)
                {
                    _unitOfWork.Category.Update(obj);
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
                    return BadRequest(new { message = "Category Id not found" });
                }

                Category category = _unitOfWork.Category.Get(u => u.IdCategory == id);
                if (category == null)
                {
                    return BadRequest(new { message = "Category Id not found" });
                }

                _unitOfWork.Category.Remove(category);
                _unitOfWork.Save();
                return Ok(new { message = "Category deleted successfully" });
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

        public IActionResult GetAllCategory()
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
            {
                return Unauthorized(); // to prevent the raw json from being seen 
            }

            List<Category>? categoryList = _unitOfWork.Category.GetAll().ToList();
            return Json(new { data = categoryList });
        }
       
        #endregion

    }
}
