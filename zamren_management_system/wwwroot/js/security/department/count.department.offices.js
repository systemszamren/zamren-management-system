$(document).ready(function () {
    $(document).on('click', '.countDepartmentOfficesBtn', function () {
        let btn = $(this)

        let id = $(this).val();
        let data = {
            departmentId: id,
        };

        ajaxDataRequest('/api/security/department/count-offices-by-department-id', 'POST', data, btn, function (err, data) {
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                // console.log('Received data:', data);
                btn.text(data.totalCount);
            }
        });
    });
});
