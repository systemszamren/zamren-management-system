$(document).ready(function () {

    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();
    let data = {
        id: id,
    };

    let table = $("#office-users-table").DataTable({
        ajax: {
            url: "/api/security/office/get-users-in-office-dt", //dt=datatable
            type: "POST",
            data: data,
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
                        text: "Manage Office Users",
                        className: "btn btn-primary",
                        action: function () {
                            window.location.href = "/Admin/Office/ManageUsers/" + id;
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

    // Reload table
    function reloadTable() {
        table.ajax.reload();
    }
});