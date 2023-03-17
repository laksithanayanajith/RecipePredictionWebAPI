using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Data;
using RecipePredictionWebAPI.Models;
using Tensorflow.Contexts;
using static Microsoft.ML.DataOperationsCatalog;

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
            var data = _mlContext.Data.LoadFromTextFile<FoodRecipe>("./Data/recipedata.xlsx", separatorChar: ',', hasHeader: true);

            // Split data
            var trainTestSplit = _mlContext.Data.TrainTestSplit(data, testFraction: 0.3);

           
            //Create pipeline
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Ingredients", "Features")
    .Append(_mlContext.Transforms.CopyColumns("Recipe", "Label"))
    .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label"))
    .Append(_mlContext.Transforms.Text.FeaturizeText("Recipe", "RecipeFeatures"))
    .Append(_mlContext.Transforms.Concatenate("Features", "RecipeFeatures"))
    .Append(_mlContext.Transforms.NormalizeMinMax("Features", "Features"));

            // Train model
            var model = pipeline.Fit(trainTestSplit.TrainSet);

            // Evaluate model
            var metrics = _mlContext.MulticlassClassification.Evaluate(model.Transform(trainTestSplit.TestSet));

            Console.WriteLine($"Accuracy: {metrics.MacroAccuracy}");
            Console.WriteLine($"Log-loss: {metrics.LogLoss}");

            ModelEvaluationResult modelEvaluationResult = new ModelEvaluationResult { Model = model, Metrics = metrics };
            Console.WriteLine($"modelEvaluationResult: {modelEvaluationResult}");

            _mlContext.Model.Save(model, trainTestSplit.TrainSet.Schema, "./Models/MLModel.zip");


            return model;
        }

        public string GetRecipe(string ingredients)
        {
            // Load trained model
            var model = _mlContext.Model.Load("./Models/MLModel.zip", out var schema);

            // Create prediction engine
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<FoodRecipe, FoodRecipePrediction>(model);

            // Predict recipe
            var recipePrediction = predictionEngine.Predict(new FoodRecipe { Ingredients = ingredients });

            return recipePrediction.Recipe;
        }

    }
}
