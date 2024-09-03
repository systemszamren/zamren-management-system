$(document).ready(function () {

    /**
     * Get the user's profile picture
     * @param element - The element to set the profile picture on (e.g. img, div)
     * @param userId - The user's ID
     * @param type - [src,bg] The type of element to set the profile picture on (e.g. src, background-image)
     */
    window.getUserProfilePicture = function (element, userId, type) {
        let data = {
            userId: userId,
        };

        ajaxDataRequest('/api/common/attachment/get-user-profile-picture', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                // console.error('An error occurred:', err);
            } else {
                // Handle success
                // console.log(data.filePath)
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
});
