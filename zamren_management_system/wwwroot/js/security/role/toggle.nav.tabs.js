$(document).ready(function () {
    $('a[data-bs-toggle="pill"]').on('shown.bs.tab', function (e) {
        let target = $(e.target).attr("href"); // activated tab
        if (target === '#pills-edit-role') {
            // console.log('#pills-edit-role tab activated');
            getRoleData();
        } else if (target === '#pills-role-users') {
            // console.log('#pills-role-users tab activated');
            getUsersInRole();
        } else if (target === '#pills-role-privileges') {
            // console.log('#pills-role-privileges tab activated');
            getPrivilegesInRoleData();
        }
    });
});