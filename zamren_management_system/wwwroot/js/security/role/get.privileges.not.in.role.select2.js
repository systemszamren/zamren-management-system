$(document).ready(function () {

    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();

    //init select2
    let selectInput = $(".privilegeId-select2");
    let addPrivilegeToRoleBtn = $(".addPrivilegeToRoleBtn");

    selectInput.select2({
        minimumInputLength: 0,
        allowClear: true,
        ajax: {
            url: "/api/security/role/get-privileges-not-in-role-select2",
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
                    results: data.results.map((privilege) => ({
                        id: privilege.id,
                        text: privilege.name
                    })),
                    pagination: {
                        more: params.page * 30 < data.totalCount,
                    },
                };
            },
            cache: true,
        },
        placeholder: "Search for a privilege",
        escapeMarkup: function (markup) {
            return markup;
        },
        templateResult: formatRepo,
        templateSelection: formatRepoSelection,

    }).on("select2:select", function (e) {
        let data = e.params.data;
        // console.log(data);
        let selectedPrivilegeId = data.id;
        let privilegeName = data.name;

        // Check if the privileges already has the role
        $.ajax({
            url: "/api/security/role/check-privilege-exists-in-role",
            type: "POST",
            data: {
                privilegeId: selectedPrivilegeId,
                roleId: id,
            },
            success: function (response) {
                // console.log(response);
                if (response.success) {
                    if (response.exists) {
                        addPrivilegeToRoleBtn.prop("disabled", true);
                        selectInput.val(null).trigger("change");
                        alert("Privilege (" + privilegeName + ") already exists in the role");
                    } else {
                        addPrivilegeToRoleBtn.prop("disabled", false);
                    }
                } else {
                    addPrivilegeToRoleBtn.prop("disabled", true);
                    alert(response.message);
                }
            }, error: function (xhr, status, error) {
                console.log(xhr, status, error);
                addPrivilegeToRoleBtn.prop("disabled", true);
            }
        });

        addPrivilegeToRoleBtn.prop("disabled", false);

    }).on("select2:unselect", function (e) {
        addPrivilegeToRoleBtn.prop("disabled", true);
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
