$(document).ready(function () {

    $("#create-process-step-form").submit(function (e) {
        e.preventDefault();

        if (!confirm("Are you sure you want to create this step?")) {
            return;
        }

        let form = $(this);

        ajaxFormSerializeSubmit("/api/workflow/step/create", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                window.location.href = "/Admin/Process/Edit/" + $('input[name="processId"]').val();
            }
        });
    });


});