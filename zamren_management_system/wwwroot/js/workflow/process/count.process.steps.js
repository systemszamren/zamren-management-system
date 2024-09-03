$(document).ready(function () {
    $(document).on('click', '.countProcessStepsBtn', function () {
        let btn = $(this)

        let id = $(this).val();
        let data = {
            processId: id,
        };

        ajaxDataRequest('/api/workflow/process/count-steps-by-process-id', 'POST', data, btn, function (err, data) {
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
