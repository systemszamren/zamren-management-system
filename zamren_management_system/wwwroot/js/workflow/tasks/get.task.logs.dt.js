$(document).ready(function () {
    window.TaskLogDatatable = null;

    $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        let target = $(e.target).attr('data-bs-target');
        if (target === '#tab2') {
            populateTaskLogDatatable();
        }
    });
    
    function populateTaskLogDatatable() {

        //if datatable already exists, destroy it
        if (window.TaskLogDatatable) {
            window.TaskLogDatatable.destroy();
        }

        let data = {
            taskId: $("#taskId").val(),
        };

        TaskLogDatatable = $("#task-log-table").DataTable({
            ajax: {
                url: "/api/workflow/task/get-task-logs-dt",
                type: "POST",
                data: data,
                datatype: "json",
                dataFilter: function (data) {
                    let json = jQuery.parseJSON(data);
                    // console.log(json.response.successDescription);
                    return JSON.stringify(json);
                }
            },
            columns: [
                {data: "counter", name: "counter"},
                {
                    data: null,
                    render: function (data, type, row) {
                        return row.currentStep.name;
                    },
                },
                {
                    data: "actionDate",
                    name: "actionDate",
                    render: function (data, type, row) {
                        let date = new Date(data);
                        if (!data || date.getFullYear() <= 1) return "";
                        return date.toLocaleString("en-GB", {
                            day: "2-digit",
                            month: "short",
                            year: "numeric",
                            hour: "2-digit",
                            minute: "2-digit",
                        });
                    },
                },
                {
                    data: null,
                    render: function (data, type, row) {
                        return row.action;
                    },
                },
                {
                    data: null,
                    render: function (data, type, row) {
                        return row.actioningUser != null ? `<a href="/Admin/User/Edit/${row.actioningUser.id}" target="_blank">${row.actioningUser.firstName} ${row.actioningUser.lastName}</a>` : "";
                    },
                },
                {
                    data: null,
                    render: function (data, type, row) {
                        return row.nextActioningUser != null ? `<a href="/Admin/User/Edit/${row.nextActioningUser.id}" target="_blank">${row.nextActioningUser.firstName} ${row.nextActioningUser.lastName}</a>` : "";
                    },
                },
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
        TaskLogDatatable.ajax.reload();
    }
});
