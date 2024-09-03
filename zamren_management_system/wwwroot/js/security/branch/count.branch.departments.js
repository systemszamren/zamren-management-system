$(document).ready(function () {
    $(document).on('click', '.countBranchDepartmentsBtn', function () {
        let btn = $(this)

        let id = $(this).val();
        let data = {
            branchId: id,
        };

        ajaxDataRequest('/api/security/branch/count-departments-by-branch-id', 'POST', data, btn, function (err, data) {
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
