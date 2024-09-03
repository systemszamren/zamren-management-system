$(document).ready(function () {

    window.getOrganizationData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/security/organization/get-organization-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('input[name="name"]').val(data.organization.name);
                $('input[name="id"]').val(data.organization.id);
                $('textarea[name="description"]').val(data.organization.description);
                $('.edit-organization-link').attr('href', '/Admin/Organization/Edit/' + data.organization.id);
                $('.organizationName').text(data.organization.name);
            }
        });
    };

    // Call the method on page load
    getOrganizationData();
});
