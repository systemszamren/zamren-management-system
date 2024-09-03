$(document).ready(function () {

    window.getUserData = function () {
        //get userId from url
        let url = new URL(window.location.href);
        let userId = url.pathname.split('/').pop();
        let data = {
            userId: userId,
        };

        ajaxDataRequest('/api/security/user/get-user-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                let fullName = $('.fullName');
                fullName.html(data.user.firstName + ' ' + data.user.lastName + "'s Account");
                $('input[name="firstName"]').val(data.user.firstName);
                $('input[name="lastName"]').val(data.user.lastName);
                $('input[name="email"]').val(data.user.email);
                $('input[name="userId"]').val(data.user.id);

                //supervisor
                let supervisor = $('.supervisorDetailsDiv');
                // console.log(data.user.supervisor);
                if (data.user.supervisor.firstName) {
                    $('.supervisorUserIdLabel').html('Change Supervisor (If Any)');
                    supervisor.html(`<span class="badge bg-light text-dark">Current Supervisor: <a target="_blank" style="text-decoration: underline" href="/Admin/User/Edit/${data.user.supervisor.id}">${data.user.supervisor.firstName} ${data.user.supervisor.lastName}</a></span>`);
                } else {
                    $('.supervisorUserIdLabel').html('Select Supervisor (If Any)');
                    supervisor.html(`<span class="badge bg-light text-dark">No Supervisor</span>`);
                }

                let userAccountSettingBtn = $('.userAccountSettingBtn');
                userAccountSettingBtn.attr('data-userid', data.user.id);
                userAccountSettingBtn.attr('data-fname', data.user.firstName);
                userAccountSettingBtn.attr('data-lname', data.user.lastName);

                let EmailConfirmedBadge = `<span class="badge m-1 bg-${data.user.emailConfirmed ? 'success' : 'danger'}">${data.user.emailConfirmed ? 'Email Confirmed' : 'Email Not Confirmed'}</span>`;
                let PhoneNumberConfirmedBadge = `<span class="badge m-1 bg-${data.user.phoneNumberConfirmed ? 'success' : 'danger'}">${data.user.phoneNumberConfirmed ? 'Phone Number Confirmed' : 'Phone Number Not Confirmed'}</span>`;
                let TwoFactorEnabledBadge = `<span class="badge m-1 bg-${data.user.twoFactorEnabled ? 'success' : 'danger'}">${data.user.twoFactorEnabled ? '2FA Enabled' : '2FA Disabled'}</span>`;
                let LockoutEnabledBadge = `<span class="badge m-1 bg-${data.user.lockoutEnabled ? 'danger' : 'success'}">${data.user.lockoutEnabled ? 'Account Locked' : 'Account Unlocked'}</span>`;
                let AccessFailedCountBadge = `<span class="badge bg-${data.user.accessFailedCount >= 1 ? 'danger' : 'success'}">${data.user.accessFailedCount >= 1 ? data.user.accessFailedCount + ' Failed Login Attempts' : 'No Failed Login Attempts'}</span>`;
                let IsScheduledForDeletionBadge = `<span class="badge m-1 bg-${data.user.isScheduledForDeletion ? 'danger' : 'success'}">${data.user.isScheduledForDeletion ? 'Scheduled For Deletion On ' + convertDatetimeToDate(data.user.accountDeletionScheduledDate) : ''}</span>`;
                let recentActivityBadge = `<span class="badge m-1 bg-light text-black">Recent Activity: ${convertToDisjointSentence(data.user.status)}</span>`;
                let isEmployeeBadge = `<span class="badge m-1 bg-${data.user.isEmployee ? 'dark' : 'light'} text-${data.user.isEmployee ? 'white' : 'dark'}">${data.user.isEmployee ? 'Employee' : 'Customer'}</span>`;

                let emailAddressVerification = $('.emailAddressVerification');
                if (data.user.emailConfirmed === true) {
                    emailAddressVerification.html(`<i class="fa fa-check text-success" title="Verified"></i>`)
                } else {
                    $('.verifyEmailAddressBtn').attr('data-userid', data.user.id).prop('disabled', false);
                }

                let isEmployee = $("#isEmployee");
                isEmployee.prop('checked', data.user.isEmployee);

                let canActionWkfTasks = $("#canActionWkfTask");
                canActionWkfTasks.prop('checked', data.user.canActionWkfTask);

                let accountStatusBadge = $('.accountStatusBadge');
                accountStatusBadge.html(``);
                accountStatusBadge.append(isEmployeeBadge);
                accountStatusBadge.append(EmailConfirmedBadge);
                accountStatusBadge.append(PhoneNumberConfirmedBadge);
                accountStatusBadge.append(TwoFactorEnabledBadge);
                accountStatusBadge.append(LockoutEnabledBadge);
                accountStatusBadge.append(IsScheduledForDeletionBadge);
                accountStatusBadge.append(AccessFailedCountBadge);
                accountStatusBadge.append(recentActivityBadge);

                getUserProfilePicture($(".photo-preview"), userId, 'bg');
            }
        });
    };

    // Call the method on page-load
    getUserData();
});
