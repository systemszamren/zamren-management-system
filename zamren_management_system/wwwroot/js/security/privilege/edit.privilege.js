$(document).ready(function () {
    $("#edit-privilege-form").submit(function (e) {
        e.preventDefault();

        //confirm
        let confirmation = confirm("Are you sure you want to update this privilege?");
        if (!confirmation) {
            return;
        }

        let form = $(this);

        ajaxFormSerializeSubmit("/api/security/privilege/edit", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                window.location.href = "/Admin/Privilege";
            }
        });
    });

    //on change of privilege name check if input name="name" value has white space
    //If it has white space, replace it with underscore
    $("#edit-privilege-form input[name='name']").on("change", function () {
        let name = $(this).val();
        if (!name) return;
        name = name.toUpperCase();
        name = name.trim();
        name = name.replace(/\s+/g, '_');
        $(this).val(name);
    });

});