$(document).ready(function () {
    window.getUserPasswordHistory = function () {

        //get userId from url
        let url = new URL(window.location.href);
        let userId = url.pathname.split('/').pop();
        let data = {
            userId: userId,
        };

        //if datatable already exists, destroy it
        if (window.userPasswordHistoryDatatable) {
            window.userPasswordHistoryDatatable.destroy();
        }

        userPasswordHistoryDatatable = $("#user-password-history-table").DataTable({
            ajax: {
                url: "/api/security/user/get-user-password-history-dt", //dt=datatable
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
                    data: "passwordCreatedDate",
                    name: "passwordCreatedDate",
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
                    data: "passwordExpiryTimeLeft",
                    name: "passwordExpiryTimeLeft",
                    render: function (data, type, row) {
                        let date = new Date(row.passwordExpiryDate);
                        if (!data || date.getFullYear() <= 1) {
                            return "N/A";
                        }
                        let formattedDate = date.toLocaleString("en-GB", {
                            day: "2-digit",
                            month: "short",
                            year: "numeric",
                            hour: "2-digit",
                            minute: "2-digit",
                        });

                        return `<span data-toggle="tooltip" title="Expire on ${formattedDate}" style="text-decoration: underline dotted; cursor: help;">${data}</span>`;
                    },
                },
                {data: "status", name: "status"},
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
                            text: "Send Password Reset Request",
                            className: "btn btn-primary",
                            action: function () {
                                sendPasswordResetRequest(data);
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

    };

    function reloadTable() {
        userPasswordHistoryDatatable.ajax.reload();
    }

    function sendPasswordResetRequest(data) {
        let confirmation = confirm("Are you sure you want to send a password reset email to this user?");
        if (!confirmation) {
            return;
        }

        ajaxDataRequest("/api/security/user/send-password-reset-email", "POST", data, null, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
            }
        });
    }
});
