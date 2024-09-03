$(document).ready(function () {

    ajaxDataRequest('/api/workflow/step/get-offices', 'POST', null, null, function (err, data) {
        // console.log(err, data);
        if (err) {
            // Handle error
            console.error('An error occurred:', err);
        } else {
            // Handle success
            populateSelectElement(data.offices, 'officeId', '- Select Office -', null, 'id', 'name');
        }
    });
});