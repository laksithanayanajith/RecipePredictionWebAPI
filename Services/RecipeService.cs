using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Data;
using RecipePredictionWebAPI.Models;
using Tensorflow.Contexts;
using static Microsoft.ML.DataOperationsCatalog;
using SkiaSharp;
using Microsoft.ML.Tokenizers;

namespace RecipePredictionWebAPI.Services
{
    public class RecipeService
    {
        private readonly ILogger<RecipeService> logger;
        MLContext _mlContext;

        public RecipeService()
        {
            _mlContext = new MLContext();
        }

        public RecipeService(ILogger<RecipeService> logger) : this()
        {
            this.logger = logger;
        }

        public ITransformer TrainModel()
        {
            // Load data
            var data = _mlContext.Data.LoadFromTextFile<FoodRecipe>("./Data/recipedata.xlsx", separatorChar: ',', hasHeader: true);

            // Split data
            var trainTestSplit = _mlContext.Data.TrainTestSplit(data, testFraction: 0.3);


            //Create pipeline
            var pipeline = _mlContext.Transforms.Text.FeaturizeText(nameof(FoodRecipe.Ingredients), "IngredientsFeatures")
    .Append(_mlContext.Transforms.Text.FeaturizeText(nameof(FoodRecipe.Recipe), "RecipeFeatures"))
    .Append(_mlContext.Transforms.Concatenate("Features", "IngredientsFeatures", "RecipeFeatures"))
    .Append(_mlContext.Transforms.Conversion.MapValueToKey(nameof(FoodRecipePrediction.Recipe)))
    .Append(_mlContext.MulticlassClassification.Trainers.SdcaNonCalibrated())
    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            // Train model
            TransformerChain<Microsoft.ML.Transforms.KeyToValueMappingTransformer> model = pipeline.Fit(trainTestSplit.TrainSet);

            // Evaluate model
            var metrics = _mlContext.MulticlassClassification.Evaluate(model.Transform(trainTestSplit.TestSet));

            Console.WriteLine($"Accuracy: {metrics.MacroAccuracy}");
            Console.WriteLine($"Log-loss: {metrics.LogLoss}");

            ModelEvaluationResult modelEvaluationResult = new ModelEvaluationResult { Model = model, Metrics = metrics };
            Console.WriteLine($"modelEvaluationResult: {modelEvaluationResult}");


            try
            {
                _mlContext.Model.Save(model, trainTestSplit.TrainSet.Schema, "./Models/MLModel.zip");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                logger.LogWarning("Cannot create MLModel zip file \n {0}", ex);
            }

            // Load trained model
            //var model = _mlContext.Model.Load("./Models/MLModel.zip", out var schema);


            return model;
        }

        public string GetRecipe(string ingredients)
        {
            TrainModel();
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
