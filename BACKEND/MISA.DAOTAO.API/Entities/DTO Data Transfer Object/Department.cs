namespace MISA.DAOTAO.API.Entities
{
    /// <summary>
    /// Thông tin phòng ban
    /// </summary>
    public class Department
    {
        public Guid DepartmentID { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
