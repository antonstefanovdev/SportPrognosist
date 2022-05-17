using PoissonFootball.Models;
using PoissonFootball.Models.Static;

namespace PoissonFootball.Services
{
    public class PredictionService
    {
        private double poisson(int goalCount, double lambda)
        {
            static int fact(int n)
            {
                int result = 1;
                for (int i = 2; i <= n; i++)
                    result *= i;
                return result;
            }

            return Math.Pow(lambda, goalCount) * Math.Exp(-lambda) / fact(goalCount);            
        }

        private ProcessStatisticsResult processStatistics(Dictionary<string,double> statisticsData)
        {
            static int tryParseInt(string val)
            {
                try
                {
                    return int.Parse(val);
                }
                catch (FormatException)
                {
                    return 0;
                }
            }

            ProcessStatisticsResult result = new();
            if (statisticsData != null && statisticsData.Any())
            {
                int validItemsCount = 0;
                //result.ownersLambda = 0.0;
                //result.guestsLambda = 0.0;

                foreach (var match in statisticsData)
                {
                    if (match.Key.Contains(':'))
                    {
                        var matchScore = match.Key.Split(':');
                        if (matchScore != null && matchScore.Length == 2)
                        {
                            validItemsCount++;
                            result.ownersLambda += tryParseInt(matchScore[0]);
                            result.guestsLambda += tryParseInt(matchScore[1]);
                        }
                    }
                }
                if (validItemsCount > 0)
                {
                    result.ownersLambda /= validItemsCount;
                    result.guestsLambda /= validItemsCount;
                }
            }

            return result;
        }
        public AnalysisViewModel predictAll(StatisticsDataModel statisticsData, 
            List<int>? goals = null,
            double timeLast = 1.0,
            int goalCountMin = 0,
            int goalCountMax = 6)
        {
            Dictionary<string, double> data = new();
            if ((goals == null) 
                || (goalCountMin > goalCountMax) 
                || (goalCountMin < 0)
                || (timeLast < 0 || timeLast > 1)
                || (statisticsData == null))
            {
                data.Add("были переданы некорректные данные", 0);
            }
            else
            {
                var statisticsResult = processStatistics(statisticsData.StatisticsData);
                var ownersCurrentGoalCount = 0;
                var guestsCurrentGoalCount = 0;

                foreach (var goal in goals)
                    if (goal == 0)
                        ownersCurrentGoalCount++;
                    else if(goal == 1)
                        guestsCurrentGoalCount++;

                var ownersVictoryProbability = 0.0;
                var guestsVictoryProbability = 0.0;
                var drawProbability = 0.0;
                var intervalNotCompletedProbability = 1.0;

                for (int ownersGoals = ownersCurrentGoalCount;
                    ownersGoals <= goalCountMax; ownersGoals++)
                    for (int guestsGoals = guestsCurrentGoalCount;
                        guestsGoals <= goalCountMax; guestsGoals++)
                        if (ownersGoals + guestsGoals <= goalCountMax 
                            && ownersGoals + guestsGoals >= goalCountMin)
                        {
                            var probability = poisson(ownersGoals - ownersCurrentGoalCount, statisticsResult.ownersLambda * (1 - timeLast))
                                * poisson(guestsGoals - guestsCurrentGoalCount, statisticsResult.guestsLambda * (1 - timeLast));

                            if (ownersGoals > guestsGoals)
                                ownersVictoryProbability += probability;
                            else if (ownersGoals == guestsGoals)
                                drawProbability += probability;
                            else
                                guestsVictoryProbability += probability;
                        }

                intervalNotCompletedProbability -= ownersVictoryProbability
                    + guestsVictoryProbability + drawProbability;

                data.Add(PredictionResults.ownersVictory, ownersVictoryProbability);
                data.Add(PredictionResults.draw, drawProbability);
                data.Add(PredictionResults.guestsVictory, guestsVictoryProbability);
                data.Add(PredictionResults.intervalNotCompleted, intervalNotCompletedProbability);
            }

            return new AnalysisViewModel()
            {
                AnalysisData = data
            };
        }

    }
}
