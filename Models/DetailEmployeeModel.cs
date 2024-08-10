using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EmployeeContract.Models
{
    public class DetailEmployeeModel
    {
        [Key]
        public int DetailEmployeeId { get; set; }

        [ForeignKey("Position")]
        public int PositionId { get; set; }
        public PositionsModel Position { get; set; }


        [ForeignKey("Branch")]
        public int BranchId { get; set; }
        public BranchModel Branch { get; set; }

 
        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }
        public EmployeeModel Employee { get; set; }

        public DateTime ContractEnd { get; set; }
    }
}
