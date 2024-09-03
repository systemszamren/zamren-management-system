$(document).ready(function () {

    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();
    let data = {
        id: id,
    };

    //init select2
    let selectInput = $(".userId-select2");
    let addUserToRoleBtn = $(".addUserToRoleBtn");

    selectInput.select2({
        minimumInputLength: 3,
        allowClear: true,
        ajax: {
            url: "/api/security/role/get-users-not-in-role-select2",
            type: "POST",
            dataType: "json",
            delay: 250,
            data: function (params) {
                // console.log(params);
                return {
                    q: params.term,
                    page: params.page,
                    roleId: id,
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                return {
                    results: data.results.map((user) => ({
                        id: user.id,
                        text: user.firstName + " " + user.lastName + " ( " + user.email + " )",
                        firstName: user.firstName,
                        lastName: user.lastName,
                    })),
                    pagination: {
                        more: params.page * 30 < data.totalCount,
                    },
                };
            },
            cache: true,
        },
        placeholder: "Search for a user",
        escapeMarkup: function (markup) {
            return markup;
        },
        templateResult: formatRepo,
        templateSelection: formatRepoSelection,

    }).on("select2:select", function (e) {
        let data = e.params.data;
        // console.log(data);
        let selectedUserId = data.id;
        let selectedUserFirstName = data.firstName;
        let selectedUserLastName = data.lastName;

        // Check if the user already has the role
        $.ajax({
            url: "/api/security/role/check-user-exists-in-role",
            type: "POST",
            data: {
                userId: selectedUserId,
                roleId: id,
            },
            success: function (response) {
                // console.log(response);
                if (response.success) {
                    if (response.exists) {
                        addUserToRoleBtn.prop("disabled", true);
                        selectInput.val(null).trigger("change");
                        alert("User (" + selectedUserFirstName + " " + selectedUserLastName + ") already exists in the role");
                    } else {
                        addUserToRoleBtn.prop("disabled", false);
                    }
                } else {
                    addUserToRoleBtn.prop("disabled", true);
                    alert(response.message);
                }
            }, error: function (xhr, status, error) {
                console.log(xhr, status, error);
                addUserToRoleBtn.prop("disabled", true);
            }
        });

        addUserToRoleBtn.prop("disabled", false);

    }).on("select2:unselect", function (e) {
        addUserToRoleBtn.prop("disabled", true);
    }).on("select2:open", function (e) {
        document.querySelector('.select2-search__field').focus();
    });
});

function formatRepo(repo) {
    if (repo.loading) {
        return repo.text;
    }
    return (
        "<div class='select2-result-repository clearfix'>" +
        "<div class='select2-result-repository__meta'>" +
        "<div class='select2-result-repository__title'>" +
        repo.text +
        "</div>"
    );
}

function formatRepoSelection(repo) {
    return repo.text;
}
