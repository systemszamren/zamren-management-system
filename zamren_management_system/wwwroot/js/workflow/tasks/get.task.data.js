$(document).ready(function () {

    window.getTaskData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            taskId: id,
        };

        ajaxDataRequest('/api/workflow/task/get-task-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('#taskId').val(data.task.id);
                $('#taskReference').val(data.task.reference);
                $('#parentTaskReference').val(data.task.parentTask ? data.task.parentTask.reference : '');
                $('#currentProcessName').val(data.task.currentProcess.name);
                $('#currentProcessStartDate').val(convertDatetimeToDate(data.task.currentProcessStartDate));
                $('#currentProcessEndDate').val(convertDatetimeToDate(data.task.currentProcessEndDate));
                $('#currentStepName').val(data.task.currentStep ? data.task.currentStep.name : '');
                $('#currentStepStartedDate').val(convertDatetimeToDate(data.task.currentStepStartedDate));
                $('#currentStepExpectedEndDate').val(convertDatetimeToDate(data.task.currentStepExpectedEndDate));
                if (data.task.currentStep.slaHours)
                    $('.stepSlaHours').text(data.task.currentStep.slaHours + ' Service Level Agreement Hours');
                $('#taskStartedByUser').val(data.task.taskStartedByUser.fullName);
                $('#currentActioningUser').val(data.task.currentActioningUser ? data.task.currentActioningUser.fullName : '');
                $('#previousActioningUser').val(data.task.previousActioningUser ? data.task.previousActioningUser.fullName : '');

                // Determine and update the alert type based on IsOpen
                let alertClass = data.task.IsOpen ? 'alert-info' : 'alert-success';
                let alertMessage = data.task.IsOpen ? 'This task is currently open.' : 'This task is closed.';
                $('.taskAlerts').html(`<div class="alert ${alertClass}" role="alert"><strong>Task Status:</strong> ${alertMessage}</div>`);
            }
        });
    };

    // Call the method on page-load
    getTaskData();

});