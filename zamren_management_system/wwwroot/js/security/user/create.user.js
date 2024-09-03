$(document).ready(function () {
    $("#register-new-user-form").submit(function (e) {
        e.preventDefault();


        let form = $(this);

        let isEmployee = form.find('input[name="isEmployee"]').is(":checked");
        if (isEmployee) {
            if (!confirm("Are you sure you want to create this EMPLOYEE account?")) return;
        } else {
            if (!confirm("Are you sure you want to create this User account?")) return;
        }


        ajaxFormSerializeSubmit("/api/security/user/create", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                //redirect to /Admin/User
                window.location.href = "/Admin/User";
            }
        });
    });


});