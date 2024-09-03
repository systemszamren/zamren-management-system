$(document).ready(function () {
    window.getExternalLogins = function () {

        //get userId from url
        let url = new URL(window.location.href);
        let userId = url.pathname.split('/').pop();
        let data = {
            userId: userId,
        };

        //if datatable already exists, destroy it
        if (window.externalLoginsDatatable) {
            window.externalLoginsDatatable.destroy();
        }

        externalLoginsDatatable = $("#external-logins-table").DataTable({
            ajax: {
                url: "/api/security/user/get-external-logins-dt",
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
                {data: "loginProvider", name: "loginProvider"},
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
        externalLoginsDatatable.ajax.reload();
    }
});
