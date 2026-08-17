using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication.Model.Entities
{
    public class DataValue
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int FileId { get; set; }
        public DateTime Date { get; set; }
        public int ExecutionTime { get; set; }
        public double Value { get; set; }
    
        public UploadedFile? File { get; set; }
    }
}