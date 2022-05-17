using Microsoft.AspNetCore.Mvc;
using PoissonFootball.Models;
using PoissonFootball.Services;
using System.Diagnostics;

namespace PoissonFootball.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Seed seed;
        private readonly StatisticsDataModel statisticsData;
        private readonly PredictionService predictionService;
        private AnalysisViewModel viewModel;

        public HomeController(ILogger<HomeController> logger, 
            Seed seed, 
            StatisticsDataModel statisticsData,
            PredictionService predictionService,
            AnalysisViewModel viewModel)
        {
            _logger = logger;
            this.seed = seed;
            this.statisticsData = statisticsData;
            this.predictionService = predictionService;
            this.viewModel = viewModel;
        }

        public IActionResult Index(string? Goals, int? CurrentTime, int? minOfGoals, int? maxOfGoals)
        {
            seed.InitStat(statisticsData);
            if (CurrentTime == null && minOfGoals == null && maxOfGoals == null)
            {                        
                Dictionary<string, string> defaultValues = new();
                defaultValues.Add("Goals", "ххг");
                defaultValues.Add("CurrentTime", "30");
                defaultValues.Add("minOfGoals", "2");
                defaultValues.Add("maxOfGoals", "5");

                viewModel.DefaultInputValues = defaultValues;

                var model = viewModel;
                return View(model);
            }
            {
                var ownersDomain = new char[] { 'o', 'O', 'h', 'H', 'X', 'x', 'Х', 'х' };

                List<int> goals = new();
                if (!string.IsNullOrEmpty(Goals))
                    foreach (var goal in Goals)
                    {
                        if (ownersDomain.Contains(goal))
                            goals.Add(0);
                        else
                            goals.Add(1);
                    }                               

                viewModel = predictionService.predictAll(statisticsData, 
                    goals, 
                    CurrentTime == null? 0: (double)CurrentTime/90.0,
                    minOfGoals == null? 0: (int)minOfGoals,
                    maxOfGoals == null? 0: (int)maxOfGoals
                    );

                Dictionary<string, string> defaultValues = new();
                defaultValues.Add("Goals", (string)Goals);
                defaultValues.Add("CurrentTime", 
                    (CurrentTime == null ? 0 : (int)CurrentTime).ToString());
                defaultValues.Add("minOfGoals", 
                    (minOfGoals == null ? 0 : (int)minOfGoals).ToString());
                defaultValues.Add("maxOfGoals",
                    (maxOfGoals == null ? 0 : (int)maxOfGoals).ToString());

                viewModel.DefaultInputValues = defaultValues;

                var model = viewModel;
                return View(model);
            }
        }        


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}