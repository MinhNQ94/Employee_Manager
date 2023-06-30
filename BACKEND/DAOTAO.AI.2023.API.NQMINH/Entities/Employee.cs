namespace DAOTAO.AI._2023.API.NQMINH.Entities
{
    /// <summary>
    /// Thông tin nhân viên
    /// </summary>
    public class Employee
    {
        public Guid EmployeeID { get; set; }
        public string EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public int? Gender { get; set; }
        public DateTime? DateOfBirth { get; set;}
        public string? IdentityNumber { get; set;}
        public DateTime? IdentityIssuedDate { get; set; }
        public string? IdentityIssuedPlace { get; set; }
        public Guid? DepartmentID { get; set; }
        public string? DepartmentName { get; set;}
        public Guid? PositionID { get; set;}
        public string? PositionName { get; set;}
        public string? MobilePhone { get; set;}
        public string? Telephone { get; set;}
        public string? Address { get; set;}
        public string? Email { get; set;}
        public string? BankAccount { get; set;}
        public string? BankName { get; set;}
        public string? Branch { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
