$(document).ready(function () {

    //populate organization select element
    // populateSelectElement(step.organizations, 'organizationId', '- Select Organization -', step.organization.plainId, 'plainId', 'name');

    let organizationIdSelect = $('#organizationId');
    let branchIdSelect = $('#branchId');
    let departmentIdSelect = $('#departmentId');
    let officeIdSelect = $('#officeId');

    // Add event listeners for cascading selects
    organizationIdSelect.change(function () {
        let orgId = $(this).val();
        if (orgId) {
            departmentIdSelect.html(`<option value="">- Select Department -</option>`).prop('disabled', true);
            officeIdSelect.html(`<option value="">- Select Office -</option>`).prop('disabled', true);

            ajaxDataRequest('/api/workflow/step/get-branches-by-organization', 'POST', {organizationId: orgId}, null, function (err, data) {
                if (err) {
                    console.error('An error occurred:', err);
                } else {
                    // console.log(data.branches);
                    populateSelectElement(data.branches, 'branchId', '- Select Branch -', null, 'plainId', 'name');
                    branchIdSelect.prop('disabled', false);
                }
            });
        } else {
            branchIdSelect.html(`<option value="">- Select Branch -</option>`).prop('disabled', true);
            departmentIdSelect.html(`<option value="">- Select Department -</option>`).prop('disabled', true);
            officeIdSelect.html(`<option value="">- Select Office -</option>`).prop('disabled', true);
        }
    });

    branchIdSelect.change(function () {
        let branchId = $(this).val();
        // console.log(branchId);
        if (branchId) {
            officeIdSelect.html(`<option value="">- Select Office -</option>`).prop('disabled', true);

            ajaxDataRequest('/api/workflow/step/get-departments-by-branch', 'POST', {branchId: branchId}, null, function (err, data) {
                if (err) {
                    console.error('An error occurred:', err);
                } else {
                    populateSelectElement(data.departments, 'departmentId', '- Select Department -', null, 'plainId', 'name');
                    departmentIdSelect.prop('disabled', false);
                }
            });
        } else {
            departmentIdSelect.html(`<option value="">- Select Department -</option>`).prop('disabled', true);
            officeIdSelect.html(`<option value="">- Select Office -</option>`).prop('disabled', true);
        }
    });

    departmentIdSelect.change(function () {
        let departmentId = $(this).val();
        if (departmentId) {
            ajaxDataRequest('/api/workflow/step/get-offices-by-department', 'POST', {departmentId: departmentId}, null, function (err, data) {
                if (err) {
                    console.error('An error occurred:', err);
                } else {
                    populateSelectElement(data.offices, 'officeId', '- Select Office -', null, 'plainId', 'name');
                    officeIdSelect.prop('disabled', false);
                }
            });
        } else {
            officeIdSelect.html(`<option value="">- Select Office -</option>`).prop('disabled', true);
        }
    });
});