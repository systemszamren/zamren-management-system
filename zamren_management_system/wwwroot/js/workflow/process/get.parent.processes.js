$(document).ready(function () {
    $('#isChildProcess').change(function () {
        if ($(this).is(':checked')) {
            $('#parentProcessGroup').show();

            // Get moduleId from selected module in select element
            let moduleId = $('#moduleId').val();

            let data = {
                moduleId: moduleId,
            };

            ajaxDataRequest('/api/workflow/process/get-parent-processes-in-module', 'POST', data, null, function (err, data) {
                if (err) {
                    // Handle error
                    console.error('An error occurred:', err);
                } else {
                    // Handle success
                    populateSelectElement(data.processes, 'parentProcessId', '- Select Process -', null, 'id', 'name');
                }
            });
        } else {
            $('#parentProcessGroup').hide();
        }
    });

    $('#moduleId').change(function () {
        $('#isChildProcess').prop('checked', false);
        $('#parentProcessId').empty();
        $('#parentProcessGroup').hide();
    });
});