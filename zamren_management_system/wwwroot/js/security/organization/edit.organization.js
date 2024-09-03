$(document).ready(function () {
    $("#edit-organization-form").submit(function (e) {
        e.preventDefault();

        //confirm
        let confirmation = confirm("Are you sure you want to update this organization?");
        if (!confirmation) {
            return;
        }

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/organization/edit", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                window.location.href = "/Admin/Organization";
            }
        });
    });

    //on change of organization name check if input name="name" value has white space
    //If it has white space, replace it with underscore
    $("#edit-organization-form input[name='name']").on("change", function () {
        let name = $(this).val();
        if (!name) return;
        name = name.toUpperCase();
        name = name.trim();
        // name = name.replace(/\s+/g, '_');
        $(this).val(name);
    });

});