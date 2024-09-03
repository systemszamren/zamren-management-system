$(document).ready(function () {
    $("#edit-role-form").submit(function (e) {
        e.preventDefault();

        //confirm
        let confirmation = confirm("Are you sure you want to update this role?");
        if (!confirmation) {
            return;
        }

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/role/edit", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                // window.location.href = "/Admin/Role/Edit/" + data.id;
                //reload the page
                location.reload();
            }
        });
    });

    $("#edit-role-form input[name='name']").on("change", function () {
        let name = $(this).val();
        if (!name) return;
        name = name.toUpperCase();
        name = name.trim();
        // name = name.replace(/\s+/g, '_');
        $(this).val(name);
    });

});