using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCTemplate.Util;
using MVCTemplate.DataAccess.Repository.IRepository;
using MVCTemplate.Models;
using MVCtemplate.DataAccess.Data;
using ClosedXML.Excel;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCTemplate.ViewModels;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}, {Roles.User}")]
    [Area("Admin")]
    public class ContractController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration; // for the passkey of contract
        private readonly IMemoryCache _memoryCache;

        public ContractController(IUnitOfWork unitOfWork, ApplicationDbContext context, IConfiguration configuration, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _configuration = configuration; // the passkey is in appsettings.json
            _memoryCache = memoryCache;
        }

        #region CRUD
        public IActionResult Index()
        {
            //var persons = _unitOfWork.Person.GetAll();

            var excludedPersonIds = _unitOfWork.Contract.GetAll() // for the create
            .Where(c => c.Validity > DateTime.Now)
            .Select(c => c.PersonId)
            .Distinct()
            .ToList();

            var persons = _unitOfWork.Person.GetAll()
                .Where(p => !excludedPersonIds.Contains(p.Id))
                .ToList();

            var viewModel = new ContractVM
            {
                Contract = new Contract(),
                PersonList = persons.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
            };

            return View(viewModel);
        }

        [HttpGet] // for edit
        public IActionResult GetPersonNameById(int id)
        {
            var person = _unitOfWork.Person.GetFirstOrDefault(p => p.Id == id);
            if (person == null)
                return NotFound();

            return Ok(new { name = person.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContractVM model)
        {
            try
            {
                bool hasRequiredFieldErrors = false;

                // Check required fields explicitly
                if (string.IsNullOrWhiteSpace(model.Contract.Name))
                {
                    ModelState.AddModelError("Contract.Name", "Name is required.");
                    hasRequiredFieldErrors = true;
                }

                if (string.IsNullOrWhiteSpace(model.Contract.Description))
                {
                    ModelState.AddModelError("Contract.Description", "Description is required.");
                    hasRequiredFieldErrors = true;
                }

                if (model.Contract.Validity == default || model.Contract.Validity <= DateTime.MinValue)
                {
                    ModelState.AddModelError("Contract.Validity", "Validity date is required.");
                    hasRequiredFieldErrors = true;
                }

                if (model.Contract.PersonId <= 0)
                {
                    ModelState.AddModelError("Contract.PersonId", "PersonId is required.");
                    hasRequiredFieldErrors = true;
                }

                // If any required fields failed, return immediately with generic message + errors
                if (hasRequiredFieldErrors)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? []
                    );

                    return BadRequest(new { success = false, message = "Please Fill Required Fields", errors });
                }

                // Existing checks
                if (!_unitOfWork.Person.Exists(model.Contract.PersonId))
                {
                    ModelState.AddModelError("Contract.PersonId", "Selected person does not exist.");
                }

                var existingContract = _unitOfWork.Contract.CheckIfUnique(model.Contract.Name);
                if (existingContract != null)
                {
                    ModelState.AddModelError("Contract.Name", "A contract with this name already exists.");
                }

                var hasFutureContract = _unitOfWork.Contract.GetFirstOrDefault(c =>
                    c.PersonId == model.Contract.PersonId && c.Validity > DateTime.Now);

                if (hasFutureContract != null)
                {
                    ModelState.AddModelError("Contract.PersonId", "This person already has a future-valid contract.");
                }

                // ModelState invalid check after those
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? []
                    );

                    return BadRequest(new { success = false, message = "Validation failed", errors });
                }

                // Save if all valid
                model.Contract.CreatedAt = DateTime.Now;
                _unitOfWork.Contract.Add(model.Contract);
                _unitOfWork.Save();

                return Ok(new { success = true, message = "Contract added successfully" });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { success = false, message = "Database error occurred while saving the contract." });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { success = false, message = "Invalid operation attempted." });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "An unexpected error occurred." });
            }
        }

        private List<Contract> GetContracts()
        {
            return _unitOfWork.Contract.ToList();
        }
        
        [HttpPost]
        public IActionResult Update(ContractVM vm)
        {
            var obj = vm.Contract;

            try
            {
                var existing = _unitOfWork.Contract.GetFirstOrDefault(c => c.Id == obj.Id);
                if (existing == null)
                {
                    return NotFound(new { message = "Contract not found." });
                }

                // Add errors with your original keys
                if (obj.Validity.HasValue && obj.Validity.Value.Date < DateTime.Now.Date)
                {
                    ModelState.AddModelError("Contract.Validity", "New validity date cannot be in the past.");
                }

                var duplicateName = _unitOfWork.Contract.ContinueIfNoChangeOnUpdate(obj.Name, obj.Id);
                if (duplicateName != null)
                {
                    ModelState.AddModelError("Contract.Name", "Contract Name already exists");
                }

                // Instead of ContainsKey, safely check if those keys exist AND have errors:
                bool hasValidityErrors = ModelState.TryGetValue("Contract.Validity", out var validityEntry) && validityEntry.Errors.Count > 0;
                bool hasNameErrors = ModelState.TryGetValue("Contract.Name", out var nameEntry) && nameEntry.Errors.Count > 0;

                if (hasValidityErrors || hasNameErrors)
                {
                    var errors = ModelState
                        .Where(kvp => kvp.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return BadRequest(new { errors, message = "Invalid Update" });
                }

                // Also check general ModelState validity for any other errors
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(kvp => kvp.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return BadRequest(new { errors, message = "Something went wrong!" });
                }

                // Update entity only after validation passed
                existing.Name = obj.Name;
                existing.Description = obj.Description;
                existing.Validity = obj.Validity;
                existing.GenerateUpdatedAt();

                _unitOfWork.Save();

                return Ok(new { message = "Updated Successfully" });
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

        [HttpPost]
        [Route("Admin/Contract/Unlock/{id}")]
        public IActionResult Unlock(int id, [FromForm] string key)
        {
            var correctKey = _configuration["ContractUnlockKey"]; // otherwise it will be hardcoded here

            if (string.IsNullOrEmpty(key) || key != correctKey)
            {
                return BadRequest(new { message = "Invalid unlock key." });
            }

            var contract = _unitOfWork.Contract.GetFirstOrDefault(c => c.Id == id);
            if (contract == null)
            {
                return NotFound(new { message = "Contract not found." });
            }

            // Extend validity to X amount of days
            contract.Validity = DateTime.Now.Date.AddDays(1);

            _unitOfWork.Contract.Update(contract);
            _unitOfWork.Save();

            return Ok(new { message = "Contract unlocked successfully." });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            if (id == 0)
            {
                return BadRequest(new { message = "Contract Id not found" });
            }

            // Reload the entity fresh from DB (no tracking)
            var contract = _unitOfWork.Contract.GetNoTracking(u => u.Id == id);

            if (contract == null)
            {
                // Entity already deleted (or never existed), so return success or not found
                return NotFound(new { message = "Contract already deleted or does not exist." });
            }

            try
            {
                // Attach the entity to context if needed before remove
                _unitOfWork.Contract.Attach(contract);
                _unitOfWork.Contract.Remove(contract);
                _unitOfWork.Save();

                return Ok(new { message = "Contract deleted successfully" });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Concurrency exception means entity no longer exists - treat as successful delete
                return Ok(new { message = "Contract already deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Unexpected error: {ex.Message}" });
            }
        }
        #endregion

        #region Export
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

        // not being used
        public async Task<ActionResult> ExportToExcel(string token)
        {
            if (!TryValidateAndConsumeToken(token))
            {
                return Unauthorized();
            }

            var contracts = GetContracts();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.AddWorksheet("Sheet 1");

                // Header row
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Description";

                int row = 2;
                foreach (var item in contracts)
                {
                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.Name;
                    worksheet.Cell(row, 3).Value = item.Validity;
                    row++;
                }

                // Apply auto-filter on the entire data range including headers
                var lastRow = row - 1;  // last row with data
                worksheet.Range(1, 1, lastRow, 3).SetAutoFilter();

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    Response.Headers.Add("Content-Disposition", "attachment; filename=ContractsExport.xlsx");
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    await memoryStream.CopyToAsync(Response.Body);
                    return new EmptyResult();
                }
            }
        }
        #endregion

        #region API Calls

        [HttpGet]
        public IActionResult GetAllContracts()
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
            {
                return Unauthorized(); // to prevent the raw json from being seen 
            }

            var contractList = _unitOfWork.Contract
                .GetAll(includeProperties: "Person") // include Person navigation property
                .Select(c => new {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.Validity,
                    c.PersonId,
                    PersonName = c.Person != null ? c.Person.Name : "(No Person)"
                })
                .ToList();

            return Json(new { data = contractList });
        }

        [HttpGet]
        public IActionResult GetAllPersonsForContract()
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
            {
                return Unauthorized(); // to prevent the raw json from being seen 
            }

            var persons = _unitOfWork.Person.GetAll()
                .Select(p => new {
                    p.Id,
                    p.Name
                })
                .ToList();

            return Json(persons);
        }

        // for donut and pie
        [HttpPost]
        [Route("/Admin/Contract/GetContractsPerMonth")]
        public IActionResult GetContractsPerMonth()
        {
            var currentYear = DateTime.Now.Year;

            var monthlyData = _context.Contracts
                .Where(c => c.Validity.HasValue && c.Validity.Value.Year == currentYear)
                .GroupBy(c => c.Validity.Value.Month)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key),
                    Count = g.Count()
                }).ToList();

            var labels = monthlyData.Select(d => d.Month).ToList();
            var values = monthlyData.Select(d => d.Count).ToList();

            return Json(new List<object> { labels, values });
        }

        // for bar and line
        [HttpPost]
        public IActionResult GetContractsPerYear()
        {
            var contractCounts = _context.Contracts
                .Where(c => c.Validity.HasValue)
                .GroupBy(c => c.Validity.Value.Year)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Year = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToList();

            var years = contractCounts.Select(x => x.Year).ToList();
            var counts = contractCounts.Select(x => x.Count).ToList();

            return Json(new List<object> { years, counts });
        }


        #endregion
    }
}
