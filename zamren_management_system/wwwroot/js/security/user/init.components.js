$(document).ready(function () {
    window.userRolesDatatable = null;
    window.userPasswordHistoryDatatable = null;
    window.externalLoginsDatatable = null;
    window.twoFADatatable = null;
    
    //get userId from url and set name="userId" in the form
    let url = new URL(window.location.href);
    let userId = url.pathname.split('/').pop();
    $("input[name='userId']").val(userId);
});
