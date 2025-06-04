using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MVCtemplate.DataAccess.Data;
using MVCTemplate.DataAccess.Repository.IRepository;
using MVCTemplate.Models;
using MVCTemplate.Util;
using MVCTemplate.ViewModels;
using System.Diagnostics;

namespace MVCTemplate.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{Roles.Admin}")]
    [Area("Admin")]
    public class PersonController : Controller
    {
        public IActionResult Index()
        {
            PersonVM personVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.NameCategory,
                    Value = u.IdCategory.ToString()
                }),
                Person = new Person()
            };
            return View(personVM);
        }

        private IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public PersonController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Update()
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category
               .GetAll().Select(u => new SelectListItem
               {
                   Text = u.NameCategory,
                   Value = u.IdCategory.ToString()
               });

            ViewBag.CategoryList = CategoryList;
            //not working
            return View();
        }

        [HttpPost]
        public IActionResult GetPersonsData()
        {
            // Fetch categories where there is at least one person linked by CategoryId
            var data = _context.Categorys
                .Where(c => _context.Persons.Any(p => p.CategoryId == c.IdCategory))
                .Select(c => new
                {
                    CategoryName = c.NameCategory, // Changed here
                    EmployeeCount = _context.Persons.Count(p => p.CategoryId == c.IdCategory) // Changed here
                })
                .ToList();

            var labels = data.Select(d => d.CategoryName).ToList();
            var counts = data.Select(d => d.EmployeeCount).ToList();

            return Json(new object[] { labels, counts });
        }



        [HttpPost]
        public IActionResult Create(Person person)
        {
            try
            {
                Models.Person? personCheck = _unitOfWork.Person.CheckIfUnique(person.Name);
                if (personCheck != null)
                {
                    ModelState.AddModelError("Name", "Category already exists");
                }

                if (ModelState.IsValid)
                {
                    _unitOfWork.Person.Add(person);
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
            catch (Exception) //exception to personrepository

            {
                return BadRequest(new { message = "An unexpected error occurred" });
            }
        }
        [HttpPut]
        public IActionResult Update(Person obj)
        {
            try
            {
                obj.GenerateUpdatedAt();
                Person? person = _unitOfWork.Person.ContinueIfNoChangeOnUpdate(obj.Name, obj.Id);

                if (person != null)
                {
                    ModelState.AddModelError("Name", "Person Name Already exists");
                }
                if (ModelState.IsValid)
                {
                    _unitOfWork.Person.Update(obj);
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
                    return BadRequest(new { message = "Person Id not found" });
                }

                Person person = _unitOfWork.Person.Get(u => u.Id == id);
                if (person == null)
                {
                    return BadRequest(new { message = "Person Id not found" });
                }

                _unitOfWork.Person.Remove(person);
                //_unitOfWork.Save();
                return Ok(new { message = "Person deleted successfully" });
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547) // DELETE statement conflicted with the reference constraint \"FK-Contracts_Persons_PersonId\"
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

        public IActionResult GetAllPersons()
        {

            List<Person>? personList = _unitOfWork.Person.GetAll().ToList();
            return Json(new { data = personList });
        }

        [HttpGet] // for exporting data from two tables
        public IActionResult ExportPersonsWithCurrentContracts()
        {
            var now = DateTime.Now;

            var data = from person in _context.Persons
                       join contract in _context.Contracts on person.Id equals contract.PersonId
                       where contract.Validity > now
                       select new
                       {
                           Person_Id = person.Id,
                           Person_Name = person.Name,
                           Person_Position = person.Position,
                           Person_CategoryId = person.CategoryId,

                           Contract_Id = contract.Id,
                           Contract_Name = contract.Name,
                           Contract_Description = contract.Description,
                           Contract_Validity = contract.Validity
                       };

            var list = data.ToList();

            return Json(list);
        }
        #endregion

    }
}
