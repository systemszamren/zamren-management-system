$(document).ready(function () {
    $("#edit-user-form").submit(function (e) {
        e.preventDefault();

        let confirmation;
        
        let isEmployee = $("#isEmployee").is(":checked");
        if (isEmployee) {
            confirmation = confirm("Are you sure you want to update this Employee's account?");
        }else{
                confirmation = confirm("Are you sure you want to update this User's account?");
        }
        
        if (!confirmation) {
            return;
        }

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/user/edit-user", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                // window.location.href = "/Admin/User";
                location.reload();
            }
        });
    });

});