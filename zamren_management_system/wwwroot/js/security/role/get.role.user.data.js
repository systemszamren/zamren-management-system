$(document).ready(function () {

    window.getRoleData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/security/role/get-role-user-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('input[name="user"]').val(data.userRole.firstName + ' ' + data.userRole.lastName + ' (' + data.userRole.email + ')');
                $('input[name="id"]').val(data.userRole.uniqueId);
                $('.edit-role-link').attr('href', '/Admin/Role/Edit/' + data.userRole.roleId);
                $('.manage-role-users-link').attr('href', '/Admin/Role/ManageUsers/' + data.userRole.roleId);
                $('.roleName').text(data.userRole.roleName);

                let startDate = $('input[name="startDate"]');
                let endDate = $('input[name="endDate"]');

                startDate.datepicker({
                    todayBtn: "linked",
                    todayHighlight: true,
                    format: 'dd/mm/yyyy',
                    autoclose: true
                }).on('changeDate', function () {
                    endDate.datepicker("setStartDate", startDate.datepicker("getDate"));
                }).datepicker("update", convertDatetimeToDatePickerFormat(data.userRole.startDate));

                endDate.datepicker({
                    todayHighlight: true,
                    format: 'dd/mm/yyyy',
                    autoclose: true
                }).datepicker("update", convertDatetimeToDatePickerFormat(data.userRole.endDate));
            }
        });
    };

    // Call the method on page load
    getRoleData();
});
