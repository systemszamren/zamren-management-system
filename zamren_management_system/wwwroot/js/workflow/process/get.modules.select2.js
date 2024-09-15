$(document).ready(function () {

    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();

    //init select2
    let selectInput = $(".moduleId-select2");

    selectInput.select2({
        minimumInputLength: 0,
        allowClear: true,
        ajax: {
            url: "/api/workflow/process/get-modules-select2",
            type: "POST",
            dataType: "json",
            delay: 250,
            data: function (params) {
                // console.log(params);
                return {
                    q: params.term,
                    page: params.page,
                    processId: id,
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;
                // console.log(data);

                return {
                    results: data.results.map((module) => ({
                        id: module.id,
                        text: module.name
                    })),
                    pagination: {
                        more: params.page * 30 < data.totalCount,
                    },
                };
            },
            cache: true,
        },
        placeholder: "Select Module",
        escapeMarkup: function (markup) {
            return markup;
        },
        templateResult: formatRepo,
        templateSelection: formatRepoSelection

    }).on("select2:select", function (e) {
        $('#isChildProcess').prop('disabled', false).prop('checked', false);
        $('#parentProcessId').empty();
        $('#parentProcessGroup').hide();

    }).on("select2:unselect", function (e) {
        $('#isChildProcess').prop('disabled', true).prop('checked', false);
        $('#parentProcessId').empty();
        $('#parentProcessGroup').hide();

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
