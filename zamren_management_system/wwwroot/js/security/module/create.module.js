$(document).ready(function () {
    $("#create-module-form").submit(function (e) {
        e.preventDefault();

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/module/create", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                //redirect to /Admin/User
                window.location.href = "/Admin/Module";
            }
        });
    });

    $("#create-module-form input[name='name'], #create-module-form input[name='code']").on("change", function () {
        let val = $(this).val();
        if (val) {
            // val = val.trim().replace(/\s/g, "_");
            val = val.toUpperCase();
            $(this).val(val);
        }
    });

});