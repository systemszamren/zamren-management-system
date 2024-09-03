$(document).ready(function () {

    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();
    let data = {
        processId: id,
    };

    ajaxDataRequest('/api/workflow/step/get-privileges', 'POST', data, null, function (err, data) {
        // console.log(err, data);
        if (err) {
            // Handle error
            console.error('An error occurred:', err);
        } else {
            // Handle success
            populateSelectElement(data.privileges, 'privilegeId', '- Select Privilege -', null, 'id', 'name');
        }
    });
});