using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using RecipePredictionWebAPI.Models;
using RecipePredictionWebAPI.Services;
using System.Formats.Asn1;
using System.Globalization;
using OfficeOpenXml;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RecipePredictionWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly RecipeService _recipeService;
        private FileInfo file;

        public RecipeController(RecipeService recipeService)
        {
            _recipeService = recipeService;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            file = new FileInfo("./Data/recipedata.xlsx");
        }
        // GET: api/<RecipeController>
        [HttpGet("{ingredients}")]
        public IActionResult GetRecipe(string ingredients)
        {
            // Predict recipe
            var recipe = _recipeService.GetRecipe(ingredients);

            if (recipe is null)
            {
                return NotFound();
            }

            return Ok(recipe);
        }

        // POST api/<RecipeController>
        [HttpPost]
        public async Task<IActionResult> AddRecipe(FoodRecipe recipe)
        {
            try
            {
                // Append the new recipe to the CSV file
                if ((recipe is not null) && (file is not null))
                {
                    await SaveExcelFile(recipe, file);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return Ok();
        }

        private async Task SaveExcelFile(FoodRecipe recipe, FileInfo file)
        {
            List<FoodRecipe> recipelist = new List<FoodRecipe> { recipe };

            using (var package = new ExcelPackage(file))
            {
                try
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("MainReport");

                    ExcelRangeBase range = ws.Cells["A1"].LoadFromCollection(recipelist, true);
                    range.AutoFitColumns();

                    await package.SaveAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
