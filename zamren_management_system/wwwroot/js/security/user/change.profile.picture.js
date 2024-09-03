$(document).ready(function () {

    $("#profilePictureAttachment").change(function () {

        if (!this.files || !this.files[0]) return;

        let url = new URL(window.location.href);
        let userId = url.pathname.split('/').pop();

        let data = {
            file: this.files[0],
            userId: userId,
        };

        ajaxDataRequest("/api/security/user/change-profile-picture", "POST", data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);

                let profilePicturePath = data.filePath;
                $(".photo-preview").css("background-image", "url('" + profilePicturePath + "')");

                getCurrentUserProfilePictureProfilePage($(".user-profile-picture"), 'src');
            }
        });
    });

});