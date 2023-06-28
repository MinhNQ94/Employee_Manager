namespace MISA.DAOTAO.API.Entities
{
    /// <summary>
    /// Dữ liệu trả về từ API lọc nhân viên
    /// </summary>
    public class PagingData
    {
        /// <summary>
        /// Danh sách nhân viên
        /// </summary>
        public List<Employee> Data { get; set; }
        /// <summary>
        /// Tổng số bản ghi thoả mãn điều kiện
        /// </summary>
        public int TotalCount { get; set; }
    }
}
