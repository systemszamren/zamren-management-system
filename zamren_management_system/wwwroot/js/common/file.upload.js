$(document).ready(function () {

    /**
     * Submit one file from the file input field to the server
     */
    $("#file-form").submit(function (e) {
        e.preventDefault();

        if (!confirm("Are you sure you want to submit this file?")) return;
        let form = $(this);

        ajaxFormDataSubmit("/api/common/attachment/upload-form-file", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                if (err) alert(err.message);
            } else {
                // Handle success
                if (data) alert(data.message);
                // window.location.href = "/Admin/Role/Edit/" + data.id;
                //reload the page
                location.reload();
            }
        });
    });

    /**
     * Submit one file on change of the file input field
     */
    $("#file-form #file").change(function () {

        if (!this.files || !this.files[0]) return alert("No file selected");

        if (!confirm("Are you sure you want to save this file?")) return;

        let data = {
            file: this.files[0],
            purpose: "file"
        };

        ajaxDataRequest("/api/common/attachment/upload-form-file", "POST", data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                if (err) alert(err.message);
            } else {
                // Handle success
                if (data) alert(data.message);
                // window.location.href = "/Admin/Role/Edit/" + data.id;
                //reload the page
                // location.reload();
            }
        });
    });

    /**
     * Submit multiple files from one input field to the server using the files form
     */
    $("#files-form").submit(function (e) {
        e.preventDefault();

        if (!confirm("Are you sure you want to submit these files?")) return;
        let form = $(this);

        ajaxFormDataSubmit("/api/common/attachment/upload-form-files", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                if (err) alert(err.message);
            } else {
                // Handle success
                if (data) alert(data.message);
                // window.location.href = "/Admin/Role/Edit/" + data.id;
                //reload the page
                location.reload();
            }
        });
    });

});