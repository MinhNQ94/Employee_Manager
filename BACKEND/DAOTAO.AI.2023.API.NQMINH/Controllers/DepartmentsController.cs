using DAOTAO.AI._2023.API.NQMINH.Entities;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace DAOTAO.AI._2023.API.NQMINH.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        /// <summary>
        /// API lấy danh sách tất cả các phòng ban (Đã xong)
        /// </summary>
        /// <returns>Danh sách cả vị trí</returns>
        [HttpGet]
        [Route("")]
        public IActionResult GetAllDepartment()
        {
            /// Khởi tạo kêt nối tới DB
            string connectionString = "Server=localhost;Port=3306;Database=db.project;Uid=root;Pwd=12345678";
            var mySqlConnection = new MySqlConnection(connectionString);
            try
            {

                /// Chuẩn bị câu lệnh truy vấn
                string getAllDepartmentsCommand = "SELECT * FROM department;";

                /// Thực hiện gọi vào DB để chạy câu lệnh truy vấn trên
                var departments = mySqlConnection.Query<Department>(getAllDepartmentsCommand);

                /// Xử lý dữ liệu trả về
                if (departments!= null )
                {
                    return StatusCode(StatusCodes.Status200OK, departments);
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e002");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }
        }
    }
}
