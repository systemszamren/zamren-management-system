$(document).ready(function () {

    /**
     * Get the user's profile picture
     * @param element - The element to set the profile picture on (e.g. img, div)
     * @param type - [src,bg] The type of element to set the profile picture on (e.g. src, background-image)
     */
    window.getCurrentUserProfilePictureNavbar = function (element, type) {

        ajaxDataRequest('/api/common/attachment/get-current-user-profile-picture', 'GET', null, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                // console.error('An error occurred:', err);
            } else {
                // Handle success
                if (data.filePath) {
                    if (type === 'src')
                        $(element).attr("src", data.filePath);

                    if (type === 'bg')
                        $(element).css("background-image", "url('" + data.filePath + "')");

                    //add more here below...
                }
            }
        });
    };

    getCurrentUserProfilePictureNavbar($(".user-profile-picture"), 'src');

});
