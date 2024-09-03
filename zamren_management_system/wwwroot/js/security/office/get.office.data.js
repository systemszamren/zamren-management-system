$(document).ready(function () {

    window.getOfficeData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/security/office/get-office-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('input[name="name"]').val(data.office.name);
                $('input[name="id"]').val(data.office.id);
                $('textarea[name="description"]').val(data.office.description);
                $('.edit-office-link').attr('href', '/Admin/Office/Edit/' + data.office.id);
                $('.officeName').text(data.office.name);

                $('input[name="currentDepartmentName"]').val(data.office.department.name + ' DEPARTMENT');

                populateSelect2(data.office.department.id);
            }
        });
    };

    // Call the method on page load
    getOfficeData();

    function populateSelect2(selectedOfficeId) {
        let selectInput = $(".departmentId-select2");

        let data = {
            departmentId: selectedOfficeId,
        };
        ajaxDataRequest('/api/security/office/get-departments-except', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                console.error('An error occurred:', err);
            } else {
                selectInput.select2({
                    minimumInputLength: 0,
                    allowClear: true,
                    data: data.departments.map((department) => ({
                        id: department.id,
                        text: department.name + ' DEPARTMENT'
                    })),
                    placeholder: "Select Department",
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