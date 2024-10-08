﻿using System.ComponentModel.DataAnnotations;

namespace EmployeeContract.Models
{
    public class PositionsModel
    {
        [Key]
        public int PositionId { get; set; }

        [MaxLength(10)]
        public required string PositionCode { get; set; }

        [MaxLength(100)]
        public required string PositionName { get; set; }
        public bool IsActive { get; set; }

        public ICollection<DetailEmployeeModel> DetailEmployees { get; set; }
    }
}
