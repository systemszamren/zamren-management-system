$(document).ready(function () {

    window.getPrivilegesInRoleData = function () {
        
        //get userId from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        //if datatable already exists, destroy it
        if (window.privilegesInRoleDatatable) {
            window.privilegesInRoleDatatable.destroy();
        }

        privilegesInRoleDatatable = $("#role-privileges-table").DataTable({
            ajax: {
                url: "/api/security/role/get-privileges-in-role-dt", //dt=datatable
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
                {data: "description", name: "description"}
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
                            text: "Manage Role Privileges",
                            className: "btn btn-primary",
                            action: function () {
                                window.location.href = "/Admin/Role/ManagePrivileges/" + id;
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
        privilegesInRoleDatatable.ajax.reload();
    }
});
