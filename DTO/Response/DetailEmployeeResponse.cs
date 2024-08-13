namespace EmployeeContract.DTO.Response
{
    public class DetailEmployeeResponse
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime BirthDate { get; set; }
        public int ContractPeriod { get; set; }
        public DateTime StartDate { get; set; }
        public int DetailEmployeeId { get; set; }
        public int PositionId { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public int BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public DateTime ContractEnd { get; set; }
    }
}
