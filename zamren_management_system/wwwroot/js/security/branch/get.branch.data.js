$(document).ready(function () {

    window.getBranchData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/security/branch/get-branch-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('input[name="name"]').val(data.branch.name);
                $('input[name="id"]').val(data.branch.id);
                $('textarea[name="description"]').val(data.branch.description);
                $('.edit-branch-link').attr('href', '/Admin/Branch/Edit/' + data.branch.id);
                $('.branchName').text(data.branch.name + ' BRANCH');
                $('input[name="currentOrganizationName"]').val(data.branch.organization.name);

                populateSelect2(data.branch.organization.id);
            }
        });
    };

    // Call the method on page load
    getBranchData();

    function populateSelect2(selectedOrganizationId) {
        let selectInput = $(".organizationId-select2");

        let data = {
            organizationId: selectedOrganizationId,
        };
        ajaxDataRequest('/api/security/organization/get-organizations-except', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                console.error('An error occurred:', err);
            } else {
                selectInput.select2({
                    minimumInputLength: 0,
                    allowClear: true,
                    data: data.organizations.map((organization) => ({
                        id: organization.id,
                        text: organization.name
                    })),
                    placeholder: "Change Organization",
                    escapeMarkup: function (markup) {
                        return markup;
                    },
                    templateResult: formatRepo,
                    templateSelection: formatRepoSelection,
                }).on("select2:open", function () {
                    document.querySelector('.select2-search__field').focus();
                });
                selectInput.val(null).trigger('change');
            }
        });

    }
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
