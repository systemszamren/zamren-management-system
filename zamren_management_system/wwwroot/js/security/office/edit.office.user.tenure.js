$(document).ready(function () {
    $("#edit-office-user-tenure-form").submit(function (e) {
        e.preventDefault();

        let confirmation = confirm("Are you sure you want to update this user's office tenure?");
        if (!confirmation) {
            return;
        }

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/office/edit-user-office-tenure", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                window.location.href = "/Admin/Office/ManageUsers/" + data.officeId;
            }
        });
    });


});