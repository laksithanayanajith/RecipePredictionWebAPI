using Microsoft.ML.Data;

namespace RecipePredictionWebAPI.Models
{
    public class FoodRecipePrediction
    {
        [ColumnName("PredictedLabel")]
        public string Recipe { get; set; } = string.Empty;
    }
}
