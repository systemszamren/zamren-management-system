$(document).ready(function () {

    //get userId from url
    let url = new URL(window.location.href);
    let userId = url.pathname.split('/').pop();

    //init select2
    let selectInput = $(".roleId-select2");
    let addRoleToUserBtn = $(".addRoleToUserBtn");

    selectInput.select2({
        minimumInputLength: 0,
        allowClear: true,
        ajax: {
            url: "/api/security/role/get-roles-not-assigned-to-user-select2",
            type: "POST",
            dataType: "json",
            delay: 250,
            data: function (params) {
                // console.log(params);
                return {
                    q: params.term,
                    page: params.page,
                    userId: userId,
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                return {
                    results: data.results.map((role) => ({
                        id: role.id,
                        text: role.name
                    })),
                    pagination: {
                        more: params.page * 30 < data.totalCount,
                    },
                };
            },
            cache: true,
        },
        placeholder: "Search For Role",
        escapeMarkup: function (markup) {
            return markup;
        },
        templateResult: formatRepo,
        templateSelection: formatRepoSelection,

    }).on("select2:select", function (e) {
        let data = e.params.data;
        // console.log(data);
        let selectedRoleId = data.id;
        let roleName = data.name;

        // Check if the user already has the role
        $.ajax({
            url: "/api/security/role/check-role-assigned-to-user",
            type: "POST",
            data: {
                userId: userId,
                roleId: selectedRoleId,
            },
            success: function (response) {
                // console.log(response);
                if (response.success) {
                    if (response.assigned) {
                        addRoleToUserBtn.prop("disabled", true);
                        selectInput.val(null).trigger("change");
                        alert("Role (" + roleName + " ) already assigned to the user");
                    } else {
                        addRoleToUserBtn.prop("disabled", false);
                    }
                } else {
                    addRoleToUserBtn.prop("disabled", true);
                    alert(response.message);
                }
            }, error: function (xhr, status, error) {
                console.log(xhr, status, error);
                addRoleToUserBtn.prop("disabled", true);
            }
        });

        addRoleToUserBtn.prop("disabled", false);

    }).on("select2:unselect", function (e) {
        addRoleToUserBtn.prop("disabled", true);
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
