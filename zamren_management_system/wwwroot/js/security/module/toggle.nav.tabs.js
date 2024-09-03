$(document).ready(function () {
    $('a[data-bs-toggle="pill"]').on('shown.bs.tab', function (e) {
        let target = $(e.target).attr("href"); // activated tab
        if (target === '#pills-edit-module') {
            // console.log('#pills-edit-role tab activated');
            getModuleData();
        } else if (target === '#pills-module-privileges') {
            // console.log('#pills-role-privileges tab activated');
            getPrivilegesInModuleData();
        }
    });
});