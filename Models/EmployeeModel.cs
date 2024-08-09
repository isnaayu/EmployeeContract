using System.ComponentModel.DataAnnotations;
namespace EmployeeContract.Models
{
    public class EmployeeModel
    {
        [Key]
        public int EmployeeId { get; set; }

        [MaxLength(100)]
        public required string EmployeeName { get; set; }

        public required DateTime BirthDate { get; set; }

        [MaxLength(10)]
        public required int ContractPeriod { get; set; }

        public required DateTime StartDate { get; set; }

        public ICollection<DetailEmployee> DetailEmployees { get; set; }
    }
}
