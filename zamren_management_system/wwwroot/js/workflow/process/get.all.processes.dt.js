$(document).ready(function () {
    window.processesDatatable = null;

    processesDatatable = $("#processes-table").DataTable({
        ajax: {
            url: "/api/workflow/process/get-processes-dt", //dt=datatable
            type: "POST",
            datatype: "json",
            dataFilter: function(data){
                let json = jQuery.parseJSON(data);
                // console.log(json);
                return JSON.stringify(json);
            }
        },
        columns: [
            {data: "counter", name: "counter"},
            // {data: "name", name: "name"},
            {
                data: null,
                render: function (data, type, row) {
                    // return row.name;
                    //if process is a child process add badge
                    if (row.parentProcessId) {
                        return `${row.name} <span class="badge bg-info">Child Process</span>`;
                    } else {
                        return row.name;
                    }
                },
            },
            {
                data: null,
                render: function (data, type, row) {
                    return row.module.name;
                },
            },
            {
                data: null,
                render: function (data, type, row) {
                    return `<button type="button" class="btn btn-sm btn-light countProcessStepsBtn" value="${row.id}">Count</button>`;
                },
            },
            {
                data: null,
                render: function (data, type, row) {
                    // console.log(row);
                    return `<a class="btn btn-outline-primary btn-sm" href="/Admin/Process/Edit/${row.id}">Manage</a>
                            <button class="btn btn-outline-danger btn-sm deleteProcessBtn" data-processid="${row.id}" data-name="${row.name}">Delete</button>`;
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
                    {
                        text: "Create New Process",
                        className: "btn btn-primary",
                        action: function () {
                            window.location.href = "/Admin/Process/Create";
                        },
                    },
                    {
                        extend: 'colvis',
                        text: 'Select Columns'
                    }
                ]
            }
        }
    });

    function reloadTable() {
        processesDatatable.ajax.reload();
    }
});
