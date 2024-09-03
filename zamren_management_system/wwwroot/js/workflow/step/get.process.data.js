$(document).ready(function () {

    window.getProcessData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            processId: id,
        };

        ajaxDataRequest('/api/workflow/step/get-process-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('.edit-process-link, .back-btn').attr('href', '/Admin/Process/Edit/' + data.process.id);
                $('#processName').val(data.process.name + ' PROCESS');
                $('#processId').val(data.process.id);
                $('#moduleName').val(data.process.module.name);
            }
        });
    };

    getProcessData();
});