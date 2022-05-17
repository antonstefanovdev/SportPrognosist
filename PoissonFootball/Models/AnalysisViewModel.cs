namespace PoissonFootball.Models
{
    public class AnalysisViewModel
    {
        public Dictionary<string, double> AnalysisData { get; set; } = 
            new Dictionary<string, double>();

        public Dictionary<string, string> DefaultInputValues { get; set; } = 
            new Dictionary<string, string>();
    }
}
