$(document).ready(function() {
    initEvents();

    loadData();
})

let limit = 10; //Số nhân viên mỗi trang cơ bản 
let offset = 1; //Số trang cơ bản
let start = 0; // Điểm bắt đầu của trang 
let end = limit; // Điểm kết thúc của trang
const totalPage = Math.ceil(200 / limit) // Tổng số trang
var formMode = 'add' //Chế độ xử lý tuỳ thuộc vào hành động
var checkbox = '<input type="checkbox" />' //checkbox
//dropdown menu
var dropdown = `<div class='dropdown text-align-center border-right-none'> 
                    <div class='drop-interact'> 
                        <div id='dropdown' class='dropdown-text'>Sửa</div> 
                            <div class="td--fix__icon icon-sprites icon-14"></div> 
                        </div>

                        <div id='dropdown-content' class='dropdown-content'> 
                            <a class='clone' href='#'>Nhân bản</a> 
                            <a class='delete' href='#'>Xóa</a>
                        </div>
                    </div>
                </div>`

// Tạo các sự kiện
function initEvents() {
    // Thực hiện việc lưu dữ liệu, load lại dữ liệu và hiển thị form thêm nhân viên
    $(document).on('click', '#btnAdd', function() {
        EmpsaveData()
        loadData()
        Add()
    })
    // Hiển thị bảng dialog
    $(document).on('click', '#btn__add', function() {
        formMode = 'add'
        $('#dialog').show()
        $('#dialog input')[2].focus()
        $('#dialog input').val(null)
        $('#dialog select').val(null)
        codes = $('table#tableEmployee tbody tr')
        newEmployeeCode = $('table#tableEmployee tbody tr').length
        if (newEmployeeCode >= 100) {
            $('#tableEmployee input')[2].value = `NV${newEmployeeCode}`
        }
        else {
            $('#tableEmployee input')[2].value = `NV0${newEmployeeCode}`
        }
        table = document.getElementById('tableEmployee')
        for (var i = 1, row; row = table.rows[i]; i++) {
            tableElement = row.cells[1].innerHTML
            if ($('#dialog input')[2].value == tableElement) {
                newEmployeeCode = newEmployeeCode + 1
                if (newEmployeeCode >= 100) {
                    $('#dialog input')[2].value = `NV${newEmployeeCode}`
                }
                else {
                    $('#dialog input')[2].value = `NV0${newEmployeeCode}`
                }
            }
        }
    });

    // Ẩn bảng thông báo
    $('.btn__close').click(function() {
        $(this).parents('.dialog').hide()
        $(this).parents('.toast-messenger').hide()
    })

    // checkbox
    var isCheckedAll = false;

    $('#select-input').click(function() {
        isCheckedAll = !isCheckedAll
        $('input[type="checkbox"]').prop('checked', isCheckedAll)
    });

    // Thông báo validate khi nhập thông tin
    $('input[nqminh]').blur(function() {
        var value = this.value
        if (!value) {
            $(this).addClass('input--error')
            $(this).attr('title', 'Thông tin không được để trống!')
        } else {
            $(this).removeClass('input--error')
            $(this).removeAttr('title')
        }
    })

    // Validate email
    $('input[type="email"]').blur(function() {
        var email = this.value
        var isEmail = checkEmailFormat(email)
        if (!isEmail) {
            $(this).addClass('input--error')
            $(this).attr('title', 'Sai rồi!!!')
        } else {
            $(this).removeClass('input--error')
            $(this).attr('title', 'Định dạng email chính xác')
        }
    })

    // Hiển thị dữ liệu khi dbclick
    $(document).on('dblclick', 'table#tableEmployee tbody tr', function() {
        $('#dialog').show()
        $('#employeeCode')[0].focus()
            // Binding dữ liệu tương ứng với bản ghi vừa chọn
        let data = $(this).data('entity')
        employeeID = $(this).data('code')
            // Duyệt tất cả dữu liệu
        let inputs = $("#dialog input")
        for (const input of inputs) {
            const propValue = $(input).attr("propValue")
            let value = data[propValue]
            input.value = value
        }
    })

    // Hightlight
    $(document).on('click', 'table#tableEmployee tbody tr', function() {
        $(this).siblings().removeClass('row-selected')
        this.classList.add('row-selected')
    })

    // btnSave
    $(document).on('click', '#btnSave', function() {
        EmpsaveData()
    })

    //Function Mở ra dropdown menu
    $(document).on('click', '.dropdown-text', function () {
        $(this).parents(".dropdown").children(".dropdown-content").toggle()
    })

    //Hiển thị tổng bản ghi
    document.getElementById('totalPage').innerHTML = `Tổng số: <div class = 'bold'>${$("table#tableEmployee tbody tr").length}</div> bản ghi`

    //Load lại trang khi ấn nút refesh
    $(document).on('click', '#btnRefresh', function() {
        location.reload()
    })

    //Chọn ra dữ liệu trong 1 trang
    $(document).on('change', '#pagination__combobox', function(){
        value = document.getElementById('pagination__combobox').value
        limit = parseInt(value)
        getCurrentPage()
        Pagination()
    })

    //Phân trang dữ liệu
    $(document).on('click', '#1-page', function() {
        offset = 1
        getCurrentPage(offset)
        Pagination()
    })
    $(document).on('click', '#2-page', function() {
        offset = 2
        getCurrentPage(offset)
        Pagination()
    })
    $(document).on('click', '#3-page', function() {
        offset = 3
        getCurrentPage(offset)
        Pagination()
    })
    $(document).on('click', '#4-page', function() {
        offset = 4
        getCurrentPage(offset)
        Pagination()
    })
    $(document).on('click', '#5-page', function() {
        offset = 5
        getCurrentPage(offset)
        Pagination()
    })
    //Tiến-lùi giữa các trang
    $(document).on('click', '#skip-forward', function() {
        offset++
        if (offset > totalPage) {
            offset = totalPage
        }
        getCurrentPage(offset)
        Pagination()
    })
    $(document).on('click', '#skip-back', function() {
        offset--
        if (offset <=1) {
            offset = 1
        }
        getCurrentPage(offset)
        Pagination()
    })

    //Tìm kiếm dữ liệu dừa vào mã nhân viên/tên/Phòng của nhân viên đó
    searchInput = document.querySelector('[data-search]')
    searchInput.addEventListener('input', function(e) {
        //Lấy ra giá trị input vào thanh tìm kiếm
        const value = e.target.value.toLowerCase()
        console.log(value)
        // Lọc qua các gí trị cần khớp qua các dòng dữ liệu
        table = document.getElementById('tableEmployee')
        for (var i = 1, row; row = table.rows[i]; i++) {
            tableCode = row.cells[1].innerHTML
            tableName = row.cells[2].innerHTML
            tableDept = row.cells[7].innerHTML
            //Ẩn đi dữ liệu không trùng khớp
            if (!tableCode.toLowerCase().includes(value)) {
                row.hidden = false
                if (!tableName.toLowerCase().includes(value)) {
                    row.hidden = false
                    if (!tableDept.toLowerCase().includes(value)) {
                        row.hidden = true
                    }
                    else {
                        row.hidden = false
                    }
                }
            }
            else {
                row.hidden = false
            }
        }
    })

    //Xoá dữ liệu
    $(document).on('click','.delete', function() {
        var selector = $(this).parents('table#tableEmployee tbody tr')
        var empCode = selector.data('code')
        $('#dlgDel').show()
        document.getElementById('empCode').innerHTML = `Bạn có muốn xoá nhân viên ${empCode} không ?`
        document.getElementById('del-agree').addEventListener('click', function() {
            $.ajax({
                url: 'http://localhost:54862/api/Employees/' + empCode,
                type: 'DELETE',
                success: function(res) {
                   console.log('Đã xoá thành công nhân viên')
                   $('#dglDel').hide()
                   selector.remove()
                }
            })
        })
    })

    //Nhân bản dữ liệu và chỉnh sửa mã nhân viên
    $(document).on('click','.clone', function () {
        $('#dialog').show()
        $('#dialog input')[2].focus()
        let data = $(this).parents('table#tableEmployee tbody tr').data('entity')
        let inputs = $("#dialog input, #dialog select")
        let value = null
        for (input of inputs) {
            const propValue = $(input).attr("propValue")
            if (propValue == 'employeeCode') {
                value = data[propValue]
                table = document.getElementById('tableEmployee')
                for (var i = 1, row; row = table.rows[i]; i++) {
                    tableElement = row.cells[1].innerHTML
                    if (value < tableElement) {
                        input.value = tableElement
                        break
                    }
                }
            }
            else {
                value = data[propValue]
                input.value = value
            }
        }
    })

    
}

// React check email
function checkEmailFormat(email) {
    const re =
        /^(([^<>()[\]\.,;:\s@\"]+(\.[^<>()[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;
    return email.match(re);
}

// Load dữ liệu 
function loadData() {
    $.ajax({
        type: 'GET',
        async: false,
        url: 'http://localhost:54862/api/Employees',
        success: function(res) {
            //Làm rỗng table
            $('table#tableEmployee tbody').empty()
            //Lấy các header của table
            let ths = $("table#tableEmployee thead th")
            //Lấy các dữ liệu được tải về từ URL trên
            res.map(function(user, index) {
                //Template cho các thành phần trong table body
                var trHTML = $(`<tr></tr>`)
                for (const th of ths) {
                    //Lấy ra các giá trị propValue ở trong các header của table
                    const propValue = $(th).attr("propValue")
                    //Lấy ra các giá trị format ở trong các header của table
                    const format = $(th).attr("format")
                    let value = null
                    let classAlign = ''
                    //Gán giá trị tương ứng với các propValue
                    if (propValue =='sort') {
                        value = checkbox
                    }
                    else if (propValue == 'function') {
                        value = dropdown
                    }
                    else {
                        value = user[propValue]
                    }
                    //Đổi format dựa tương ứng với các format được lấy ở trên
                    switch (format) {
                        case "date":
                            value = formatDate(value)
                            classAlign = 'text-align-center'
                            break
                        case "checkbox":
                            value = '<input type="checkbox" name="" id=""></input>'
                            classAlign = 'text-align-center'
                            break
                        case "gender":
                            value = formatGender(value)
                            classAlign = 'text-align-center'
                            break
                        case "function":
                                value = dropdown
                                classAlign = 'text-align-center border-right-none'
                            break
                        default:
                            classAlign = 'padding-left-8'
                            break
                    }
                    //Đưa các giá trị vừa lấy ra ở trên và đưa vào thành phần trong body
                    let tdHTML = `<td class="${classAlign}">${value || ''}</td>`
                    trHTML.append(tdHTML)
                }
                //Gán giá trị của các thành phần của các dữ liệu ở trên vào các biến cụ thể
                $(trHTML).data("code", user.employeeCode)
                $(trHTML).data("entity", user)
                $('table#tableEmployee tbody').append(trHTML)
            })
        }
    })
}

//Return giá trị phân trang
function getCurrentPage() {
    start = (offset - 1) * limit
    end = offset * limit
}

//Function phân trang
function Pagination() {
    $.ajax({
        type: 'GET',
        url: 'http://localhost:54862/api/Employees',
        async: false,
        success: function(res) {
            $('table#tableEmployee tbody').empty();
            let ths = $('table#tableEmployee thead th');
            users = res.map(function(user, index) {
                //Giới hạn số lượng dữ liệu lấy ra từ url ở trên dựa vào điểm bắt đầu và kết thúc của 1 trang
                if(index >= start && index < end) {
                    var trHTML = $('<tr></tr>')
                for (const th of ths) {
                    //Lấy ra các giá trị propValue ở trong các header của table
                    const propValue = $(th).attr("propValue")
                    //Lấy ra các giá trị format ở trong các header của table
                    const format = $(th).attr("format")
                    let value = null
                    let classAlign = ''
                    //Gán giá trị tương ứng với các propValue
                    if (propValue =='sort') {
                        value = checkbox
                    }
                    else if (propValue == 'function') {
                        value = dropdown
                    }
                    else {
                        value = user[propValue]
                    }
                    //Đổi format dựa tương ứng với các format được lấy ở trên
                    switch (format) {
                        case "date":
                            value = formatDate(value)
                            classAlign = 'text-align-center'
                            break
                        case "checkbox":
                            value = '<input type="checkbox" name="" id=""></input>'
                            classAlign = 'text-align-center'
                            break
                        case "gender":
                            value = formatGender(value)
                            classAlign = 'text-align-center'
                            break
                        default:
                            classAlign = 'padding-left-8'
                            break
                    }
                    //Đưa các giá trị vừa lấy ra ở trên và đưa vào thành phần trong body
                    let thHTML = `<td class="${classAlign}">${value || ''}</td>`
                    trHTML.append(thHTML)
                }
                //Gán giá trị của các thành phần của các dữ liệu ở trên vào các biến cụ thể
                $(trHTML).data("code", user.employeeCode)
                $(trHTML).data("entity", user)
                $('table#tableEmployee tbody').append(trHTML)
                }
            })
        }
    })
}

//Function lưu dữ liệu vào bộ dữ liệu
function EmpsaveData() {
    //Lấy các thông tin trong những input
    let inputs = $('#dialog input, #dialog select')
    //Tạo một tuple rỗng
    var employee = {}
    //Chuẩn bị gí trị để gán
    var employeeCode = null
    for (const input of inputs) {
        const propValue = $(input).attr('propValue')
        if (propValue = 'employeeCode') {
            let value = input.value
            employeeCode = value
            employee[propValue] = value
            table = document.getElementById('tableEmployee')
            //Đặt các giá trị vừa lấy vào các điều kiện để chuẩn bị nhập vào bộ dữ liệu
            //Không đáp ứng được điều kiện sẽ bị nhảy ra ngoài và hiện thông báo
            if (!value) {
                $(input).addClass('input--error')
                $(input).attr('title', 'Dữ liệu không chính xác. Yêu cầu nhập lại !')
                document.getElementById('toast-error__content').innerHTML = `${document.getElementById('emp-input').innerHTML} không được để trống`
                $('#dlgError').show()
                break
            }
            else if (formMode == 'edit') {
                let value = input.value
                employeeCode = value
                employee[propValue] = value
            }
            else {
                for (var i = 1, row; row = table.rows[i]; i++) {
                    tableElement = row.cells[i].innerHTML
                    if (value == tableElement) {
                        $(input).addClass('input--error')
                        $(input).attr('title', 'Dữ liệu không chính xác. Yêu cầu nhập lại !')
                        document.getElementById('toast-error__content').innerHTML = `${document.getElementById('emp-input').innerHTML} không được để trùng`
                        $('#dlgError').show()
                        break
                    }
                }
            }
        }
        else if (propValue == 'employeeName') {
            let value = input.value
            employee[propValue] = value
            if (!value) {
                $(input).addClass('input--error')
                $(input).attr('title', 'Dữ liệu không chính xác. Yêu cầu nhập lại !')
                document.getElementById('toast-error__content').innerHTML = `${document.getElementById('emp-input').innerHTML} không được để trống`
                $('#dlgError').show()
                break
            }
        }
        else if (propValue == 'dateOfBirth') {
            //Chuyển đổi giá trị datr được format ở trên để phù hợp với dữ liệu đưa vào và yêu cầu của new date
            let value = input.value
            date = value.replace(/(..).(..).(....)/, "$2/$1/$3")
            Datedate = new Date(date)
            datejson = Datedate.toJSON()
            employee[propValue] = datejson;
            curr = new Date()
            if (!value) {
                $(input).addClass('input--error')
                $(input).attr('title', 'Dữ liệu không chính xác. Yêu cầu nhập lại !')
                document.getElementById('toast-error__content').innerHTML = `${document.getElementById('birth-input').innerHTML} không được để trống`
                $('#dlgError').show()
                break
            }
            else if (Datedate > curr) {
                $(input).addClass('input--error')
                $(input).attr('title', 'Dữ liệu không chính xác. Yêu cầu nhập lại!')
                document.getElementById('toast-error__content').innerHTML = `${document.getElementById('birth-input').innerHTML} không hợp lệ`
                $('#dlgError').show()
                break
            }
        }
        else if (propValue == 'positionName') {
            let value = input.value;
            employee[propValue] = value;
            if (!value) {
                $(input).addClass('input--error')
                $(input).attr('title', 'Dữ liệu không chính xác. Yêu cầu nhập lại!')
                document.getElementById('toast-error__content').innerHTML = `${document.getElementById('pos-input').innerHTML} không được để trống`
                $("#dlgError").show()
                break
            }
        }
    }

    //Nhập dữ liệu nếu formMode thoả mãn điều kiện
    if (formMode == 'edit') {
        $.ajax({
            type: 'PUT',
            url: 'http://localhost:54862/api/Employees/' + employee.employeeCode,
            data: JSON.stringify(employee),
            dataType: 'json',
            contentType: 'application/json',
            success: function (res) {
                $('#dialog').hide()
                loadData()
            }
        })
    }
    else {
        $.ajax({
            type: 'POST',
            url: 'http://localhost:54862/api/Employees',
            data: JSON.stringify(employee),
            dataType: 'json',
            contentType: 'application/json',
            success: function (res) {
                $('#dialog').hide()
                loadData()
            }
        })
    }
}

//Function hiển thị và format form thêm nhân viên
function Add() {
    //Gán sự kiện mới vào formMode
    formMode = 'add'
    //Hiển thị form thêm nhân viên và đặt điểm khởi đầu ở input đầu tiên
    $('#dialog').show()
    $('#dialog input')[2].focus()
    //Clear hét giá trị của form từ những lần thêm mới/sửa trước
    $('#dialog input').val(null)
    $('#dialog select').val(null)
    codes = $('table#tableEmployee tbody tr')
    //Lấy độ dài của dữ liệu
    newEmployeeCode = $('table#tableEmployee tbody tr').length
    //Gán giá trị cho input dựa vào độ dài của bộ dữ liệu
    if (newEmployeeCode >= 100) {
        $('#dialog input')[2].value = `NV${newEmployeeCode}`
    }
    else {
        $('#dialog input')[2].value = `NV0${newEmployeeCode}`
    }
    //Lọc qua từng giá trị mã nhân viên của bộ dữ liệu để đảm bảo input có giá trị lớn nhất
    table = document.getElementById('tableEmployee')
    for (var i = 1, row; row = table.rows[i]; i++) {
        tableElement = row.cells[1].innerHTML
        if ($('#dialog input')[2].value = tableElement) {
            newEmployeeCode = newEmployeeCode + 1
            if (newEmployeeCode >= 100) {
                $('#dialog input')[2].value = `NV${newEmployeeCode}`
            }
            else {
                $('#dialog input')[2].value = `NV0${newEmployeeCode}`
            }
        }
    }
}

// Định dạng ngày sinh
function formatDate(date) {
    try {
        if (date) {
            date = new Date(date)
            dateValue = date.getDate()
            dateValue = dateValue < 10 ? `0${dateValue}` : dateValue
            let monthValue = date.getMonth() + 1
            monthValue = monthValue < 10 ? `0${monthValue}` : monthValue
            let yearValue = date.getFullYear()
            return `${dateValue}/${monthValue}/${yearValue}`
        } else {
            return ''
        }
    } catch (error) {
        console.log(error)
    }
}

// Định dạng giới tính
function formatGender(num) {
    switch (num) {
        case 1:
            return "Nữ";
        case 2:
            return "Nam";
        default:
            return "";
    }
}

