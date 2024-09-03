$(document).ready(function () {

    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();

    //init select2
    let selectInput = $(".privilegeId-select2");
    let addPrivilegeToModuleBtn = $(".addPrivilegeToModuleBtn");

    selectInput.select2({
        minimumInputLength: 0,
        allowClear: true,
        ajax: {
            url: "/api/security/module/get-privileges-not-in-module-select2",
            type: "POST",
            dataType: "json",
            delay: 250,
            data: function (params) {
                // console.log(params);
                return {
                    q: params.term,
                    page: params.page,
                    moduleId: id,
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

        // Check if the privileges already has the module
        $.ajax({
            url: "/api/security/module/check-privilege-exists-in-module",
            type: "POST",
            data: {
                privilegeId: selectedPrivilegeId,
                moduleId: id,
            },
            success: function (response) {
                // console.log(response);
                if (response.success) {
                    if (response.exists) {
                        addPrivilegeToModuleBtn.prop("disabled", true);
                        selectInput.val(null).trigger("change");
                        alert("Privilege (" + privilegeName + ") already exists in the module");
                    } else {
                        addPrivilegeToModuleBtn.prop("disabled", false);
                    }
                } else {
                    addPrivilegeToModuleBtn.prop("disabled", true);
                    alert(response.message);
                }
            }, error: function (xhr, status, error) {
                console.log(xhr, status, error);
                addPrivilegeToModuleBtn.prop("disabled", true);
            }
        });

        addPrivilegeToModuleBtn.prop("disabled", false);

    }).on("select2:unselect", function (e) {
        addPrivilegeToModuleBtn.prop("disabled", true);
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
