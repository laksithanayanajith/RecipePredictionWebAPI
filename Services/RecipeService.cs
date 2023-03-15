using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Data;
using RecipePredictionWebAPI.Models;

namespace RecipePredictionWebAPI.Services
{
    public class RecipeService
    {
        private readonly MLContext _mlContext;
        public RecipeService()
        {
            _mlContext = new MLContext();
        }

        public ITransformer TrainModel()
        {
            // Load data
            var data = _mlContext.Data.LoadFromTextFile<FoodRecipe>("./Data/recipedata.csv", separatorChar: ',', hasHeader: true);

            // Split data
            var trainTestSplit = _mlContext.Data.TrainTestSplit(data, testFraction: 0.3);

            // Define pipeline
            var pipeline = _mlContext.Transforms
                .Text.FeaturizeText("Features", nameof(FoodRecipe.Ingredients))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey(nameof(FoodRecipe.Recipe)))
                .Append(_mlContext.Transforms
                    .Concatenate("Features", "Features")
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features")))
                .Append(_mlContext.Transforms.Conversion
                    .MapKeyToValue(nameof(FoodRecipePrediction.Recipe), nameof(FoodRecipePrediction.Recipe)))
                .Append(_mlContext.MulticlassClassification
                    .Trainers.SdcaNonCalibrated());


            // Train model
            var model = pipeline.Fit(trainTestSplit.TrainSet);

            // Evaluate model
            var metrics = _mlContext.MulticlassClassification.Evaluate(model.Transform(trainTestSplit.TestSet));

            Console.WriteLine($"Accuracy: {metrics.MacroAccuracy}");
            Console.WriteLine($"Log-loss: {metrics.LogLoss}");

            ModelEvaluationResult modelEvaluationResult = new ModelEvaluationResult { Model = model, Metrics = metrics };
            return (ITransformer)modelEvaluationResult;

            //return model;
        }

        public string GetRecipe(string ingredients)
        {
            // Load trained model
            var model = _mlContext.Model.Load("./MLModel.zip", out var schema);

            // Create prediction engine
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<FoodRecipe, FoodRecipePrediction>(model);

            // Predict recipe
            var recipePrediction = predictionEngine.Predict(new FoodRecipe { Ingredients = ingredients });

            return recipePrediction.Recipe;
        }

    }
}
