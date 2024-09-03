$(document).ready(function () {
    let datatable = $("#office-users-table2").DataTable();
    let selectInput = $(".userId-select2");

    $("#add-user-to-office-form").submit(function (e) {
        e.preventDefault();

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/office/add-user-to-office", "POST", form, function (err, data) {
            // console.log(err, data);
            selectInput.val(null).trigger("change");
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                datatable.ajax.reload();
                alert(data.message);
                
                //reset form
                form.trigger("reset");
            }
        });
    });


});