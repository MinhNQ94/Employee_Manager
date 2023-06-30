namespace DAOTAO.AI._2023.API.NQMINH.Entities
{
    /// <summary>
    /// Thông tin Đơn vị
    /// </summary>
    public class Department
    {
        public Guid DepartmentID { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }
}
