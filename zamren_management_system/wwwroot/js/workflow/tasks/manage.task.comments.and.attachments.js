$(document).ready(function () {

    //preview file
    $('#fileModal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget);
        let file = button.data('file');
        let fileViewer = $('#fileViewer');
        let fileModalLabel = $('#fileModalLabel');
        fileModalLabel.text(file);
        fileViewer.attr('src', '/uploads/' + file);
    });

    //upload file
    $('#fileUploadButton').on('click', function () {
        $('#wkf-comment-files').click();
    });

    //Add file preview on file input change
    $('#wkf-comment-files').on('change', function (event) {
        const files = event.target.files;
        const filePreviewContainerPdf = $('#filePreviewContainerPdf');
        const filePreviewContainerImage = $('#filePreviewContainerImage');

        filePreviewContainerImage.empty(); // Clear existing previews
        filePreviewContainerPdf.empty(); // Clear existing previews

        Array.from(files).forEach((file, index) => {
            const fileReader = new FileReader();
            fileReader.onload = function (e) {
                const fileUrl = e.target.result;
                const fileType = file.type;
                let previewHtml = 
                    `<div class="file-preview-tile border-dark border border-1 rounded m-1 p-1" title="${file.name}" data-file-index="${index}" style="float: left">`;

                if (fileType.startsWith('image/')) {
                    previewHtml += `<img src="${fileUrl}" alt="${file.name}" class="img-thumbnail d-block" style="width: 70px; height: 70px;">`;
                } else if (fileType === 'application/pdf') {
                    previewHtml += `<embed src="${fileUrl}" type="application/pdf" class="img-thumbnail d-block" style="width: 70px; height: 70px;">`;
                }

                // previewHtml += `<p class="text-truncate" style="font-size: x-small">${file.name}</p>`;

                previewHtml += `<button type="button" class="btn btn-sm btn-light mt-1 remove-file" aria-label="Remove"> Remove</button>`;

                previewHtml += `</div>`;
                // filePreviewContainerImage.append(previewHtml);

                if (fileType.startsWith('image/')) {
                    filePreviewContainerImage.append(previewHtml);
                } else if (fileType === 'application/pdf') {
                    filePreviewContainerPdf.append(previewHtml);
                }
            };
            fileReader.readAsDataURL(file);
        });
    });

    //Remove file preview
    $(document).on('click', '.remove-file', function () {
        const fileIndex = $(this).parent().data('file-index');
        const fileInput = $('#wkf-comment-files')[0];
        const dataTransfer = new DataTransfer();

        Array.from(fileInput.files).forEach((file, index) => {
            if (index !== fileIndex) {
                dataTransfer.items.add(file);
            }
        });

        fileInput.files = dataTransfer.files;
        $(this).parent().remove();
    });

});