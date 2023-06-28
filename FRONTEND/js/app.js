$(document).ready(function() {
    initEvents();

    loadData();
})

var employeeID = null
    // Tạo các sự kiện
function initEvents() {
    // Hiển thị bảng dialog
    $('#btn__add').click(function() {
        $('#dialog').show()

        $('#employeeCode')[0].focus()
    })

    // Ẩn bảng thông báo
    $('.btn__close').click(function() {
        $(this).parents('.dialog').hide()
    })

    // Hiển thị contextmenu
    $('#td--fix__icon').click(function() {
        $('#context-menu').show()
    })

    // checkbox
    var isCheckedAll = false;

    $('#check-all').click(function() {
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
            $(this).attr('title', 'Không đúng định dạng email')
        } else {
            $(this).removeClass('input--error')
            $(this).attr('title', 'Định dạng email chính xác')
        }
    })

    // Hiển thị dữ liệu khi click
    $(document).on('dblclick', 'table#tableEmployee tbody tr', function() {
        $('#dialog').show()
        $('#employeeCode')[0].focus()
            // Binding dữ liệu tương ứng với bản ghi vừa chọn
        let data = $(this).data('entity')
        employeeID = $(this).data('id')
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
    $('#btnSave').click(saveData)

    // Thêm mới dữ liệu
    var btnAdd = document.getElementById('btnAdd')
    btnAdd.addEventListener('click', function() {
        document.getElementById('dialog').style.display = 'block'
        $('input').val(null)
        $('textarea').val(null)

        $.ajax({
            type: 'POST',
            url: 'http://localhost:5033/api/Employees',
            success: function(newEmployeeCode) {
                $('#employeeCode').val(newEmployeeCode)
                $('#employeeCode').focus()
            }
        })
    })

    $('#btnAdd').click(function() {
        $('#dialog').show()
        $('#employeeCode')[0].focus()
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
    fetch('http://localhost:5033/api/Employees')
        .then(function(response) {
            return response.json()
        })
        .then(function(response) {
            $('table#tableEmployee tbody').empty()

            let ths = $("table#tableEmployee thead th")
            for (const emp of response) {
                //duyệt từng cột trong tiêu đề
                var trElement = $(`<tr></tr>`)
                for (const th of ths) {
                    // lấy ra propValue tương ứng với các cột
                    const propValue = $(th).attr("propValue")
                    const format = $(th).attr("format")
                        // Lấy giá trị tương ứng với tên của propValue trong đối tượng
                    let value = emp[propValue]
                    let classAlign = ''
                    switch (format) {
                        case "date":
                            value = formatDate(value)
                            classAlign = 'text-align-center'
                            break
                        case "checkbox":
                            value = '<input type="checkbox" name="" id=""></input>'
                            classAlign = 'text-align-center'
                            break
                        case "fix":
                            value = `<div class="td--wrap">
                                        <div class="td--fix">Sửa</div>
                                        <div class="td--fix__icon icon-sprites icon-14"></div>
                                    </div>`
                            classAlign = 'text-align-center border-right-none'
                            break
                        case "gender":
                            value = formatGender(value)
                            classAlign = 'text-align-center'
                            break
                        default:
                            classAlign = 'padding-left-8'
                            break
                    }

                    // tạo thHTML
                    let thHTML = `<td class="${classAlign}">${value || ''}</td>`
                        // Đẩy vào trElement
                    trElement.append(thHTML)
                }
                $(trElement).data("id", emp.employeeID)
                $(trElement).data("entity", emp)
                $('table#tableEmployee tbody').append(trElement)
            }
        })
        .catch(function(response) {

        })
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

// Save data
function saveData() {
    let inputs = $("#dialog input")
    var employee = {}
        // Build object
    for (const input of inputs) {
        const propValue = $(input).attr("propValue")
        if (propValue) {
            let value = input.value
            employee[propValue] = value
        }
    }
    $.ajax({
        type: 'PUT',
        url: 'http://localhost:5033/api/Employees/' + employeeID,
        data: JSON.stringify(employee),
        dataType: 'json',
        contentType: 'application/json',
        success: function(response) {
            loadData()
            $('#dialog').hide()
        }
    })
}