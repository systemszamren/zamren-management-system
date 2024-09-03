$(document).ready(function () {
    window.get2FAStatus = function () {

        //get userId from url
        let url = new URL(window.location.href);
        let userId = url.pathname.split('/').pop();
        let data = {
            userId: userId,
        };

        //if datatable already exists, destroy it
        if (window.twoFADatatable) {
            window.twoFADatatable.destroy();
        }

        twoFADatatable = $("#two-factor-authentication-table").DataTable({
            ajax: {
                url: "/api/security/user/get-user-2fa-status-dt",
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
                {
                    data: "is2faEnabled",
                    name: "is2faEnabled",
                    render: function (data, type, row) {
                        return data ? '<span class="badge bg-success">Enabled</span>' : '<span class="badge bg-danger">Disabled</span>';
                    }
                },
                {data: "recoveryCodesLeft", name: "recoveryCodesLeft"},
                {
                    data: "is2faEnabled",
                    name: "is2faEnabled",
                    render: function (data, type, row) {
                        return data ? '<button class="btn btn-sm btn-danger disable2faBtn">Disable 2FA</button>' : '<button class="btn btn-sm btn-danger" disabled>Disable 2FA</button>';
                    }
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
        twoFADatatable.ajax.reload();
    }
});
