$(document).ready(function () {

    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();
    let data = {
        id: id,
    };

    let table = $("#role-users-table2").DataTable({
        ajax: {
            url: "/api/security/role/get-users-in-role-dt2", //dt=datatable
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
            // {data: "fullName", name: "fullName"},
            //fullName links to the user's details page
            {
                data: null,
                render: function (data, type, row) {
                    let userId = row.userId;
                    let fullName = row.fullName;
                    return `<a href="/Admin/User/Edit/${userId}" target="_blank">${fullName}</a>`;
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
                data: null,
                render: function (data, type, row) {
                    let userId = row.userId;
                    let uniqueId = row.uniqueId;
                    let firstName = row.firstName;
                    let lastName = row.lastName;
                    return `
                        <a href="/Admin/Role/ManageUsers/EditTenure/${uniqueId}" class="btn btn-outline-primary btn-sm">Edit</a>
                        <button type="button" class="btn btn-outline-danger btn-sm removeUserFromRoleBtn" data-fname="${firstName}" data-lname="${lastName}" data-userid="${userId}">Remove</button>`;
                }
            }
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

    // Reload table
    function reloadTable() {
        table.ajax.reload();
    }
});