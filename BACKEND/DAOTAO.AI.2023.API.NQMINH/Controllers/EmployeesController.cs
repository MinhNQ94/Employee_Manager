using DAOTAO.AI._2023.API.NQMINH.Entities;
using DAOTAO.AI._2023.API.NQMINH.Entities.Data_Tranfer_Object;
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
        /// API lấy danh sách tất cả nhân viên
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IActionResult GetAllEmployees()
        {
            try
            {
                /// Khởi tạo kêt nối tới DB
                string connectionString = "Server=18.179.16.166;Port=3306;Database=DAOTAO.AI.2023.NQMINH;Uid=nvmanh;Pwd=12345678";
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
                /// Khởi tạo kêt nối tới DB
                string connectionString = "Server=18.179.16.166;Port=3306;Database=DAOTAO.AI.2023.NQMINH;Uid=nvmanh;Pwd=12345678";
                var mySqlConnection = new MySqlConnection(connectionString);

                /// Chuẩn bị câu lệnh truy vấn
                string storedProcedureName = "SELECT * FROM employee WHERE EmployeeID = @EmployeeID;";

                ///Chuẩn bị tham số đầu vào cho stored procedure
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeID", employeeID);

                /// Thực hiện gọi vào DB để chạy câu lệnh truy vấn trên
                var employee = mySqlConnection.QueryFirstOrDefault<Employee>(storedProcedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);

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
        /// API lọc danh sách nhân viên có điều kiện tìm kiếm và phân trang
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
            [FromQuery] Guid? positionID,
            [FromQuery] Guid? departmentID,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1)
        {
            try
            {
                /// Khởi tạo kết nối tới DB MySQL
                string connectionString = "Server=18.179.16.166;Port=3306;Database=DAOTAO.AI.2023.NQMINH;Uid=nvmanh;Pwd=12345678";
                var mySqlConnection = new MySqlConnection(connectionString);

                ///Chuẩn bị tên stored procedure
                string storedProcedureName = "Proc_employee_GetPaging";

                ///Chuẩn bị tham số đầu vào cho stored procedure
                var parameters = new DynamicParameters();
                parameters.Add("@v_Offset", (pageNumber - 1) * pageSize);
                parameters.Add("@v_Limit", pageSize);
                parameters.Add("@v_Sort", "ModifiedDate DESC");

                /// Xây dựng câu lệnh where
                var orConditions = new List<string>();
                var andConditions = new List<string>();
                string whereClause = "";

                if (keyword != null)
                {
                    orConditions.Add($"EmployeeCode LIKE '%{keyword}%'");
                    orConditions.Add($"EmployeeName LIKE '%{keyword}%'");
                    orConditions.Add($"MobilePhone LIKE '%{keyword}%'");
                }
                if (orConditions.Count > 0)
                {
                    whereClause = $"({string.Join(" OR ", orConditions)})";
                }

                if (positionID != null)
                {
                    andConditions.Add($"PositionID LIKE '%{positionID}%'");
                }
                if (departmentID != null)
                {
                    andConditions.Add($"DepartmentID LIKE '%{departmentID}%'");
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
                    var totalCount = multipleResults.Read<long>().Single();
                    return StatusCode(StatusCodes.Status200OK, new PagingData<Employee>()
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
        /// API thêm mới 1 nhân viên
        /// </summary>
        /// <param name="employee">Đối tượng nhân viên cần thêm mới</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult InsertEmployee([FromBody] Employee employee)
        {
            try
            {
                string connectionString = "Server=18.179.16.166;Port=3306;Database=DAOTAO.AI.2023.NQMINH;Uid=nvmanh;Pwd=12345678";
                var mySqlConnection = new MySqlConnection(connectionString);

                string insertEmployeeCommand = "INSERT INTO employee (EmployeeID, EmployeeCode, EmployeeName, Gender, DateOfBirth, IdentityNumber, IdentityIssuedDate, IdentityIssuedPlace, DepartmentID, DepartmentName, PositionID, PositionName, MobilePhone, Telephone, Address, Email, BankAccount, BankName, Branch, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate) VALUES (@EmployeeID, @EmployeeCode, @EmployeeName, @Gender, @DateOfBirth, @IdentityNumber, @IdentityIssuedDate, @IdentityIssuedPlace, @DepartmentID, @DepartmentName, @PositionID, @PositionName, @MobilePhone, @Telephone, @Address, @Email, @BankAccount, @BankName, @Branch, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate)";

                var employeeID = Guid.NewGuid();
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeID", employeeID);
                parameters.Add("@EmployeeCode", employee.EmployeeCode);
                parameters.Add("@EmployeeName", employee.EmployeeName);
                parameters.Add("@Gender", employee.Gender);
                parameters.Add("@DateOfBirth", employee.DateOfBirth);
                parameters.Add("@IdentityNumber", employee.IdentityNumber);
                parameters.Add("@IdentityIssuedDate", employee.IdentityIssuedDate);
                parameters.Add("@IdentityIssuedPlace", employee.IdentityIssuedPlace);
                parameters.Add("@DepartmentID", employee.DepartmentID);
                parameters.Add("@DepartmentName", employee.DepartmentName);
                parameters.Add("@PositionID", employee.PositionID);
                parameters.Add("@PositionName", employee.PositionName);
                parameters.Add("@MobilePhone", employee.MobilePhone);
                parameters.Add("@Telephone", employee.Telephone);
                parameters.Add("@Address", employee.Address);
                parameters.Add("@Email", employee.Email);
                parameters.Add("@BankAccount", employee.BankAccount);
                parameters.Add("@BankName", employee.BankName);
                parameters.Add("@Branch", employee.Branch);
                parameters.Add("@CreatedBy", employee.CreatedBy);
                parameters.Add("@CreatedDate", employee.CreatedDate);
                parameters.Add("@ModifiedBy", employee.ModifiedBy);
                parameters.Add("@ModifiedDate", employee.ModifiedDate);

                int numberOfAffectedRows = mySqlConnection.Execute(insertEmployeeCommand, parameters);

                if (numberOfAffectedRows > 0)
                {
                    return StatusCode(StatusCodes.Status201Created, employeeID);
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "e002");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == (int)MySqlErrorCode.DuplicateKeyEntry)
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
        /// API sửa 1 nhân viên
        /// </summary>
        /// <param name="employee">Đối tượng nhân viên cần sửa</param>
        /// <param name="employeeID">ID của nhân viên cần sửa</param>
        /// <returns></returns>

        [HttpPut]
        [Route("{employeeID}")]
        public IActionResult UpdateEmployee([FromRoute] Guid employeeID, [FromBody] Employee employee)
        {
            try
            {
                string connectionString = "Server=18.179.16.166;Port=3306;Database=DAOTAO.AI.2023.NQMINH;Uid=nvmanh;Pwd=12345678";
                var mySqlConnection = new MySqlConnection(connectionString);
                string updateEmployeeCommand = "UPDATE employee e SET EmployeeCode = @EmployeeCode, EmployeeName = @EmployeeName, Gender = @Gender, DateOfBirth = @DateOfBirth, IdentityNumber = @IdentityNumber, IdentityIssuedDate = @IdentityIssuedDate, IdentityIssuedPlace = @IdentityIssuedPlace, DepartmentID = @DepartmentID, DepartmentName = @DepartmentName, PositionID = @PositionID, PositionName = @PositionName, MobilePhone = @MobilePhone, Telephone = @Telephone, Address = @Address, Email = @Email, BankAccount = @BankAccount, BankName = @BankName, Branch = @Branch, CreatedBy = @CreatedBy, CreatedDate = @CreatedDate, ModifiedBy = @ModifiedBy, ModifiedDate = @ModifiedDate WHERE EmployeeID = @EmployeeID";

                var employeeId = Guid.NewGuid();
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeID", employeeId);
                parameters.Add("@EmployeeCode", employee.EmployeeCode);
                parameters.Add("@EmployeeName", employee.EmployeeName);
                parameters.Add("@Gender", employee.Gender);
                parameters.Add("@DateOfBirth", employee.DateOfBirth);
                parameters.Add("@IdentityNumber", employee.IdentityNumber);
                parameters.Add("@IdentityIssuedDate", employee.IdentityIssuedDate);
                parameters.Add("@IdentityIssuedPlace", employee.IdentityIssuedPlace);
                parameters.Add("@DepartmentID", employee.DepartmentID);
                parameters.Add("@DepartmentName", employee.DepartmentName);
                parameters.Add("@PositionID", employee.PositionID);
                parameters.Add("@PositionName", employee.PositionName);
                parameters.Add("@MobilePhone", employee.MobilePhone);
                parameters.Add("@Telephone", employee.Telephone);
                parameters.Add("@Address", employee.Address);
                parameters.Add("@Email", employee.Email);
                parameters.Add("@BankAccount", employee.BankAccount);
                parameters.Add("@BankName", employee.BankName);
                parameters.Add("@Branch", employee.Branch);
                parameters.Add("@CreatedBy", employee.CreatedBy);
                parameters.Add("@CreatedDate", employee.CreatedDate);
                parameters.Add("@ModifiedBy", employee.ModifiedBy);
                parameters.Add("@ModifiedDate", employee.ModifiedDate);

                int numberOfAffectedRows = mySqlConnection.Execute(updateEmployeeCommand, parameters);

                /// Xử lý kq trả về từ DB
                if (numberOfAffectedRows > 0)
                {
                    return StatusCode(StatusCodes.Status201Created, employeeId);
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
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode(StatusCodes.Status400BadRequest, "e001");
            }


        }
        /// <summary>
        /// API xoá 1 nhân viên
        /// </summary>
        /// <param name="employeeID">ID của nhân viên muốn xoá</param>
        /// <returns>ID của nhân viên vừa xoá</returns>
        [HttpDelete]
        [Route("{employeeID}")]
        public IActionResult DeleteEmployee([FromRoute] Guid employeeID)
        {
            return StatusCode(StatusCodes.Status200OK, employeeID);
        }
    }
}
