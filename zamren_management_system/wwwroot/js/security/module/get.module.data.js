$(document).ready(function () {

    window.getModuleData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/security/module/get-module-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('input[name="name"]').val(data.module.name);
                $('.moduleName').text(data.module.name);
                $('input[name="id"]').val(data.module.id);
                $('input[name="currentModuleId"]').val(data.module.id);
                $('input[name="code"]').val(data.module.code);
                $('textarea[name="description"]').val(data.module.description);
                $('.edit-module-link').attr('href', '/Admin/Module/Edit/' + data.module.id);
            }
        });
    };

    // Call the method on page load
    getModuleData();
});
