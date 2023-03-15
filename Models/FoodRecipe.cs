using Microsoft.ML.Data;

namespace RecipePredictionWebAPI.Models
{
    public class FoodRecipe
    {
        [LoadColumn(0)]
        public string Ingredients { get; set; } = string.Empty;

        [LoadColumn(1)]
        public string Recipe { get; set; } = string.Empty;
    }
}
