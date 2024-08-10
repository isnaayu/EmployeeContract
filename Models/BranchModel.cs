using System.ComponentModel.DataAnnotations;

namespace EmployeeContract.Models
{
    public class BranchModel
    {
        [Key]
        public int BranchId { get; set; }

        [MaxLength(10)]
        public required string BranchCode { get; set; }

        [MaxLength(100)]
        public required string BranchName { get; set; }

        public ICollection<DetailEmployeeModel> DetailEmployees { get; set; }
    }
}
