using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication.Model.Entities
{
    public class AggregationResult
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int FileId { get; set; }
        public int DeltaDate { get; set; }
        public DateTime MinDate { get; set; }
        public double MeanExecutionTime { get; set; }
        public double MeanValue { get; set; }
        public double MedianValue { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
    
        public UploadedFile? File { get; set; }
    }
}