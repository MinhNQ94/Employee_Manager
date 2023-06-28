using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.DAOTAO.API.Entities;
using MySqlConnector;

namespace MISA.DAOTAO.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        /// <summary>
        /// Lấy ra danh sách tất cả vị trí
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{Position}")]
        public IActionResult GetAllPosition()
        {
            try
            {
                //Khởi tạo kết nối tới DB MySQL
                string connectionString = "Server=localhost;Port=3306;Database=data.project1;Uid=root;Pwd=123456;";
                var mySqlConnection = new MySqlConnection(connectionString);

                //Chuẩn bị câu lệnh truy vấn
                string getAllPositionCommand = "SELECT * FROM positions;";

                //Thực hiện gọi vào DB để chạy câu lệnh truy vấn ở trên
                var positions = mySqlConnection.Query<Position>(getAllPositionCommand);

                //Xử lý dữ liệu trả về
                if (positions != null)
                {
                    //Trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, positions);
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
