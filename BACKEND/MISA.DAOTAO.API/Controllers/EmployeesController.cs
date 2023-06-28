using Dapper;
using Microsoft.AspNetCore.Mvc;
using MISA.DAOTAO.API.Entities;
using MySqlConnector;

namespace MISA.DAOTAO.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        /// <summary>
        /// Lấy danh sách tất cả nhân viên
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IActionResult GetAllEmployees()
        {
            try
            {
                //Khởi tạo kết nối tới DB MySQL
                string connectionString = "Server=localhost;Port=3306;Database=data.project1;Uid=root;Pwd=123456;";
                var mySqlConnection = new MySqlConnection(connectionString);

                //Chuẩn bị câu lệnh truy vấn
                string getAllEmployeeCommand = "SELECT * FROM employee;";

                //Thực hiện gọi vào DB để chạy câu lệnh truy vấn ở trên
                var employees = mySqlConnection.Query<Employee>(getAllEmployeeCommand);

                //Xử lý dữ liệu trả về
                if (employees != null)
                {
                    //Trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, employees);
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


        /// <summary>
        /// Lấy ra thông tin chi tiết 1 nhân viên
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{employeeID}")]
        public IActionResult GetEmployeeByID([FromRoute] Guid employeeID)
        {
            try
            {
                string connectionString = "Server=localhost;Port=3306;Database=data.project1;Uid=root;Pwd=123456;";
                var mySqlConnection = new MySqlConnection(connectionString);

                //Chuẩn bị tên Stored Procedure
                string storedProcedureName = "SELECET * FROM employee WHERE EmployeeID = @EmployeeID;";

                //Chuẩn bị tham số đầu vào cho stored procedure
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeID", employeeID);

                //Thực hiện gọi vào DB để chạy stored procedure với tham số đầu vào ở trên
                var employee = mySqlConnection.QueryFirstOrDefault<Employee>(storedProcedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);

                if (employee != null)
                {
                    //Trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, employee);
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

        /// <summary>
        /// API lọc danh sách nhân viên có điều kiện tìm kiếm và phân trang
        /// </summary>
        /// <param name="keyword">Từ khoá muốn tìm kiếm (Mã NV, Tên NV, SDT,...)</param>
        /// <param name="PositionID">ID của vị trí</param>
        /// <param name="DepartmentID">ID của phòng ban</param>
        /// <param name="limit">Số bản ghi trong 1 trang</param>
        /// <param name="offset">Vị trí bản ghi bắt đầu lấy dữ liệu</param>
        /// <returns>Danh sách nhân viên</returns>
        [HttpGet]
        [Route("filter")]
        public IActionResult FilterEmployees(
            [FromQuery] string? keyword,
            [FromQuery] Guid? PositionID,
            [FromQuery] Guid? DepartmentID,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1
            )
        {
            try
            {
                string connectionString = "Server=localhost;Port=3306;Database=data.project1;Uid=root;Pwd=123456;";
                var mySqlConnection = new MySqlConnection(connectionString);

                //Chuẩn bị tên Stored procedure
                string storedProcedureName = "Proc_employee_GetPaging";

                //Chuẩn bị tham số đầu vào cho stored procedure
                var parameters = new DynamicParameters();
                parameters.Add("@v_Offset", (pageNumber - 1) * pageSize);
                parameters.Add("@v_Limit", pageSize);
                parameters.Add("@v_Sort", "ModifiedDate DESC");

                //Xây dựng câu lệnh where
                var orConditions = new List<string>();
                var andCondittions = new List<string>();
                string whereClause = "";

                if (keyword != null)
                {
                    orConditions.Add($"EmployeeCode LIKE '%{keyword}%'");
                    orConditions.Add($"EmployeeName LIKE '%{keyword}%'");
                    orConditions.Add($"PhoneNumber LIKE '%{keyword}%'");
                }
                if (orConditions.Count > 0)
                {
                    whereClause = $"({string.Join(" OR ", orConditions)})";
                }
                if (PositionID != null)
                {
                    andCondittions.Add($"PositionID LIKE '%{PositionID}%'");
                }
                if (DepartmentID != null)
                {
                    andCondittions.Add($"DepartmentID LIKE '%{DepartmentID}%'");
                }
                if (andCondittions.Count > 0)
                {
                    whereClause += $" AND {string.Join(" AND ", andCondittions)}";
                }

                parameters.Add("@v_Where", whereClause);

                //Thực hiện gọi vào DB để chạy stored procedure với tham số đầu vào ở trên
                var multipleResults = mySqlConnection.QueryMultiple(storedProcedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);

                //
                if (multipleResults != null)
                {
                    var employees = multipleResults.Read<Employee>().ToList();
                    var totalCount = multipleResults.Read<int>().Single();
                    return StatusCode(StatusCodes.Status200OK, new PagingData()
                    {
                        Data = employees,
                        TotalCount = totalCount
                    });
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

        /// <summary>
        /// API thêm mới 1 nhân viên
        /// </summary>
        /// <param name="employee">Đối tượng nhân viên cần thêm mới</param>
        /// <returns>ID của nhân viên vừa thêm mới</returns>
        [HttpPost]
        [Route("")]
        public IActionResult InsertEmployee([FromBody] Employee employee)
        {
            try
            {
                //Khởi tạo kết nối tới DB MySQL
                string connectionString = "Server=localhost;Port=3306;Database=data.project1;Uid=root;Pwd=123456;";
                var mySqlConnection = new MySqlConnection(connectionString);

                //Chuẩn bị câu lệnh insert into
                string insertEmployeeCommand = "INSERT INTO employee (EmployeeID, EmployeeCode, EmployeeName, DateOfBirth, Gender, IdentityNumber, PositionID, PositionName, DepartmentID, DepartmentName, WorkStatus, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate) " +
                    "VALUES ( @EmployeeID, @EmployeeCode, @EmployeeName, @DateOfBirth, @Gender, @IdentityNumber, @PositionID, @PositionName, @DepartmentID, @DepartmentName, @WorkStatus, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate );";

                //Chuẩn bị tham số đầu vào cho câu lệnh insert into
                var employeeID = Guid.NewGuid(); //Guid.NewGuid => tạo ra 1 chuỗi 36 kí tự. Mỗi lần sẽ tạo ra 1 chuỗi khác nhau
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeID", employeeID);
                parameters.Add("@EmployeeCode", employee.EmployeeCode);
                parameters.Add("@EmployeeName", employee.EmployeeName);
                parameters.Add("@DateOfBirth", employee.DateOfBirth);
                parameters.Add("@Gender", employee.Gender);
                parameters.Add("@IdentityNumber", employee.IdentityNumber);
                parameters.Add("@PositionID", employee.PositionID);
                parameters.Add("@PositionName", employee.PositionName);
                parameters.Add("@DepartmentID", employee.DepartmentID);
                parameters.Add("@DepartmentName", employee.DepartmentName);
                parameters.Add("@WorkStatus", employee.WorkStatus);
                parameters.Add("@CreatedBy", employee.CreatedBy);
                parameters.Add("@CreatedDate", employee.CreatedDate);
                parameters.Add("@ModifiedBy", employee.ModifiedBy);
                parameters.Add("@ModifiedDate", employee.ModifiedDate);

                //Thực hiện gọi vào DB để chạy câu lệnh insert into với tham số đầu vào ở trên
                int numberOfAffectdRows = mySqlConnection.Execute(insertEmployeeCommand, parameters);

                //Xử lý kết quả trả về từ DB
                if (numberOfAffectdRows > 0)
                {
                    //Trả về dữ liệu từ client
                    return StatusCode(StatusCodes.Status201Created, employeeID);
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e002");
                }
            }
            catch (MySqlException mySqlException)
            {
                if (mySqlException.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e003");
                }
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }
        }

        /// <summary>
        /// API sửa 1 nhân viên
        /// </summary>
        /// <param name="employee">Đối tượng của nhân viên vừa sửa</param>
        /// <param name="employeeID">ID của nhân viên muốn sửa</param>
        /// <returns>ID của nhân viên vừa sửa</returns>
        [HttpPut]
        [Route("{employeeID}")]
        public IActionResult UpdateEmployee([FromBody] Employee employee, [FromRoute] Guid employeeID)
        {
            try
            {
                //Khởi tạo kết nối tới DB MySQL
                string connectionString = "Server=localhost;Port=3306;Database=data.project1;Uid=root;Pwd=123456;";
                var mySqlConnection = new MySqlConnection(connectionString);

                //Chuẩn bị câu lệnh update
                string updateEmployeeCommand = "UPDATE employee e " +
                    "SET EmployeeID = @EmployeeID, " +
                    "EmployeeCode = @EmployeeCode, " +
                    "EmployeeName = @EmployeeName, " +
                    "DateOfBirth = @DateOfBirth, " +
                    "Gender = @Gender, " +
                    "IdentityNumber = @IdentityNumber, " +
                    "PositionID = @PositionID, " +
                    "PositionName = @PositionName, " +
                    "DepartmentID = @DepartmentID, " +
                    "DepartmentName = @DepartmentName, " +
                    "WorkStatus = @WorkStatus, " +
                    "CreatedBy = @CreatedBy, " +
                    "CreatedDate = @CreatedDate, " +
                    "ModifiedBy = @ModifiedBy, " +
                    "ModifiedDate = @ModifiedDate " +
                    "WHERE EmployeeID = @EmployeeID;";

                var parameters = new DynamicParameters();

                parameters.Add("@EmpolyeeID", employeeID);
                parameters.Add("@EmployeeCode", employee.EmployeeCode);
                parameters.Add("@EmployeeName", employee.EmployeeName);
                parameters.Add("@DateOfBirth", employee.DateOfBirth);
                parameters.Add("@Gender", employee.Gender);
                parameters.Add("@IdentityNumber", employee.IdentityNumber);
                parameters.Add("@PositionID", employee.PositionID);
                parameters.Add("@PositionName", employee.PositionName);
                parameters.Add("@DepartmentID", employee.DepartmentID);
                parameters.Add("@DepartmentName", employee.DepartmentName);
                parameters.Add("@WorkStatus", employee.WorkStatus);
                parameters.Add("@CreatedBy", employee.CreatedBy);
                parameters.Add("@CreatedDate", employee.CreatedDate);
                parameters.Add("@ModifiedBy", employee.ModifiedBy);
                parameters.Add("@ModifiedDate", employee.ModifiedDate);

                int numberOfAffectedRows = mySqlConnection.Execute(updateEmployeeCommand, parameters);

                if (numberOfAffectedRows > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, employee);
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

        /// <summary>
        /// API xoá 1 nhân viên
        /// </summary>
        /// <param name="employeeID">ID của nhân viên muốn xoá</param>
        /// <returns>ID của nhân viên vừa xoá</returns>
        [HttpDelete]
        [Route("Delete")]
        public IActionResult DelEmployee([FromQuery] Guid employeeID)
        {
            try
            {
                //Khởi tạo kết nối tới DB MySQL
                string connectionString = "Server=localhost;Port=3306;Database=data.project1;Uid=root;Pwd=123456;";
                var mySqlConnection = new MySqlConnection(connectionString);

                //Chuẩn bị câu lệnh DEL
                string deleteEmployeeCommand = "DELETE FROM employee WHERE EmployeeID = @EmployeeID;";

                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeID", employeeID);

                int numberOfAffectedRows = mySqlConnection.Execute(deleteEmployeeCommand, parameters);

                if (numberOfAffectedRows > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, "Xoá thành công");
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
