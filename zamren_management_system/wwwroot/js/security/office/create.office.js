$(document).ready(function () {
    $("#create-office-form").submit(function (e) {
        e.preventDefault();

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/office/create", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                //redirect to /Admin/User
                window.location.href = "/Admin/Office";
            }
        });
    });

    $("#create-office-form input[name='name']").on("change", function () {
        let name = $(this).val();
        if (!name) return;
        name = name.toUpperCase();
        name = name.trim();
        // name = name.replace(/\s+/g, '_');
        $(this).val(name);
    });

});