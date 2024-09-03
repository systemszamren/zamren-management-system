$(document).ready(function () {

    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();
    let data = {
        processId: id,
    };

    ajaxDataRequest('/api/workflow/step/get-processes-except', 'POST', data, null, function (err, data) {
        // console.log(err, data);
        if (err) {
            // Handle error
            console.error('An error occurred:', err);
        } else {
            // Handle success
            // populateSelectElement(data.processes, 'nextProcessId', '- Select Next Process -', null, 'id', 'name');
        
            // populateSelectElement(data.processes, 'prevProcessId', '- Select Previous Process -', null, 'id', 'name');
        }
    });
});