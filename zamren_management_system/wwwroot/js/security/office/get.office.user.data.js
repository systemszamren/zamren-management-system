$(document).ready(function () {

    window.getOfficeData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/security/office/get-office-user-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('input[name="user"]').val(data.userOffice.firstName + ' ' + data.userOffice.lastName + ' (' + data.userOffice.email + ')');
                $('input[name="id"]').val(data.userOffice.id);
                $('.edit-office-link').attr('href', '/Admin/Office/Edit/' + data.userOffice.officeId);
                $('.manage-office-users-link').attr('href', '/Admin/Office/ManageUsers/' + data.userOffice.officeId);
                $('.officeName').text(data.userOffice.officeName);

                let startDate = $('input[name="startDate"]');
                let endDate = $('input[name="endDate"]');

                startDate.datepicker({
                    todayBtn: "linked",
                    todayHighlight: true,
                    format: 'dd/mm/yyyy',
                    autoclose: true
                }).on('changeDate', function () {
                    endDate.datepicker("setStartDate", startDate.datepicker("getDate"));
                }).datepicker("update", convertDatetimeToDatePickerFormat(data.userOffice.startDate));

                endDate.datepicker({
                    todayHighlight: true,
                    format: 'dd/mm/yyyy',
                    autoclose: true
                }).datepicker("update", convertDatetimeToDatePickerFormat(data.userOffice.endDate));
            }
        });
    };

    // Call the method on page load
    getOfficeData();
});
