using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.DAOTAO.API.Entities;
using MySqlConnector;

namespace MISA.DAOTAO.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        /// <summary>
        /// Lấy danh sách tất cả phòng ban
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{Department}")]
        public IActionResult GetAllDepartment()
        {
            try
            {
                //Khởi tạo kết nối tới DB MySQL
                string connectionString = "Server=localhost;Port=3306;Database=data.project1;Uid=root;Pwd=123456;";
                var mySqlConnection = new MySqlConnection(connectionString);

                //Chuẩn bị câu lệnh truy vấn
                string getAllDepartmentCommand = "SELECT * FROM department;";

                //Thực hiện gọi vào DB để chạy câu lệnh truy vấn ở trên
                var departments = mySqlConnection.Query<Department>(getAllDepartmentCommand);

                //Xử lý dữ liệu trả về
                if (departments != null)
                {
                    //Trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, departments);
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e002");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }
        }
    }
}
