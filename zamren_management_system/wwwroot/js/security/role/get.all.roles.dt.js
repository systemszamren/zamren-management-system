$(document).ready(function () {
    let table = $("#roles-table").DataTable({
        ajax: {
            url: "/api/security/role/get-roles-dt", //dt=datatable
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
            // {data: "description", name: "description"},
            {
                data: null,
                render: function (data, type, row) {
                    return `<button class="btn btn-sm btn-light countRoleUsersBtn" value="${row.id}">Count</button>`;
                },
            },
            {
                data: null,
                render: function (data, type, row) {
                    // console.log(row);
                    return `<a class="btn btn-outline-primary btn-sm" href="/Admin/Role/Edit/${row.id}">Manage</a>
                            <button class="btn btn-outline-danger btn-sm deleteRoleBtn" data-rolename="${row.name}" data-roleid="${row.id}">Delete</button>`;
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
                        text: "Create New Role",
                        className: "btn btn-primary",
                        action: function () {
                            window.location.href = "/Admin/Role/Create";
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
        table.ajax.reload();
    }
});
