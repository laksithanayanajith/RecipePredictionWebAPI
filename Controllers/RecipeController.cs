using CsvHelper;
using System;
using OpenAI_API;
using Microsoft.AspNetCore.Mvc;
using RecipePredictionWebAPI.Models;
using RecipePredictionWebAPI.Services;
using System.Formats.Asn1;
using System.Globalization;
using OfficeOpenXml;
using Microsoft.CodeAnalysis.Text;
using System.Net;
using OpenAI_API.Completions;
using Microsoft.AspNetCore.Cors;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RecipePredictionWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        #region dependancyinjection
        private readonly RecipeService _recipeService;
        private readonly string line = string.Empty;
        private string respones = string.Empty;
        private CompletionRequest completionRequest;
        private FileInfo file;

        public RecipeController(RecipeService recipeService)
        {
            _recipeService = recipeService;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            file = new FileInfo("./Data/recipedata.xlsx");
        }

        #endregion
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

        #region data
        [HttpGet("Date")]
        public async Task<IActionResult> GetData(string input)
        {
            #region connection
            OpenAIAPI oapi = new OpenAIAPI(System.IO.File.ReadAllText("./Data/note.txt"));
            completionRequest = new CompletionRequest();
            completionRequest.Model = "text-davinci-003";
            completionRequest.MaxTokens = 4000;
            completionRequest.Prompt = input;
            #endregion
            var output = await oapi.Completions.CreateCompletionAsync(completionRequest);

            if (output is null)
            {
                return NotFound();
            }

            foreach (var item in output.Completions)
            {
                respones = item.Text;
            }

            return Ok(respones);
        }
        #endregion
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
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("Recipe");

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
