$(document).ready(function () {

    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();

    //init select2
    let selectInput = $("#actioningUserId");

    selectInput.select2({
        minimumInputLength: 3,
        width: '100%',
        allowClear: true,
        ajax: {
            url: "/api/workflow/step/get-employees-select2",
            type: "POST",
            dataType: "json",
            delay: 250,
            data: function (params) {
                // console.log(params);
                return {
                    q: params.term,
                    page: params.page,
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;

                return {
                    results: data.results.map((user) => ({
                        id: user.id,
                        text: user.firstName + " " + user.lastName + " (" + user.email + ")",
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
        $('#roleId').prop('disabled', true).val(null);
        $('#officeId').prop('disabled', true).val(null);
        $('#departmentId').prop('disabled', true).val(null);
        $('#branchId').prop('disabled', true).val(null);
        $('#organizationId').prop('disabled', true).val(null);

    }).on("select2:unselect", function (e) {
        $('#roleId').prop('disabled', false);
        // $('#officeId').prop('disabled', false);
        // $('#departmentId').prop('disabled', false);
        // $('#branchId').prop('disabled', false);
        $('#organizationId').prop('disabled', false);

    }).on("select2:open", function () {
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
