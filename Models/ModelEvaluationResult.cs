using Microsoft.ML.Data;
using Microsoft.ML;

namespace RecipePredictionWebAPI.Models
{
    public class ModelEvaluationResult
    {
        public ITransformer? Model { get; set; }
        public MulticlassClassificationMetrics? Metrics { get; set; }
    }
}
