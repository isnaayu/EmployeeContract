CREATE PROCEDURE GetDetailEmployeeById
    @DetailEmployeeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        de.DetailEmployeeId,
        e.BirthDate,
        e.EmployeeId,
        e.EmployeeName,
        b.BranchId,
        b.BranchName,
        p.PositionId,
        p.PositionName,
        e.StartDate,
        de.ContractEnd
    FROM Employees e
    JOIN DetailEmployees de ON e.EmployeeId = de.EmployeeId
    JOIN Branches b ON de.BranchId = b.BranchId
    JOIN Positions p ON de.PositionId = p.PositionId
    WHERE de.DetailEmployeeId = @DetailEmployeeId;
END
