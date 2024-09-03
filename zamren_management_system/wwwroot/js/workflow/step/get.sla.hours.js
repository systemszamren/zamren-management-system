$(document).ready(function () {

    ajaxDataRequest('/api/workflow/step/get-sla-hours', 'POST', null, null, function (err, data) {
        // console.log(err, data);
        if (err) {
            // Handle error
            console.error('An error occurred:', err);
        } else {
            // Handle success
            populateSelectElement(data.slaHours, 'slaHours', '- Select SLA Hours -', null, 'hour', 'description');
        }
    });
});