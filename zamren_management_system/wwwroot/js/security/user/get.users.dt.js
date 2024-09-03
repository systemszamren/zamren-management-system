$(document).ready(function () {
    let table = $("#users-table").DataTable({
        ajax: {
            url: "/api/security/user/get-users-dt", //dt=datatable
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
            {data: "firstName", name: "firstName"},
            {data: "lastName", name: "lastName"},
            {data: "email", name: "email"},
            {
                data: "isEmployee",
                name: "isEmployee",
                render: function (data, type, row) {
                    if (row.isEmployee) {
                        return `<strong class="badge bg-dark text-white">Employee</strong>`;
                    } else {
                        return `<strong class="badge bg-light text-dark">Customer</strong>`;
                    }
                },
            },
            // {data: "phoneNumber", name: "phoneNumber"},
            // {data: "userName", name: "userName"},
            {
                data: "status",
                name: "status",
                render: function (data, type, row) {
                    return `<strong class="badge bg-light text-dark">${convertToDisjointSentence(data)}</strong>`;
                },
            },

            // {data: "emailConfirmed", name: "emailConfirmed"},
            // {data: "phoneNumberConfirmed", name: "phoneNumberConfirmed"},
            // {data: "twoFactorEnabled", name: "twoFactorEnabled"},
            /*{
                data: "lockoutEnd",
                name: "lockoutEnd",
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
            {data: "lockoutEnabled", name: "lockoutEnabled"},
            {data: "accessFailedCount", name: "accessFailedCount"},
            {
                data: "accountCreatedDate",
                name: "accountCreatedDate",
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
                data: "lastSuccessfulLoginDate",
                name: "lastSuccessfulLoginDate",
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
                data: "lastSuccessfulPasswordChangeDate",
                name: "lastSuccessfulPasswordChangeDate",
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
                data: "accountDeletionScheduledDate",
                name: "accountDeletionScheduledDate",
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
            },*/
            {
                data: null,
                render: function (data, type, row) {
                    // console.log(row);
                    return `<a class="btn btn-outline-primary btn-sm" title="View ${row.firstName} ${row.lastName}'s Profile" href="/Admin/User/Edit/${row.id}" data-fname="${row.firstName}" data-lname="${row.lastName}">Profile</a>`;
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
                        className: "btn btn-light",
                        titleAttr: 'Reload',
                        action: function () {
                            reloadTable();
                        }
                    },
                    {
                        text: "Create New User",
                        className: "btn btn-primary",
                        action: function () {
                            window.location.href = "/Admin/User/Create";
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

    function reloadTable() {
        table.ajax.reload();
    }
});
