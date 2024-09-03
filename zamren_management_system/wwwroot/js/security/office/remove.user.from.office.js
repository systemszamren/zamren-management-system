$(document).ready(function () {
    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();

    let datatable = $("#office-users-table2").DataTable();

    // Handle delete button click event
    $('#office-users-table2 tbody').on('click', '.removeUserFromOfficeBtn', function () {

        let btn = $(this);
        let firstName = btn.data('fname');
        let lastName = btn.data('lname');

        //confirm
        let confirmation = confirm("Are you sure you want to remove " + firstName + " " + lastName + " from this office?");
        if (!confirmation) {
            return;
        }

        let userId = btn.data('userid');
        let data = {
            userId: userId,
            officeId: id,
        };

        ajaxDataRequest("/api/security/office/remove-user-from-office", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                datatable.ajax.reload();
            }
        });

    });

});