$(document).ready(function () {
    let datatable = $("#role-privileges-table2").DataTable();
    let selectInput = $(".privilegeId-select2");

    $("#add-privilege-to-role-form").submit(function (e) {
        e.preventDefault();

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/role/add-privilege-to-role", "POST", form, function (err, data) {
            // console.log(err, data);
            selectInput.val(null).trigger("change");
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                datatable.ajax.reload();
                alert(data.message);
            }
        });
    });


});