$(document).ready(function () {

    window.getUserDetails = function () {
        //get userId from url
        let url = new URL(window.location.href);
        let userId = url.pathname.split('/').pop();
        let data = {
            userId: userId,
        };

        ajaxDataRequest('/api/security/user/get-user-details', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {

                let dateOfBirthField = $('input[name="dateOfBirth"]');
                let genderField = $('select[name="gender"]');
                let identityTypeField = $('select[name="identityType"]');
                let countryCodeField = $('select[name="countryCode"]');
                let nextOfKinGenderField = $('select[name="nextOfKinGender"]');
                let nextOfKinIdentityTypeField = $('select[name="nextOfKinIdentityType"]');
                let nextOfKinCountryCodeField = $('select[name="nextOfKinCountryCode"]');
                let identityNumberField = $('input[name="identityNumber"]');
                let cityField = $('input[name="city"]');
                let phoneNumberField = $('input[name="phoneNumber"]');
                let alternativePhoneNumberField = $('input[name="alternativePhoneNumber"]');
                let alternativeEmailAddressField = $('input[name="alternativeEmailAddress"]');
                let physicalAddressField = $('textarea[name="physicalAddress"]');
                let nextOfKinFirstNameField = $('input[name="nextOfKinFirstName"]');
                let nextOfKinLastNameField = $('input[name="nextOfKinLastName"]');
                let nextOfKinIdentityNumberField = $('input[name="nextOfKinIdentityNumber"]');
                let nextOfKinPhysicalAddressField = $('textarea[name="nextOfKinPhysicalAddress"]');
                let nextOfKinPhoneNumberField = $('input[name="nextOfKinPhoneNumber"]');
                let nextOfKinCityField = $('input[name="nextOfKinCity"]');
                let nextOfKinEmailAddressField = $('input[name="nextOfKinEmailAddress"]');
                let termsOfUseAcceptedField = $('input[name="termsOfUseAccepted"]');
                let privacyPolicyAcceptedField = $('input[name="privacyPolicyAccepted"]');
                let userDetailId = $('input[name="userDetailId"]');
                let userId = $('input[name="userId"]');

                let previewIdentityAttachmentBtn = $('.previewIdentityAttachmentBtn');
                let previewProofOfResidencyAttachmentBtn = $('.previewProofOfResidencyAttachmentBtn');
                let previewNextOfKinProofOfResidencyAttachmentBtn = $('.previewNextOfKinProofOfResidencyAttachmentBtn');
                let previewNextOfKinIdentityAttachmentBtn = $('.previewNextOfKinIdentityAttachmentBtn');

                let verifyPhoneNumberBtn = $('.verifyPhoneNumberBtn');
                let verifyAlternativePhoneNumberBtn = $('.verifyAlternativePhoneNumberBtn');
                let verifyNextOfKinPhoneNumberBtn = $('.verifyNextOfKinPhoneNumberBtn');
                let verifyAlternativeEmailAddressBtn = $('.verifyAlternativeEmailAddressBtn');
                let verifyNextOfKinEmailAddressBtn = $('.verifyNextOfKinEmailAddressBtn');


                //progress bar
                let progressBar = $('.progress-bar');
                let currentProgress = 0;
                if (data.userDetail.profileCompletionPercentage)
                    currentProgress = Number(data.userDetail.profileCompletionPercentage);

                // Update the width and text of the progress bar
                progressBar.css('width', currentProgress + '%').text(currentProgress + '%');

                // Update the color of the progress bar based on the percentage of completion
                if (currentProgress === 0) {
                    progressBar.css('width', 100 + '%').text('0% Profile Incomplete');
                } else if (currentProgress < 33) {
                    progressBar.removeClass('bg-warning bg-success').addClass('bg-danger');
                } else if (currentProgress < 66) {
                    progressBar.removeClass('bg-danger bg-success').addClass('bg-warning');
                } else {
                    progressBar.removeClass('bg-danger bg-warning').addClass('bg-success');
                }

                $.get("https://ipinfo.io?token=e8492ddd3d3fb4", function (response) {
                    if (response.country) {
                        //set phone number values
                        window.intlTelInput(phoneNumber, {
                            initialCountry: data.userDetail.user.phoneNumberCountryCode || response.country,
                        });
                        window.intlTelInput(alternativePhoneNumber, {
                            initialCountry: data.userDetail.alternativePhoneNumberCountryCode || response.country,
                        });
                        window.intlTelInput(nextOfKinPhoneNumber, {
                            initialCountry: data.userDetail.nextOfKinPhoneNumberCountryCode || response.country,
                        });
                    }
                }, "jsonp");

                //initialize phone number verification button values
                verifyPhoneNumberBtn.val(data.userDetail.user.phoneNumber);
                verifyPhoneNumberBtn.attr('data-userdetailid', data.userDetail.id);
                if (data.userDetail.user.phoneNumber) verifyPhoneNumberBtn.prop('disabled', false);
                verifyAlternativePhoneNumberBtn.val(data.userDetail.alternativePhoneNumber);
                verifyAlternativePhoneNumberBtn.attr('data-userdetailid', data.userDetail.id);
                if (data.userDetail.alternativePhoneNumber) verifyAlternativePhoneNumberBtn.prop('disabled', false);
                verifyNextOfKinPhoneNumberBtn.val(data.userDetail.nextOfKinPhoneNumber);
                verifyNextOfKinPhoneNumberBtn.attr('data-userdetailid', data.userDetail.id);
                if (data.userDetail.nextOfKinPhoneNumber) verifyNextOfKinPhoneNumberBtn.prop('disabled', false);
                verifyAlternativeEmailAddressBtn.val(data.userDetail.alternativeEmailAddress);
                verifyAlternativeEmailAddressBtn.attr('data-userdetailid', data.userDetail.id);
                if (data.userDetail.alternativeEmailAddress) verifyAlternativeEmailAddressBtn.prop('disabled', false);
                verifyNextOfKinEmailAddressBtn.val(data.userDetail.nextOfKinEmailAddress);
                verifyNextOfKinEmailAddressBtn.attr('data-userdetailid', data.userDetail.id);
                if (data.userDetail.nextOfKinEmailAddress) verifyNextOfKinEmailAddressBtn.prop('disabled', false);

                let phoneNumberVerification = $('.phoneNumberVerification');
                let alternativePhoneNumberVerification = $('.alternativePhoneNumberVerification');
                let nextOfKinPhoneNumberVerification = $('.nextOfKinPhoneNumberVerification');

                //is phone number valid
                if (data.userDetail.user.phoneNumberConfirmed) {
                    phoneNumberVerification.html(`<i class="fa fa-check text-success" title="Verified"></i>`)
                }
                if (data.userDetail.alternativePhoneNumberConfirmed) {
                    alternativePhoneNumberVerification.html(`<i class="fa fa-check text-success" title="Verified"></i>`)
                }
                if (data.userDetail.nextOfKinPhoneNumberConfirmed) {
                    nextOfKinPhoneNumberVerification.html(`<i class="fa fa-check text-success" title="Verified"></i>`)
                }

                identityNumberField.val(data.userDetail.identityNumber);
                cityField.val(data.userDetail.city);
                phoneNumberField.val(data.userDetail.user.phoneNumber);
                alternativePhoneNumberField.val(data.userDetail.alternativePhoneNumber);
                alternativeEmailAddressField.val(data.userDetail.alternativeEmailAddress);

                let alternativeEmailAddressVerification = $('.alternativeEmailAddressVerification');
                if (data.userDetail.alternativeEmailAddressConfirmed) {
                    alternativeEmailAddressVerification.html(`<i class="fa fa-check text-success" title="Verified"></i>`)
                }

                physicalAddressField.val(data.userDetail.physicalAddress);
                nextOfKinFirstNameField.val(data.userDetail.nextOfKinFirstName);
                nextOfKinLastNameField.val(data.userDetail.nextOfKinLastName);
                nextOfKinIdentityNumberField.val(data.userDetail.nextOfKinIdentityNumber);
                nextOfKinPhysicalAddressField.val(data.userDetail.nextOfKinPhysicalAddress);
                nextOfKinPhoneNumberField.val(data.userDetail.nextOfKinPhoneNumber);
                nextOfKinCityField.val(data.userDetail.nextOfKinCity);
                nextOfKinEmailAddressField.val(data.userDetail.nextOfKinEmailAddress);

                let nextOfKinEmailAddressVerification = $('.nextOfKinEmailAddressVerification');
                if (data.userDetail.nextOfKinEmailAddressConfirmed) {
                    nextOfKinEmailAddressVerification.html(`<i class="fa fa-check text-success" title="Verified"></i>`)
                }

                //set btn val and enable/disable if attachment exists
                if (data.userDetail.identityAttachmentId) {
                    previewIdentityAttachmentBtn.val(data.userDetail.identityAttachmentId).prop('disabled', false);
                }
                if (data.userDetail.proofOfResidencyAttachmentId) {
                    previewProofOfResidencyAttachmentBtn.val(data.userDetail.proofOfResidencyAttachmentId).prop('disabled', false);
                }
                if (data.userDetail.nextOfKinProofOfResidencyAttachmentId) {
                    previewNextOfKinProofOfResidencyAttachmentBtn.val(data.userDetail.nextOfKinProofOfResidencyAttachmentId).prop('disabled', false);
                }
                if (data.userDetail.nextOfKinIdentityAttachmentId) {
                    previewNextOfKinIdentityAttachmentBtn.val(data.userDetail.nextOfKinIdentityAttachmentId).prop('disabled', false);
                }

                //check if terms of use and privacy policy are accepted
                if (data.userDetail.termsOfUseAccepted) {
                    termsOfUseAcceptedField.prop('checked', data.userDetail.termsOfUseAccepted).val(data.userDetail.termsOfUseAccepted);
                }
                if (data.userDetail.privacyPolicyAccepted) {
                    privacyPolicyAcceptedField.prop('checked', data.userDetail.privacyPolicyAccepted).val(data.userDetail.privacyPolicyAccepted);
                }
                userDetailId.val(data.userDetail.id);
                userId.val(data.userDetail.userId);

                //genders
                setSelectGenders(genderField, data.userDetail.gender);
                setSelectGenders(nextOfKinGenderField, data.userDetail.nextOfKinGender);
                setSelectCountries(countryCodeField, data.userDetail.countryCode);

                //identity types
                setSelectIdentityTypes(identityTypeField, data.userDetail.identityType);
                setSelectIdentityTypes(nextOfKinIdentityTypeField, data.userDetail.nextOfKinIdentityType);
                setSelectCountries(nextOfKinCountryCodeField, data.userDetail.nextOfKinCountryCode);

                if (data.userDetail.dateOfBirth) {
                    dateOfBirthField.datepicker({
                        todayBtn: "linked",
                        todayHighlight: true,
                        format: 'dd/mm/yyyy',
                        autoclose: true
                    }).on('changeDate', function () {
                    }).datepicker("update", convertDatetimeToDatePickerFormat(data.userDetail.dateOfBirth));
                } else {
                    dateOfBirthField.datepicker("setDate", null);
                }
                let legalAgeDate = new Date();
                legalAgeDate.setFullYear(legalAgeDate.getFullYear() - 16);
                dateOfBirthField.datepicker("setEndDate", legalAgeDate);
            }
        });
    };

    //onchange phone number
    $('input[name="phoneNumber"]').on('keyup', function () {
        let phoneNumber = $(this).val();
        if (!phoneNumber.startsWith('+')) phoneNumber = '+' + phoneNumber;
        phoneNumber = phoneNumber.replace(/\s+/g, '');
        $('input[name="phoneNumber"]').val(phoneNumber);
    });

    //onchange alternative phone number
    $('input[name="alternativePhoneNumber"]').on('keyup', function () {
        let alternativePhoneNumber = $(this).val();
        if (!alternativePhoneNumber.startsWith('+')) alternativePhoneNumber = '+' + alternativePhoneNumber;
        alternativePhoneNumber = alternativePhoneNumber.replace(/\s+/g, '');
        $('input[name="alternativePhoneNumber"]').val(alternativePhoneNumber);
    });

    //onchange next of kin phone number
    $('input[name="nextOfKinPhoneNumber"]').on('keyup', function () {
        let nextOfKinPhoneNumber = $(this).val();
        if (!nextOfKinPhoneNumber.startsWith('+')) nextOfKinPhoneNumber = '+' + nextOfKinPhoneNumber;
        nextOfKinPhoneNumber = nextOfKinPhoneNumber.replace(/\s+/g, '');
        $('input[name="nextOfKinPhoneNumber"]').val(nextOfKinPhoneNumber);
    });

});
