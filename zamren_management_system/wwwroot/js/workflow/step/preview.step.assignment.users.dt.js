$(document).ready(function () {
    window.stepAssignmentUsersDatatable = null;

    $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        let target = $(e.target).attr('data-bs-target');
        if (target === '#previewAssignmentsTab3') {
            populateStepAssignmentUsersDatatable();
        }
    });

    function populateStepAssignmentUsersDatatable() {

        //if datatable already exists, destroy it
        if (window.stepAssignmentUsersDatatable) {
            window.stepAssignmentUsersDatatable.destroy();
        }

        let data = {
            stepId: $("#stepId").val(),
        };

        stepAssignmentUsersDatatable = $("#step-assignment-users-table").DataTable({
            ajax: {
                url: "/api/workflow/step/get-step-assignment-users-dt", //dt=datatable
                type: "POST",
                data: data,
                datatype: "json",
                dataFilter: function (data) {
                    let json = jQuery.parseJSON(data);
                    // console.log(json.response.successDescription);

                    if (json.response.successDescription)
                        $(".previewAssignmentsTab3Alert").text(json.response.successDescription);

                    return JSON.stringify(json);
                }
            },
            columns: [
                {data: "counter", name: "counter"},
                {
                    data: null,
                    render: function (data, type, row) {
                        return `<a href="/Admin/User/Edit/${row.id}" style="text-decoration: underline" target="_blank">${row.fullName}</a>`;
                    },
                },
                {data: "email", name: "email"},
            ],
            serverSide: true,
            processing: true,
            responsive: true,
            language: {
                processing: "Loading records...",
                emptyTable: "No records found",
            },
            order: [0, "asc"],
            pageLength: 10,
            lengthMenu: [10, 25, 50, 75, 100],
            layout: {
                topStart: {
                    buttons: [
                        {
                            text: 'Reload Table <span>&#8635;</span>',
                            titleAttr: 'Reload',
                            action: function () {
                                reloadTable();
                            }
                        },
                        {
                            extend: 'colvis',
                            text: 'Select Columns'
                        }
                    ]
                }
            }
        });
    }

    function reloadTable() {
        stepAssignmentUsersDatatable.ajax.reload();
    }
});
