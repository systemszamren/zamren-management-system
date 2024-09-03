$(document).ready(function () {
    $(document).on('click', '.countOrganizationBranchesBtn', function () {
        let btn = $(this)

        let id = $(this).val();
        let data = {
            organizationId: id,
        };

        ajaxDataRequest('/api/security/organization/count-branches-by-organization-id', 'POST', data, btn, function (err, data) {
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
