CREATE PROCEDURE GetAllEmployeesWithDetails
AS
BEGIN
    SELECT e.EmployeeId, e.EmployeeName, e.BirthDate, e.ContractPeriod, e.StartDate,
           de.DetailEmployeeId, de.PositionId, p.PositionName, de.BranchId, b.BranchName, de.ContractEnd
    FROM Employees e
    INNER JOIN DetailEmployees de ON e.EmployeeId = de.EmployeeId
    INNER JOIN Positions p ON de.PositionId = p.PositionId
    INNER JOIN Branches b ON de.BranchId = b.BranchId
END
