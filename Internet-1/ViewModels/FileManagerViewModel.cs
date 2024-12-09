using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Internet_1.ViewModels
{

    [Table("FileManagerViewModel")]
    public class FileManagerViewModel
    {
        [Key]
        public int Id { get; set; } // Primary Key
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public DateTime ModifiedDate { get; set; }
        public long Size { get; set; }
        public string UserId { get; internal set; }

    }
}