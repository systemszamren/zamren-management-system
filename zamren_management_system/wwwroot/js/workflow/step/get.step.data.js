$(document).ready(function () {

    window.getProcessData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/workflow/step/get-step-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                let step = data.step;
                // console.log(step);

                $('input[name="processId"]').val(step.process.id);
                $('input[name="stepId"]').val(step.id);
                $('.edit-process-link, .back-btn').attr('href', '/Admin/Process/Edit/' + step.process.id);
                $('#processName').val(step.process.name + ' PROCESS');
                $('#moduleName').val(step.process.module.name);

                //populate step form fields
                $('#name').val(step.name);
                $('#description').val(step.description);
                $('#isInitialStep').prop('checked', step.isInitialStep);
                $('#isFinalStep').prop('checked', step.isFinalStep);
                $('#requestMap').val(step.requestMap);

                if (step.isInitialStep)
                    $('#previousStepId').prop('disabled', true).val(null);

                if (step.isFinalStep)
                    $('#nextStepId').prop('disabled', true).val(null);

                let isAutoApproved = $('#isAutoApproved')
                isAutoApproved.prop('checked', step.isAutoApproved);

                let isDepartmentHeadApproved = $('#isDepartmentHeadApproved')
                isDepartmentHeadApproved.prop('checked', step.isDepartmentHeadApproved);

                //populate organization select element
                populateSelectElement(step.organizations, 'organizationId', '- Select Organization -', step.organization.plainId, 'plainId', 'name');

                //populate branch select element
                populateSelectElement(step.branches, 'branchId', '- Select Branch -', step.branch.plainId, 'plainId', 'name');

                //populate department select element
                populateSelectElement(step.departments, 'departmentId', '- Select Department -', step.department.plainId, 'plainId', 'name');

                //populate office select element
                populateSelectElement(step.offices, 'officeId', '- Select Office -', step.office.plainId, 'plainId', 'name');

                //if step.organization.plainId is set, enable branch select
                if (step.organization.plainId)
                    $('#branchId').prop('disabled', false);

                //if step.branch.plainId is set, enable department select
                if (step.branch.plainId)
                    $('#departmentId').prop('disabled', false);

                //if step.department.plainId is set, enable office select
                if (step.department.plainId)
                    $('#officeId').prop('disabled', false);


                //populate role select element
                populateSelectElement(step.roles, 'roleId', '- Select Role -', step.role.plainId, 'plainId', 'name');

                //populate processes select element
                // populateSelectElement(step.processes, 'nextProcessId', '- Select Next Process -', step.nextProcess.plainId, 'plainId', 'name');

                //populate processes select element
                // populateSelectElement(step.processes, 'prevProcessId', '- Select Previous Process -', step.prevProcess.plainId, 'plainId', 'name');

                //populate previous step select element
                populateSelectElement(step.steps, 'previousStepId', '- Select Previous Step -', step.previousStep.plainId, 'plainId', 'name');

                //populate next step select element
                populateSelectElement(step.steps, 'nextStepId', '- Select Next Step -', step.nextStep.plainId, 'plainId', 'name');

                //populate privileges select element
                populateSelectElement(step.privileges, 'privilegeId', '- Select Privilege -', step.privilege.plainId, 'plainId', 'name');

                //populate sla hours select element
                populateSelectElement(step.slaHoursList, 'slaHours', '- Select SLA Hours -', step.slaHours, 'hour', 'description');

                let selectedUserDiv = $('.selectedUserDiv');
                let selectedUserId = $('#selectedUserId');

                if (step.actioningUser.id || step.isAutoApproved === true) {//if either is true
                    $('#roleId').prop('disabled', true).val(null);
                    // $('#privilegeId').prop('disabled', true).val(null);
                    $('#officeId').prop('disabled', true).val(null);
                    $('#departmentId').prop('disabled', true).val(null);
                    $('#branchId').prop('disabled', true).val(null);
                    $('#organizationId').prop('disabled', true).val(null);
                }

                if (step.isAutoApproved === true) { //if checked
                    $('#actioningUserId').prop('disabled', true).val(null).trigger("change");
                    $('#stepAssignmentsAlert').hide();
                } else if (step.isAutoApproved === false) {//if not checked
                    $('#stepAssignmentsAlert').show();
                }

                if (step.actioningUser.id) {
                    let label = $('label[for="actioningUserId"]');
                    label.text('Change Actioning User');
                    selectedUserDiv.html('SELECTED USER: <a style="text-decoration: underline" target="_blank" href="/Admin/User/Edit/' + step.actioningUser.id + '">' + step.actioningUser.firstName + ' ' + step.actioningUser.lastName + ' (' + step.actioningUser.email + ')' + '</a> <button type="button" class="btn btn-sm btn-dark m-1 p-1" onclick="clearSelectedUser()">Remove</button>');
                    selectedUserId.val(step.actioningUser.id);
                    // selectedUserDiv.addClass('text-danger');
                    selectedUserDiv.addClass('bold');
                    selectedUserDiv.removeClass('text-muted');
                    $('#actioningUserId').prop('disabled', true).val(null).trigger("change");
                }

                // console.log(step.isDepartmentHeadApproved);
                if (step.isDepartmentHeadApproved === true) { //if checked 
                    $('#actioningUserId').prop('disabled', true).val(null).trigger("change");
                    $('#roleId').prop('disabled', true).val(null);
                    $('#officeId').prop('disabled', true).val(null);
                    $('#departmentId').prop('disabled', true).val(null);
                    $('#branchId').prop('disabled', true).val(null);
                    $('#organizationId').prop('disabled', true).val(null);
                    $('#stepAssignmentsAlert').hide();
                }
            }
        });
    };

    getProcessData();

    //clearSelectedUser()
    window.clearSelectedUser = function () {
        let selectedUserDiv = $('.selectedUserDiv');
        let selectedUserId = $('#selectedUserId');
        // let selectedUserId = $('#selectedUserId');
        let label = $('label[for="actioningUserId"]');
        label.text('Select Actioning User');
        selectedUserDiv.html('');
        selectedUserId.val(null);
        // selectedUserId.val(null);
        // selectedUserDiv.removeClass('bold');
        // selectedUserDiv.addClass('text-muted');

        //check if $('#actioningUserId') has a value
        let actioningUserId = $('#actioningUserId');
        if (!actioningUserId.val()) {
            actioningUserId.prop('disabled', false);
            $('#roleId').prop('disabled', false);
            // $('#privilegeId').prop('disabled', false);
            $('#officeId').prop('disabled', false);
            $('#departmentId').prop('disabled', false);
            $('#branchId').prop('disabled', false);
            $('#organizationId').prop('disabled', false);
        }

    };
});