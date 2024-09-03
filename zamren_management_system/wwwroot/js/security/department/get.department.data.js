$(document).ready(function () {

    window.getDepartmentData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/security/department/get-department-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('input[name="name"]').val(data.department.name);
                $('input[name="id"]').val(data.department.id);
                $('textarea[name="description"]').val(data.department.description);
                $('.edit-department-link').attr('href', '/Admin/Department/Edit/' + data.department.id);
                $('.departmentName').text(data.department.name + ' DEPARTMENT');
                $('input[name="currentBranchName"]').val(data.department.branch.name + ' BRANCH');

                populateSelect2(data.department.branch.id);
            }
        });
    };

    // Call the method on page load
    getDepartmentData();

    function populateSelect2(selectedBranchId) {
        let selectInput = $(".branchId-select2");

        let data = {
            branchId: selectedBranchId,
        };
        ajaxDataRequest('/api/security/branch/get-branches-except', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                console.error('An error occurred:', err);
            } else {
                selectInput.select2({
                    minimumInputLength: 0,
                    allowClear: true,
                    data: data.branches.map((branch) => ({
                        id: branch.id,
                        text: branch.name + ' BRANCH'
                    })),
                    placeholder: "Select Branch",
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

