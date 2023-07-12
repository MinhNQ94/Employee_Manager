using DAOTAO.AI._2023.API.NQMINH.Entities;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace DAOTAO.AI._2023.API.NQMINH.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        /// <summary>
        /// API lấy danh sách tất cả các chức danh (Đã xong)
        /// </summary>
        /// <returns>Danh sách cả chức danh</returns>
        [HttpGet]
        [Route("")]
        public IActionResult GetAllPosition()
        {
            try
            {
                /// Khởi tạo kêt nối tới DB
                string connectionString = "Server=localhost;Port=3306;Database=db.project;Uid=root;Pwd=12345678";
                var mySqlConnection = new MySqlConnection(connectionString);

                /// Chuẩn bị câu lệnh truy vấn
                string getAllPositionsCommand = "SELECT * FROM positions;";

                /// Thực hiện gọi vào DB để chạy câu lệnh truy vấn trên
                var positions = mySqlConnection.Query<Position>(getAllPositionsCommand);

                /// Xử lý dữ liệu trả về
                if (positions != null)
                {
                    return StatusCode(StatusCodes.Status200OK, positions);
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
