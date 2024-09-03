$(document).ready(function () {

    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();
    let data = {
        id: id,
    };

    let table = $("#branch-departments-table").DataTable({
        ajax: {
            url: "/api/security/branch/get-departments-in-branch-dt", //dt=datatable
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
            {data: "name", name: "name"},
            {data: "description", name: "description"},
            {
                data: null,
                render: function (data, type, row) {
                    return `<a class="btn btn-outline-primary btn-sm" href="/Admin/Department/Edit/${row.id}">Edit</a>`;
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