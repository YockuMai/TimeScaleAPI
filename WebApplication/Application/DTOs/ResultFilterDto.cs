namespace WebApplication.Application.DTOs
{
    public class ResultFilterDto
    {
        public string? FileName { get; set; }
        public DateTime? FirstOperationFrom { get; set; }
        public DateTime? FirstOperationTo { get; set; }
        public double? MeanValueFrom { get; set; }
        public double? MeanValueTo { get; set; }
        public double? MeanExecutionTimeFrom { get; set; }
        public double? MeanExecutionTimeTo { get; set; }
    }
}