namespace EmployeeContract.DTO.Request
{
    public class DetailEmployeeRequest
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime BirthDate { get; set; }
        public int ContractPeriod { get; set; }
        public DateTime StartDate { get; set; }
        public int DetailEmployeeId { get; set; }
        public int PositionId { get; set; }
        public int BranchId { get; set; }
    }
}
