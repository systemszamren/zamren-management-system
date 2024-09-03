$(document).ready(function () {
    $("#edit-module-form").submit(function (e) {
        e.preventDefault();

        //confirm
        let confirmation = confirm("Are you sure you want to update this module?");
        if (!confirmation) {
            return;
        }

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/module/edit", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                window.location.href = "/Admin/Module";
            }
        });
    });

    //on change of module name check if input name="name" value has white space
    //If it has white space, replace it with underscore
    $("#edit-module-form input[name='name'], #edit-module-form input[name='code']").on("change", function () {
        let val = $(this).val();
        if (val) {
            // val = val.trim().replace(/\s/g, "_");
            val = val.toUpperCase();
            $(this).val(val);
        }
    });

});