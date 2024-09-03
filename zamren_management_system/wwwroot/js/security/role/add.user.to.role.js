$(document).ready(function () {
    let datatable = $("#role-users-table2").DataTable();
    let selectInput = $(".userId-select2");

    $("#add-user-to-role-form").submit(function (e) {
        e.preventDefault();

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/role/add-user-to-role", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                
                if (data.success) {
                    selectInput.val(null).trigger("change");
                    form.trigger("reset");
                    datatable.ajax.reload();
                }
            }
        });
    });


});