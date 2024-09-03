$(document).ready(function () {
    $(document).on('click', '.previewNextOfKinProofOfResidencyAttachmentBtn,.previewNextOfKinIdentityAttachmentBtn,.previewIdentityAttachmentBtn,.previewProofOfResidencyAttachmentBtn', function () {
        let btn = $(this)

        let id = btn.val();
        let labelText = btn.data('label');

        ajaxDataRequest('/api/common/attachment/' + id, 'GET', null, btn, function (err, data) {
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
                alert(err.message)
            } else {
                // Handle success
                // console.log('Received data:', data);
                if (data.attachment.contentType === 'application/pdf') {
                    let previewAttachmentModal = $('#previewAttachmentModal');
                    previewAttachmentModal.find('#previewAttachmentModalLabel').text(labelText);
                    previewAttachmentModal.find('#attachmentPreview').attr('src', data.attachment.filePath);
                    previewAttachmentModal.modal('show');
                } else if (data.attachment.contentType === 'image/png' || data.attachment.contentType === 'image/jpg' || data.attachment.contentType === 'image/jpeg') {
                    let previewImageModal = $('#previewImageModal');
                    previewImageModal.find('#previewImageModalLabel').text(labelText);
                    previewImageModal.find('#imagePreview').attr('src', data.attachment.filePath);
                    previewImageModal.modal('show');
                }
            }
        });
    });
});
