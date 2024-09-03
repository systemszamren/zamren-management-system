$(document).ready(function () {

    window.getOfficeData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/security/office/get-office-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('input[name="name"]').val(data.office.name);
                $('input[name="id"]').val(data.office.id);
                $('input[name="currentOfficeId"]').val(data.office.id);
                $('textarea[name="description"]').val(data.office.description);
                $('.edit-office-link').attr('href', '/Admin/Office/Edit/' + data.office.id);
                $('.officeName').text(data.office.name);
            }
        });
    };

    // Call the method on page load
    getOfficeData();
});
