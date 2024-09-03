$(document).ready(function () {
    $(document).on('click', '.countModulePrivilegesBtn', function () {
        let btn = $(this)

        let id = $(this).val();
        let data = {
            moduleId: id,
        };

        ajaxDataRequest('/api/security/module/count-privileges-by-module-id', 'POST', data, btn, function (err, data) {
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                // console.log('Received data:', data);
                btn.text(data.totalCount);
            }
        });
    });
});
