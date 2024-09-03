$(document).ready(function () {

    window.tasksDatatable = null;

    tasksDatatable = $("#workflow-tasks-table").DataTable({
        ajax: {
            url: "/api/workflow/task/get-tasks-dt", //dt=datatable
            type: "POST",
            datatype: "json",
            dataFilter: function (data) {
                let json = jQuery.parseJSON(data);
                // console.log(json);
                return JSON.stringify(json);
            }
        },
        columns: [
            {data: "counter", name: "counter"},
            {
                data: null,
                render: function (data, type, row) {
                    return `<a href="/Admin/Task/Details/${row.id}" class="referenceLinkText" style="text-decoration: underline">${row.reference}</a> 
                            <i class="fa fa-copy copyReference" data-bs-toggle="tooltip" data-bs-placement="top" title="Copy reference"></i>`;
                },
            },
            {
                data: null,
                render: function (data, type, row) {
                    return row.currentProcess.name;
                },
            },
            {
                data: null,
                render: function (data, type, row) {
                    return row.currentStep != null ? row.currentStep.name : "";
                },
            },
            {
                data: null,
                render: function (data, type, row) {
                    return row.isOpen ? '<span class="badge bg-success">Active</span>' : '<span class="badge bg-danger">Closed</span>';
                },
            },
            {
                data: null,
                render: function (data, type, row) {
                    if (row.currentActioningUser != null) {
                        return `<a href="/Admin/User/Edit/${row.currentActioningUser.id}" target="_blank">${row.currentActioningUser.firstName} ${row.currentActioningUser.lastName}</a>`;
                    } else {
                        return "";
                    }
                },
            },
            {
                data: "currentProcessStartDate",
                name: "currentProcessStartDate",
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
                    return `<a href="/Admin/User/Edit/${row.taskStartedByUser.id}" target="_blank">${row.taskStartedByUser.firstName} ${row.taskStartedByUser.lastName}</a>`;
                },
            },
            {
                data: null,
                render: function (data, type, row) {
                    return row.previousActioningUser != null ? `<a href="/Admin/User/Edit/${row.previousActioningUser.id}" target="_blank">${row.previousActioningUser.firstName} ${row.previousActioningUser.lastName}</a>` : "";
                },
            },
            {
                data: null,
                render: function (data, type, row) {
                    return row.parentTask != null ? `<a href="/Admin/Task/Details/${row.parentTask.id}">${row.parentTask.reference}</a>` : "";
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
                        text: '<span>&#8635;</span>',
                        titleAttr: 'Reload',
                        action: function () {
                            reloadTable();
                        }
                    },
                    /*{
                        text: "Create New Process",
                        className: "btn btn-primary",
                        action: function () {
                            window.location.href = "/Admin/Process/Create";
                        },
                    },*/
                    {
                        extend: 'colvis',
                        text: 'Select Columns'
                    }
                ]
            }
        }
    });

    function reloadTable() {
        tasksDatatable.ajax.reload();
    }

    $(document).on('click', '.copyReference', function () {
        let copyText = $('.referenceLinkText').text();
        navigator.clipboard.writeText(copyText).then(function () {
            $('.copyReference').attr('data-bs-original-title', 'Copied!').tooltip('show');
        }, function (err) {
            console.error('Could not copy text: ', err);
        });
    });
});
