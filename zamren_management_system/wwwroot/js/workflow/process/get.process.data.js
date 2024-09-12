$(document).ready(function () {

    window.getProcessData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/workflow/process/get-process-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('.processName').text(data.process.name);
                $('input[name="name"]').val(data.process.name);
                $('input[name="id"]').val(data.process.id);
                $('textarea[name="description"]').val(data.process.description);
                populateSelectElement(data.modules, 'moduleId', '- Select Module -', data.process.module.plainId, 'plainId', 'name');
                // $('.edit-process-link').attr('href', '/Admin/Process/Edit/' + data.process.id);
                // $('.processName').text(data.process.name + ' PROCESS');
            }
        });
    };

    // Call the method on page-load
    getProcessData();

});