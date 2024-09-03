$(document).ready(function () {

    window.getUsersInRole = function () {

        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        //if datatable already exists, destroy it
        if (window.usersInRoleDatatable) {
            window.usersInRoleDatatable.destroy();
        }

        usersInRoleDatatable = $("#role-users-table").DataTable({
            ajax: {
                url: "/api/security/role/get-users-in-role-dt1", //dt=datatable
                type: "POST",
                data: data,
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
                        return `<a href="/Admin/User/Edit/${row.userId}" target="_blank" title="View ${row.firstName} ${row.lastName}'s Profile">${row.firstName} ${row.lastName}</a>`;
                    }
                },
                // {data: "firstName", name: "firstName"},
                // {data: "lastName", name: "lastName"},
                {data: "email", name: "email"},
                {
                    data: "startDate",
                    name: "startDate",
                    render: function (data, type, row) {
                        let date = new Date(data);
                        if (!data || date.getFullYear() <= 1) {
                            return "N/A";
                        }
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
                    data: "endDate",
                    name: "endDate",
                    render: function (data, type, row) {
                        let date = new Date(data);
                        if (!data || date.getFullYear() <= 1) {
                            return "N/A";
                        }
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
                    data: "isActive",
                    name: "isActive",
                    render: function (data, type, row) {
                        // console.log(data);
                        return data ? '<span class="badge m-1 bg-danger">Inactive</span>' : '<span class="badge m-1 bg-success">Active</span>';
                    },
                },
                /*{
                    data: null,
                    render: function (data, type, row) {
                        // console.log(row);
                        return `<a class="btn btn-outline-primary btn-sm" title="View ${row.firstName} ${row.lastName}'s Profile" href="/Admin/User/Edit/${row.userId}" data-fname="${row.firstName}" data-lname="${row.lastName}">Profile</a>`;
                    },
                }*/
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
                            text: "Manage Role Users",
                            className: "btn btn-primary",
                            action: function () {
                                window.location.href = "/Admin/Role/ManageUsers/" + id;
                            },
                        },
                        {
                            extend: 'colvis',
                            text: 'Select Columns'
                        }
                    ]
                }
            },
            // scrollX: true,
            // scrollY: 300,
            fixedColumns: {
                rightColumns: 1,
                leftColumns: 0
            }
        });

    };


    function reloadTable() {
        usersInRoleDatatable.ajax.reload();
    }
});
