$(document).ready(function () {
    $('a[data-bs-toggle="pill"]').on('shown.bs.tab', function (e) {
        let target = $(e.target).attr("href"); // activated tab
        if (target === '#pills-profile') {
            getUserData();
        } else if (target === '#pills-user-details') {
            getUserDetails();
        } else if (target === '#pills-roles') {
            getUserRoles();
        } else if (target === '#pills-password') {
            getUserPasswordHistory();
        } else if (target === '#pills-external-logins') {
            getExternalLogins();
        } else if (target === '#pills-2fa') {
            get2FAStatus();
        } else if (target === '#pills-account-settings') {
        }
    });
});