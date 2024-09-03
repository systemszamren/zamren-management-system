$(document).ready(function () {
    $("#edit-user-details-form").submit(function (e) {
        e.preventDefault();

        //confirm
        let confirmation = confirm("Are you sure you want to update this user's details?");
        if (!confirmation) {
            return;
        }

        let form = $(this);

        ajaxFormSerializeSubmit("/api/client/user/edit-user-details", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                location.reload();
            }
        });
    });

    $("#edit-user-details-form input[name='identityAttachment']").change(function () {

        if (!this.files || !this.files[0]) return alert("No file selected");
        if (!confirm("Are you sure you want to save this file?")) return;
        let userDetailId = $("#edit-user-details-form input[name='userDetailId']").val();

        let data = {
            file: this.files[0],
            userDetailId: userDetailId
        };

        ajaxDataRequest("/api/client/user/upload-identity-attachment", "POST", data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                if (err) alert(err.message);
            } else {
                // Handle success
                if (data) alert(data.message);
                //set btn value to the attachment id: previewIdentityAttachmentBtn
                $(".previewIdentityAttachmentBtn").val(data.attachmentId).prop('disabled', false);
            }
        });
    });

    $("#edit-user-details-form input[name='proofOfResidencyAttachment']").change(function () {

        if (!this.files || !this.files[0]) return alert("No file selected");
        if (!confirm("Are you sure you want to save this file?")) return;
        let userDetailId = $("#edit-user-details-form input[name='userDetailId']").val();

        let data = {
            file: this.files[0],
            userDetailId: userDetailId
        };

        ajaxDataRequest("/api/client/user/upload-proof-of-residency-attachment", "POST", data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                if (err) alert(err.message);
            } else {
                // Handle success
                if (data) alert(data.message);
                //set btn value to the attachment id: previewProofOfResidencyAttachmentBtn | enalbe the button
                $(".previewProofOfResidencyAttachmentBtn").val(data.attachmentId).prop('disabled', false);
            }
        });
    });

    $("#edit-user-details-form input[name='nextOfKinProofOfResidencyAttachment']").change(function () {

        if (!this.files || !this.files[0]) return alert("No file selected");
        if (!confirm("Are you sure you want to save this file?")) return;
        let userDetailId = $("#edit-user-details-form input[name='userDetailId']").val();

        let data = {
            file: this.files[0],
            userDetailId: userDetailId
        };

        ajaxDataRequest("/api/client/user/upload-next-of-kin-proof-of-residency-attachment", "POST", data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                if (err) alert(err.message);
            } else {
                // Handle success
                if (data) alert(data.message);
                //set btn value to the attachment id: previewNextOfKinProofOfResidencyAttachmentBtn
                $(".previewNextOfKinProofOfResidencyAttachmentBtn").val(data.attachmentId).prop('disabled', false);
            }
        });
    });

    $("#edit-user-details-form input[name='nextOfKinIdentityAttachment']").change(function () {

        if (!this.files || !this.files[0]) return alert("No file selected");
        if (!confirm("Are you sure you want to save this file?")) return;
        let userDetailId = $("#edit-user-details-form input[name='userDetailId']").val();

        let data = {
            file: this.files[0],
            userDetailId: userDetailId
        };

        ajaxDataRequest("/api/client/user/upload-next-of-kin-identity-attachment", "POST", data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                if (err) alert(err.message);
            } else {
                // Handle success
                if (data) alert(data.message);
                //set btn value to the attachment id: previewNextOfKinIdentityAttachmentBtn
                $(".previewNextOfKinIdentityAttachmentBtn").val(data.attachmentId).prop('disabled', false);
            }
        });
    });

});