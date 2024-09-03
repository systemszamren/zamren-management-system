$(document).ready(function () {

    loadFilePreviewDiv();

    function loadFilePreviewDiv() {

        let div = $('#file-preview-div');

        if (!div.length) return;

        let inputname = div.data('inputname'); //multiple or one
        let quantity = div.data('quantity'); //multiple or one
        let required = div.data('required'); //true or false
        let filetypes = div.data('filetypes'); //e.g: .png,.jpg,.jpeg,.pdf
        let title = div.data('title'); //e.g: supportingDocuments or supportingDocument or Attachment or Image, etc

        // console.log(quantity, required, filetypes, title)

        if (quantity === 'multiple') {
            quantity = 'multiple'
        } else if (quantity === 'one') {
            quantity = ''
        } else {
            return;
        }

        if (required === false) {
            required = `<i>(Optional - Max File Size: 10MB)</i>`;
        } else if (required === true) {
            required = `<i>(Max File Size: 10MB)</i><span class="text-danger">*</span>`;
        } else {
            return;
        }

        if (!title) return;
        if (!filetypes) return;
        if (!inputname) return;

        div.html(`<div class="row">
                        <div class="form-group col-sm-12 mb-0 pb-0">
                            <label for="file">` + title + ` ` + required + `</label>
                            <input type="file" class="file-field" name="` + inputname + `" accept="` + filetypes + `"
                                   ` + quantity + ` style="visibility: hidden; position: absolute;">
                            <button type="button" class="browse btn btn-dark btn-block" style="display: block;width: 100%">
                                Browse <i class="fa fa-file-o"></i>
                            </button>
                            <input type="text" class="form-control bg-transparent m-0 p-0" disabled
                                   placeholder="Select Files" id="file" style="visibility: hidden; height: 20px;">

                        </div>
                        <div class="col-sm-12 preview-div-img m-1">
                                <img src="~/wwwroot/temp/temp.png" id="preview"
                                    style="width: 100px; height: 100px" class="img-thumbnail" alt="image">
                        </div>
                        <div class="col-sm-12 preview-div-pdf m-1">
                        </div>
                        <div class="col-sm-12 preview-div-clear m-1">
                            <button type="button"
                                    class="btn btn-light btn-sm bg-light text-dark clear-button">
                                Clear All files <i class="fa fa-close"></i>
                            </button>
                        </div>
                    </div>`)
    }


    $(document).on("click", ".browse", function () {
        let file = $(this).parents().find(".file-field");
        file.trigger("click");
    });

    $('.preview-div-clear').hide();

    $('.file-field').change(function (e) {
        $('.preview-div-pdf, .preview-div-img').html(``);
        $('.preview-div-clear').show();

        let files = e.target.files;
        if (files) {
            for (let i = 0; i < files.length; i++) {
                let file = files[i];
                let reader = new FileReader();

                reader.onload = function (e) {
                    let fileData = e.target.result;
                    let fileName = file.name;

                    if (fileName.endsWith('.pdf')) {// For PDF files
                        $('.preview-div-pdf').append('<embed title="' + fileName + '" src="' + fileData + '" width="150" height="150" type="application/pdf" class="img-thumbnail">');

                    } else if (fileName.endsWith('.png') || fileName.endsWith('.jpg') || fileName.endsWith('.jpeg')) {// For image files (PNG, JPG, JPEG)

                        let img = `
                                    <div class="img-div-` + i + `" style="width: 150px; height: 150px; position: relative;">
                                        <button type="button" value="` + i + `" class="btn bg-white btn-sm text-dark clear-button" style="position: absolute; right: 0; top: 0; outline: none"> <i class="fa fa-close"></i> </button>
                                        <img title="` + fileName + `" src="` + fileData + `" alt="` + fileName + `" style="width: 100%; height: 100%" class="img-thumbnail">
                                    </div>`;
                        $('.preview-div-img').append(img);
                    }
                };
                reader.readAsDataURL(file);
            }
        }
    });

    $(document).on('click', '.clear-button', function () {
        clearSelectedFiles();
    });
    
    /*window.clearSelectedFile = function clearSelectedFile(index) {
        //check if $('.preview-div-img') is empty
        let previewDivImg = $('.preview-div-img');
        let previewDivPdf = $('.preview-div-pdf');
        let imgDiv = $('.preview-div-img').html();
        if (imgDiv === '') {
            $('.preview-div-pdf, .preview-div-img').html(``)
            $('.preview-div-img').html(`<img src="/assets/portal/images/80x80.png" id="preview"
                                             style="width: 100px; height: 100px" class="img-thumbnail" alt="image">`);
        }
    }*/

    window.clearSelectedFiles = function clearSelectedFiles() {
        let fileField = $('.file-field');
        fileField.val('');
        $('.preview-div-pdf, .preview-div-img').html(``)
        $('.preview-div-img').html(`<img src="/assets/portal/images/80x80.png" id="preview"
                                             style="width: 100px; height: 100px" class="img-thumbnail" alt="image">`);
        $('.preview-div-clear').hide();
        // console.log('files removed')
    }
});