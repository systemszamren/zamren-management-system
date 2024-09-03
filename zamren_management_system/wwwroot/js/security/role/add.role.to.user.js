$(document).ready(function () {
    // let datatable = $("#user-roles-table").DataTable();
    let selectInput = $(".roleId-select2");

    $("#add-role-to-user-form").submit(function (e) {
        e.preventDefault();

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/role/add-role-to-user", "POST", form, function (err, data) {
            // console.log(err, data);
            selectInput.val(null).trigger("change");
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                userRolesDatatable.ajax.reload();
                alert(data.message);
                form.trigger("reset");
            }
        });
    });


});