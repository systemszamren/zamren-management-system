$(document).ready(function () {

    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();
    let data = {
        processId: id,
    };

    window.stepsDatatable = null;

    stepsDatatable = $("#process-steps-table").DataTable({
        ajax: {
            url: "/api/workflow/step/get-steps-in-process-dt", //dt=datatable
            type: "POST",
            data: data,
            datatype: "json",
            dataFilter: function (data) {
                let json = jQuery.parseJSON(data);
                // console.log(json.response);

                let alert = `<div class="alert alert-success" role="alert">All steps are properly configured and ordered correctly</div>`;
                if (json.response) {
                    if (json.response.errors[0]) {
                        let error = json.response.errors[0].description;
                        alert = `<div class="alert alert-danger" role="alert">${error}</div>`;
                    }
                }
                $('.responseMsg').html(alert);
                return JSON.stringify(json);
            },
        },
        columns: [
            {data: "order", name: "order"},
            //name bold
            {
                data: null,
                render: function (data, type, row) {
                    return `<strong>${row.name}</strong>`;
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    return row.previousStep.name ? row.previousStep.name : '<i class="fa fa-minus-circle"></i>';
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    //if is not final step and has no next step return link to add next step, else return minus icon
                    /*if (!row.isFinalStep && !row.nextStep.name) {
                        if (data.order === stepsDatatable.rows().count()) {
                            return `<a class="btn btn-outline-dark btn-sm" href="/Admin/Step/Create/${id}">Create Next Step</a>`;
                        } else {
                            return `<a class="btn btn-outline-dark btn-sm" href="/Admin/Step/Edit/${row.id}">Add Next Step</a>`;
                        }
                    } else {
                        return row.nextStep.name ? row.nextStep.name : '<i class="fa fa-minus-circle"></i>';
                    }*/
                    return row.nextStep.name ? row.nextStep.name : '<i class="fa fa-minus-circle"></i>';
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    return `<a class="btn btn-outline-primary btn-sm" href="/Admin/Step/Edit/${row.id}">Manage</a>
                            <button class="btn btn-outline-danger btn-sm deleteStepBtn" data-stepid="${row.id}" data-name="${row.name}">Delete</button>`;
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
        /*rowReorder: {
            selector: 'tr',
            update: false,
            dataSrc: 'readingOrder',
        },
        select: true,*/
        pageLength: 50,
        lengthMenu: [50, 100, 250, 500],
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
                        text: "Create Step",
                        className: "btn btn-primary",
                        action: function () {
                            window.location.href = "/Admin/Step/Create/" + id;
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

    stepsDatatable.on("row-reorder", function (e, diff, edit) {
        let temp = edit.triggerRow.data();
        let result =
            "Reorder started on row: " + edit.triggerRow.data()["name"] + "<br>";

        for (let i = 0, ien = diff.length; i < ien; i++) {
            let rowData = stepsDatatable.row(diff[i].node).data();

            result +=
                rowData["name"] +
                " updated to be in position " +
                diff[i].newData +
                " (was " +
                diff[i].oldData +
                ")<br>";
        }

        // document.querySelector('#result').innerHTML = 'Event result:<br>' + result;
        console.log('Event result:<br>' + result);
    });

    // Reload table
    function reloadTable() {
        stepsDatatable.ajax.reload();
    }
})
;