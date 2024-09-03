$(document).ready(function () {

    $("#profilePictureAttachment").change(function () {

        if (!this.files || !this.files[0]) return;

        let data = {
            file: this.files[0],
        };

        ajaxDataRequest("/api/client/user/change-profile-picture", "POST", data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);

                // $(".photo-preview").css("background-image", "url('" + data.filePath + "')");
                $(".photo-preview-client-view").css("background-image", "url('" + data.filePath + "')");
                $(".user-profile-picture").attr("src", data.filePath);
            }
        });
    });

});