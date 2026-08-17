namespace WebApplication.Application.DTOs
{
    public class AggregationResultDto
    {
        public int FileId { get; set; }
        public required string FileName { get; set; }
        public int DeltaDate { get; set; }
        public DateTime MinDate { get; set; }
        public double MeanExecutionTime { get; set; }
        public double MeanValue { get; set; }
        public double MedianValue { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
    }
}