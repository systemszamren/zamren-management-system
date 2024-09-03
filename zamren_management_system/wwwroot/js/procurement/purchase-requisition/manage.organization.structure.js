$(document).ready(function () {
    
    let organizationIdSelect = $('#organizationId');
    let branchIdSelect = $('#branchId');
    let departmentIdSelect = $('#departmentId');
    let officeIdSelect = $('#officeId');

    // Add event listeners for cascading selects
    organizationIdSelect.change(function () {
        let orgId = $(this).val();
        if (orgId) {
            departmentIdSelect.empty().prop('disabled', true);
            officeIdSelect.empty().prop('disabled', true);

            ajaxDataRequest('/api/procurement/purchase-requisition/get-branches-by-organization', 'POST', {organizationId: orgId}, null, function (err, data) {
                if (err) {
                    console.error('An error occurred:', err);
                } else {
                    console.log(data.branches);
                    populateSelectElement(data.branches, 'branchId', '- Select Branch -', null, 'plainId', 'name');
                    branchIdSelect.prop('disabled', false);
                }
            });
        } else {
            branchIdSelect.empty().prop('disabled', true);
            departmentIdSelect.empty().prop('disabled', true);
            officeIdSelect.empty().prop('disabled', true);
        }
    });

    branchIdSelect.change(function () {
        let branchId = $(this).val();
        // console.log(branchId);
        if (branchId) {
            officeIdSelect.empty().prop('disabled', true);

            ajaxDataRequest('/api/procurement/purchase-requisition/get-departments-by-branch', 'POST', {branchId: branchId}, null, function (err, data) {
                if (err) {
                    console.error('An error occurred:', err);
                } else {
                    populateSelectElement(data.departments, 'departmentId', '- Select Department -', null, 'plainId', 'name');
                    departmentIdSelect.prop('disabled', false);
                }
            });
        } else {
            departmentIdSelect.empty().prop('disabled', true);
            officeIdSelect.empty().prop('disabled', true);
        }
    });

    departmentIdSelect.change(function () {
        let departmentId = $(this).val();
        if (departmentId) {
            ajaxDataRequest('/api/procurement/purchase-requisition/get-offices-by-department', 'POST', {departmentId: departmentId}, null, function (err, data) {
                if (err) {
                    console.error('An error occurred:', err);
                } else {
                    populateSelectElement(data.offices, 'officeId', '- Select Office -', null, 'plainId', 'name');
                    officeIdSelect.prop('disabled', false);
                }
            });
        } else {
            officeIdSelect.empty().prop('disabled', true);
        }
    });

});