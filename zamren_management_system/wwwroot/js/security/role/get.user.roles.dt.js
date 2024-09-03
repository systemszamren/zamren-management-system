$(document).ready(function () {
    window.getUserRoles = function () {

        //get userId from url
        let url = new URL(window.location.href);
        let userId = url.pathname.split('/').pop();
        let data = {
            userId: userId,
        };

        //if datatable already exists, destroy it
        if (window.userRolesDatatable) {
            window.userRolesDatatable.destroy();
        }

        userRolesDatatable = $("#user-roles-table").DataTable({
            ajax: {
                url: "/api/security/role/get-user-roles-dt", //dt=datatable
                type: "POST",
                datatype: "json",
                data: data,
                dataFilter: function(data){
                    let json = jQuery.parseJSON(data);
                    // console.log(json);
                    return JSON.stringify(json);
                }
            },
            columns: [
                {data: "counter", name: "counter"},
                {data: "roleName", name: "roleName"},
                // {data: "roleDescription", name: "roleDescription"},
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
                    data: null,
                    render: function (data, type, row) {
                        // console.log(row);
                        return `
                                <a href="/Admin/Role/ManageUsers/EditTenure/${row.uniqueId}?ReturnUrl=true" class="btn btn-outline-primary btn-sm">Edit</a>
                                <button type="button" class="btn btn-outline-danger btn-sm removeRoleFromUserBtn" data-userid="${row.userId}" data-roleid="${row.roleId}" data-rolename="${row.roleName}">Remove</button>
                                `;
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
                            extend: 'colvis',
                            text: 'Select Columns'
                        }
                    ]
                }
            }
        });

    };

    function reloadTable() {
        userRolesDatatable.ajax.reload();
    }
});
