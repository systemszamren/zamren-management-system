$(document).ready(function () {

    // if #isEmployee is checked, show the .employeesSelect div and .canActionWkfTaskCheckbox div
    $("#isEmployee").change(function () {
        if ($(this).is(":checked")) {
            $(".employeesSelect").show();
            $(".canActionWkfTaskCheckbox").show();
        } else {
            $(".employeesSelect").hide();
            $(".canActionWkfTaskCheckbox").hide();
        }
    });

    //init select2
    let selectInput = $(".employeesUserId-select2");

    selectInput.select2({
        minimumInputLength: 3,
        allowClear: true,
        width: '100%',
        ajax: {
            url: "/api/security/user/get-employees-select2",
            type: "POST",
            dataType: "json",
            delay: 250,
            data: function (params) {
                // console.log(params);
                return {
                    q: params.term,
                    page: params.page
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

    }).on("select2:unselect", function (e) {
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
