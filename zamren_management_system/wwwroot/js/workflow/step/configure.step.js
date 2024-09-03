$(document).ready(function () {

    //if #isInitialStep checkbox is checked, disable the #previousStepId select and set it to null
    $('#isInitialStep').change(function () {
        if (this.checked) {
            $('#previousStepId').prop('disabled', true).val(null);
        } else {
            $('#previousStepId').prop('disabled', false);
        }
    });

    //if #isFinalStep checkbox is checked, disable the #nextStepId select and set it to null
    $('#isFinalStep').change(function () {
        if (this.checked) {
            $('#nextStepId').prop('disabled', true).val(null);
        } else {
            $('#nextStepId').prop('disabled', false);
        }
    });

    //if #isAutoApproved checkbox is checked, disable and set to null
    $('#isAutoApproved').change(function () {
        if (this.checked) {
            $('#stepAssignmentsAlert').hide();
            $('#actioningUserId').prop('disabled', true).val(null).trigger("change");
            $('#roleId').prop('disabled', true).val(null);
            // $('#officeId').prop('disabled', true).val(null);
            // $('#departmentId').prop('disabled', true).val(null);
            // $('#branchId').prop('disabled', true).val(null);
            $('#organizationId').prop('disabled', true).val(null);
            $('.selectedUserDiv').hide();
        } else {
            //selectedUserId
            let selectedUserId = $('#selectedUserId').val();
            if (!selectedUserId) {
                $('#roleId').prop('disabled', false);
                // $('#officeId').prop('disabled', false);
                // $('#departmentId').prop('disabled', false);
                // $('#branchId').prop('disabled', false);
                $('#organizationId').prop('disabled', false);
                $('#actioningUserId').prop('disabled', false);
            }
            $('#stepAssignmentsAlert').show();
            $('.selectedUserDiv').show();
        }
    });

    $('#isDepartmentHeadApproved').change(function () {
        if (this.checked) {
            $('#stepAssignmentsAlert').hide();
            $('#actioningUserId').prop('disabled', true).val(null).trigger("change");
            $('#roleId').prop('disabled', true).val(null);
            $('#officeId').prop('disabled', true).val(null);
            $('#departmentId').prop('disabled', true).val(null);
            $('#branchId').prop('disabled', true).val(null);
            $('#organizationId').prop('disabled', true).val(null);
            $('.selectedUserDiv').hide();
        } else {
            //selectedUserId
            let selectedUserId = $('#selectedUserId').val();
            if (!selectedUserId) {
                $('#roleId').prop('disabled', false);
                $('#officeId').prop('disabled', false);
                $('#departmentId').prop('disabled', false);
                $('#branchId').prop('disabled', false);
                $('#organizationId').prop('disabled', false);
                $('#actioningUserId').prop('disabled', false);
            }
            $('#stepAssignmentsAlert').show();
            $('.selectedUserDiv').show();
        }
    });


});