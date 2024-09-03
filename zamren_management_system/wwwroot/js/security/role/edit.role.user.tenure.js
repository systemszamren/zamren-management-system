$(document).ready(function () {
    $("#edit-role-user-tenure-form").submit(function (e) {
        e.preventDefault();

        let confirmation = confirm("Are you sure you want to update this user's role tenure?");
        if (!confirmation) {
            return;
        }

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/role/edit-user-role-tenure", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);

                //check if url contains '?ReturnUrl=true': invoke window.history.back() to go back to previous page
                if (window.location.href.indexOf('?ReturnUrl=true') > -1) {
                    window.history.back();
                } else {
                    window.location.href = "/Admin/Role/ManageUsers/" + data.roleId;
                }
            }
        });
    });


});