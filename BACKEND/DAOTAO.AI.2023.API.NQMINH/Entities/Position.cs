namespace DAOTAO.AI._2023.API.NQMINH.Entities
{
    /// <summary>
    /// Thông tin chức danh
    /// </summary>
    public class Position
    {
        public Guid PositionID { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }
}
