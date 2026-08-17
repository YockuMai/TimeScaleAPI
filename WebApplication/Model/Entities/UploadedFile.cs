using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication.Model.Entities
{
    public class UploadedFile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<DataValue> Values { get; set; } = new List<DataValue>();
        public AggregationResult? Result { get; set; }
    }
}