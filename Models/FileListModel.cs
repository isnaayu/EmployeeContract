using System.ComponentModel.DataAnnotations;

namespace EmployeeContract.Models
{
    public class FileListModel
    {
        [Key]
        public int FileListId { get; set; }

        [MaxLength(100)]
        public required string SubjectName { get; set; }

        public required String FileName { get; set; }
    }
}
