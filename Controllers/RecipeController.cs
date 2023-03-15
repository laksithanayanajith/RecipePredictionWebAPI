using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using RecipePredictionWebAPI.Models;
using RecipePredictionWebAPI.Services;
using System.Formats.Asn1;
using System.Globalization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RecipePredictionWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly RecipeService _recipeService;
        public RecipeController(RecipeService recipeService)
        {
            _recipeService = recipeService;
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
        public IActionResult AddRecipe(FoodRecipe recipe)
        {
            try
            {
                // Append the new recipe to the CSV file
                using (var streamWriter = new StreamWriter("./Data/recipedata.csv", true))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    if (recipe is not null)
                    {
                        csvWriter.WriteRecord(recipe);
                    }
                    else
                    {
                        return BadRequest("Recipe is empty!");
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
