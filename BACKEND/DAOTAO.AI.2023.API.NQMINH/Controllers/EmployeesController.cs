using DAOTAO.AI._2023.API.NQMINH.Entities;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace DAOTAO.AI._2023.API.NQMINH.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        /// <summary>
        /// API lấy danh sách tất cả nhân viên (Đã xong)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IActionResult GetAllEmployees()
        {
            try
            {
                /// Khởi tạo kêt nối tới DB
                string connectionString = "Server=localhost;Port=3306;Database=db.project;Uid=root;Pwd=12345678";
                var mySqlConnection = new MySqlConnection(connectionString);

                /// Chuẩn bị câu lệnh truy vấn
                string getAllEmployeeCommand = "SELECT * FROM employee;";

                /// Thực hiện gọi vào DB để chạy câu lệnh truy vấn trên
                var employees = mySqlConnection.Query<Employee>(getAllEmployeeCommand);

                if (employees != null)
                {
                    ///trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, employees);
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e002");
                }
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }
        }
        /// <summary>
        /// Lấy ra thông tin chi tiết 1 nhân viên (Đã xong)
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{employeeCode}")]
        public IActionResult GetEmployeeByCode([FromRoute] string? employeeCode)
        {
            /// Khởi tạo kêt nối tới DB
            string connectionString = "Server=localhost;Port=3306;Database=db.project;Uid=root;Pwd=12345678";
            var mySqlConnection = new MySqlConnection(connectionString);
            try
            {
                ///Chuẩn bị tham số đầu vào cho stored procedure
                var parameters = new DynamicParameters();

                /// Chuẩn bị câu lệnh truy vấn
                string storedProcedureName = "SELECT * FROM employee WHERE EmployeeCode = @EmployeeCode;";

                parameters.Add("@EmployeeCode", employeeCode);

                /// Thực hiện gọi vào DB để chạy câu lệnh truy vấn trên
                var employee = mySqlConnection.QueryFirstOrDefault<Employee>(storedProcedureName, parameters);

                if (employee != null)
                {
                    ///trả về dữ liệu cho client
                    return StatusCode(StatusCodes.Status200OK, employee);
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
        /// <summary>
        /// API lọc danh sách nhân viên có điều kiện tìm kiếm và phân trang (Đã xong)
        /// </summary>
        /// <param name="keyword">Từ khoá muốn tìm kiếm</param>
        /// <param name="positionID">ID chức danh</param>
        /// <param name="departmentID">ID vị trí</param>
        /// <param name="limit">Số bản ghi trong 1 trang</param>
        /// <param name="offset">Vị trí bản ghi bắt đầu lấy dữ liệu</param>
        /// <returns></returns>
        [HttpGet]
        [Route("filter")]
        public IActionResult FilterEmployees(
            [FromQuery] string? keyword,
            [FromQuery] string? positionName,
            [FromQuery] string? departmentName,
            [FromQuery] int? pageSize = 10,
            [FromQuery] int? pageNumber = 1)
        {
            /// Khởi tạo kết nối tới DB MySQL
            string connectionString = "Server=localhost;Port=3306;Database=db.project;Uid=root;Pwd=12345678";
            var mySqlConnection = new MySqlConnection(connectionString);
            try
            {
                ///Chuẩn bị tên stored procedure
                string storedProcedureName = "Proc_employee_GetPaging";

                ///Chuẩn bị tham số đầu vào cho stored procedure
                var parameters = new DynamicParameters();
                parameters.Add("@v_Offset", (pageNumber - 1) * pageSize);
                parameters.Add("@v_Limit", pageSize);
                parameters.Add("@v_Sort", "");

                /// Xây dựng câu lệnh where
                var orConditions = new List<string>();
                var andConditions = new List<string>();
                string whereClause = "";

                if (keyword != null)
                {
                    orConditions.Add($"EmployeeCode LIKE '%{keyword}%'");
                    orConditions.Add($"EmployeeName LIKE '%{keyword}%'");
                }
                if (orConditions.Count > 0)
                {
                    whereClause = $"({string.Join(" OR ", orConditions)})";
                }

                if (positionName != null)
                {
                    andConditions.Add($"PositionName LIKE '%{positionName}%'");
                }
                if (departmentName != null)
                {
                    andConditions.Add($"DepartmentName LIKE '%{departmentName}%'");
                }

                if (andConditions.Count > 0)
                {
                    whereClause += $" AND {string.Join(" AND ", andConditions)}";
                }

                parameters.Add("@v_Where", whereClause);

                ///Thực hiện gọi vào DB để chạy stored procedure với tham số đầu vào ở trên
                var multipleResults = mySqlConnection.QueryMultiple(storedProcedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);

                /// Xử lý kết quả trả về từ DB
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
            catch (Exception exception) 
            {
                Console.WriteLine(exception.Message);
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }
        }
        /// <summary>
        /// API thêm mới 1 nhân viên (Đã xong)
        /// </summary>
        /// <param name="employee">Đối tượng nhân viên cần thêm mới</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public IActionResult InsertEmployee([FromBody] Employee employee)
        {
            string connectionString = "Server=localhost;Port=3306;Database=db.project;Uid=root;Pwd=12345678";
            var mySqlConnection = new MySqlConnection(connectionString);
            try
            {

                string insertEmployeeCommand = "INSERT INTO employee (EmployeeCode, EmployeeName, Gender," +
                    " DateOfBirth, IdentityNumber, IdentityIssuedDate, IdentityIssuedPlace, DepartmentCode," +
                    " DepartmentName, PositionCode, PositionName, MobilePhone, Telephone, Address, Email, BankAccount," + 
                    " BankName, Branch) VALUES (@EmployeeCode, @EmployeeName, @Gender, @DateOfBirth, @IdentityNumber, @IdentityIssuedDate," +
                    " @IdentityIssuedPlace, @DepartmentCode, @DepartmentName, @PositionCode, @PositionName, @MobilePhone, @Telephone, @Address, @Email, @BankAccount, @BankName, @Branch);";

                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeCode", employee.EmployeeCode);
                parameters.Add("@EmployeeName", employee.EmployeeName);
                parameters.Add("@Gender", employee.Gender);
                parameters.Add("@DateOfBirth", employee.DateOfBirth);
                parameters.Add("@IdentityNumber", employee.IdentityNumber);
                parameters.Add("@IdentityIssuedDate", employee.IdentityIssuedDate);
                parameters.Add("@IdentityIssuedPlace", employee.IdentityIssuedPlace);
                parameters.Add("@DepartmentCode", employee.DepartmentCode);
                parameters.Add("@DepartmentName", employee.DepartmentName);
                parameters.Add("@PositionCode", employee.PositionCode);
                parameters.Add("@PositionName", employee.PositionName);
                parameters.Add("@MobilePhone", employee.MobilePhone);
                parameters.Add("@Telephone", employee.Telephone);
                parameters.Add("@Address", employee.Address);
                parameters.Add("@Email", employee.Email);
                parameters.Add("@BankAccount", employee.BankAccount);
                parameters.Add("@BankName", employee.BankName);
                parameters.Add("@Branch", employee.Branch);

                int numberOfAffectedRows = mySqlConnection.Execute(insertEmployeeCommand, parameters);

                if (numberOfAffectedRows > 0)
                {
                    return StatusCode(StatusCodes.Status201Created, employee.EmployeeCode);
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e002");
                }
            }
            catch (MySqlException mysqlException)
            {
                ///Trùng mã nhân viên
                if (mysqlException.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e003");
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e001");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }
        }
        /// <summary>
        /// API sửa 1 nhân viên (Đã xong)
        /// </summary>
        /// <param name="employee">Đối tượng nhân viên cần sửa</param>
        /// <param name="employeeID">ID của nhân viên cần sửa</param>
        /// <returns></returns>

        [HttpPut]
        [Route("{employeeCode}")]
        public IActionResult UpdateEmployee([FromRoute] string? employeeCode, [FromBody] Employee employee)
        {
            string connectionString = "Server=localhost;Port=3306;Database=db.project;Uid=root;Pwd=12345678";
            var mySqlConnection = new MySqlConnection(connectionString);
            try
            {
                string updateEmployeeCommand = "UPDATE employee e SET EmployeeCode = @newEmployeeCode, EmployeeName = @EmployeeName," + 
                    " Gender = @Gender, DateOfBirth = @DateOfBirth, IdentityNumber = @IdentityNumber, IdentityIssuedDate = @IdentityIssuedDate," + 
                    " IdentityIssuedPlace = @IdentityIssuedPlace, DepartmentCode = @DepartmentCode, DepartmentName = @DepartmentName," + 
                    " PositionCode = @PositionCode, PositionName = @PositionName, MobilePhone = @MobilePhone, Telephone = @Telephone," + 
                    " Address = @Address, Email = @Email, BankAccount = @BankAccount, BankName = @BankName, Branch = @Branch WHERE EmployeeCode = @EmployeeCode;";

                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeCode", employeeCode);
                parameters.Add("@newEmployeeCode", employee.EmployeeCode);
                parameters.Add("@EmployeeName", employee.EmployeeName);
                parameters.Add("@Gender", employee.Gender);
                parameters.Add("@DateOfBirth", employee.DateOfBirth);
                parameters.Add("@IdentityNumber", employee.IdentityNumber);
                parameters.Add("@IdentityIssuedDate", employee.IdentityIssuedDate);
                parameters.Add("@IdentityIssuedPlace", employee.IdentityIssuedPlace);
                parameters.Add("@DepartmentCode", employee.DepartmentCode);
                parameters.Add("@DepartmentName", employee.DepartmentName);
                parameters.Add("@PositionCode", employee.PositionCode);
                parameters.Add("@PositionName", employee.PositionName);
                parameters.Add("@MobilePhone", employee.MobilePhone);
                parameters.Add("@Telephone", employee.Telephone);
                parameters.Add("@Address", employee.Address);
                parameters.Add("@Email", employee.Email);
                parameters.Add("@BankAccount", employee.BankAccount);
                parameters.Add("@BankName", employee.BankName);
                parameters.Add("@Branch", employee.Branch);

                int numberOfAffectedRows = mySqlConnection.Execute(updateEmployeeCommand, parameters);

                /// Xử lý kq trả về từ DB
                if (numberOfAffectedRows > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, employee);
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
        /// <summary>
        /// API xoá 1 nhân viên (đã xong)
        /// </summary>
        /// <param name="employeeID">ID của nhân viên muốn xoá</param>
        /// <returns>ID của nhân viên vừa xoá</returns>
        [HttpDelete]
        [Route("Delete")]
        public IActionResult DeleteEmployee([FromQuery] string? employeeCode)
        {
            string connectionString = "Server=localhost;Port=3306;Database=db.project;Uid=root;Pwd=12345678";
            var mySqlConnection = new MySqlConnection(connectionString);
            try
            {
                var parameters = new DynamicParameters();
                string delCommand = "DELETE FROM employee WHERE EmployeeCode = @EmployeeCode;";
                parameters.Add("@EmployeeCode", employeeCode);
                int numberOfAffectedRows = mySqlConnection.Execute(delCommand, parameters);

                if (numberOfAffectedRows > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, "Đã xoá thành công nhân viên");
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
