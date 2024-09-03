$(document).ready(function () {
    window.departmentsDatatable = null;

    departmentsDatatable = $("#departments-table").DataTable({
        ajax: {
            url: "/api/security/department/get-departments-dt", //dt=datatable
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
            {data: "name", name: "name"},
            {
                data: null,
                render: function (data, type, row) {
                    return row.branch.name;
                },
            },
            {data: "description", name: "description"},
            {
                data: null,
                render: function (data, type, row) {
                    return `<button type="button" class="btn btn-sm btn-light countDepartmentOfficesBtn" value="${row.id}">Count</button>`;
                },
            },
            {
                data: null,
                render: function (data, type, row) {
                    // console.log(row);
                    return `<a class="btn btn-outline-primary btn-sm" href="/Admin/Department/Edit/${row.id}">Edit</a>
                            <button class="btn btn-outline-danger btn-sm deleteDepartmentBtn" data-departmentid="${row.id}" data-name="${row.name}">Delete</button>`;
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
                        text: "Create New Department",
                        className: "btn btn-primary",
                        action: function () {
                            window.location.href = "/Admin/Department/Create";
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
        departmentsDatatable.ajax.reload();
    }
});
