$(document).ready(function () {

    //get encrypted reference from url if url contains ?reference=xxx parameter
    let url = new URL(window.location.href);
    let reference = url.searchParams.get("reference");
    let data = {reference: reference ? reference : null};

    ajaxDataRequest('/api/procurement/purchase-requisition-good/get-purchase-requisition-data', 'POST', data, null, function (err, data) {
        // console.log(err, data);
        if (err) {
            // Handle error
            console.error('An error occurred:', err);
        } else {
            // Handle success
            $('.currentUserFullName').val(data.user.fullName);
            $('.processName').html(data.currentProcess.name);
            $('.stepName').html(data.currentStep.name);


            //manage task btns
            if (data?.task?.wasSentBack) {
                $('#send-back-task-btn').show();
            }

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
                $('#justification').val(data.purchaseRequisition.justification);

                //populate items here
                if (data.purchaseRequisition.purchaseRequisitionGoods) {
                    populateItems(data.purchaseRequisition.purchaseRequisitionGoods);
                }

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

    function populateItems(items) {
        $('#goodsContainer').empty(); // Clear existing items
        items.forEach((item, index) => {
            let goodItem = `
            <tr class="good-item">
                <td class="item-number">${index + 1}</td>
                <td>
                    <textarea class="form-control large-textarea" maxlength="255" name="itemDescription[]" rows="3" placeholder="Describe Item" required>${item.description}</textarea>
                </td>
                <td>
                    <input type="number" class="form-control" name="quantity[]" min="1" placeholder="0" required value="${item.quantity}">
                </td>
                <td>
                    <input type="number" class="form-control" name="unitPrice[]" min="1" placeholder="0.00" required value="${item.unitPrice}">
                </td>
                <td>
                    <input type="file" class="form-control" name="goodFile[]" accept=".png,.jpg,.jpeg,.pdf">                    
                    <a href="#" class="badge text-primary bg-light chat-step-attachment" title="${item.systemAttachment.originalFileName}" data-bs-toggle="modal" data-bs-target="#fileModal" data-file="${item.systemAttachment.systemFileName}">
                        <i class="fa fa-paperclip text-primary"></i> ${item.systemAttachment.originalFileName}
                    </a>
                </td>
                <td>
                    <button type="button" class="btn btn-danger removeGood">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
            $('#goodsContainer').append(goodItem);
        });
    }

});
