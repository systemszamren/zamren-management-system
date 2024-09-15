$(document).ready(function () {

    //get encrypted reference from url if url contains ?reference=xxx parameter
    let url = new URL(window.location.href);
    let reference = url.searchParams.get("reference");
    let data = {reference: reference ? reference : null};

    ajaxDataRequest('/api/procurement/purchase-requisition/get-initiate-purchase-requisition-data', 'POST', data, null, function (err, data) {
        console.log(err, data);
        if (err) {
            // Handle error
            console.error('An error occurred:', err);
        } else {
            // Handle success
            $('.currentUserFullName').val(data.user.fullName);
            $('.processName').html(data.currentProcess.name);
            $('.stepName').html(data.currentStep.name);

            //populate organization select element
            populateSelectElement(
                data.organizations,
                'organizationId',
                '- Select Organization -',
                data.purchaseRequisition?.organizationId,
                'plainId',
                'name'
            );

            //populate branch select element
            populateSelectElement(
                data.branches,
                'branchId',
                '- Select Branch -',
                data.purchaseRequisition?.branchId,
                'plainId',
                'name'
            );

            //populate department select element
            populateSelectElement(
                data.departments,
                'departmentId',
                '- Select Department -',
                data.purchaseRequisition?.departmentId,
                'plainId',
                'name'
            );

            //populate office select element
            populateSelectElement(
                data.offices,
                'officeId',
                '- Select Office -',
                data.purchaseRequisition?.officeId,
                'plainId',
                'name'
            );

            if (data.purchaseRequisition) {

                //populate items
                $('#itemDescription').val(data.purchaseRequisition.itemDescription);
                $('#quantity').val(data.purchaseRequisition.quantity);
                $('#estimatedCost').val(data.purchaseRequisition.estimatedCost);
                $('#justification').val(data.purchaseRequisition.justification);

                //if step.organizationId is set, enable branch select
                if (data.purchaseRequisition.organizationId)
                    $('#branchId').prop('disabled', false);

                //if step.branchId is set, enable department select
                if (data.purchaseRequisition.branchId)
                    $('#departmentId').prop('disabled', false);

                //if step.departmentId is set, enable office select
                if (data.purchaseRequisition.departmentId)
                    $('#officeId').prop('disabled', false);
            }
        }
    });

});
