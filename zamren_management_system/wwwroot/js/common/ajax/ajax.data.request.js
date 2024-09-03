$(document).ready(function () {
    /**
     * Make an AJAX call to the server using the provided URL, method, data, button object, and callback function
     * @param url - The URL to send the AJAX request to
     * @param method - The HTTP method to be used in the AJAX call
     * @param data - The json data to be sent to the server as FormData
     * @param button - The button that was clicked to submit the form (optional) - if provided, the button will be disabled and its text will be changed to "Loading..."
     * @param callback - A function to be executed after the AJAX call is complete. The function should take two parameters: error and response.
     */
    window.ajaxDataRequest = function (url, method, data, button, callback) {
        // console.log(data); 

        let btnOriginalText = '';
        if (button) btnOriginalText = button.text();
        if (button) button.html(`Loading... <span>&#8635;</span>`).prop('disabled', true);

        //get data and add to form-data
        let formData = new FormData();
        if (data) {
            for (let key in data) {
                formData.append(key, data[key]);
            }
        }

        $.ajax({
            url: url,
            type: method,
            data: formData,
            dataType: 'json',
            cache: false,
            processData: false,
            contentType: false,
            success: function (response) {
                // console.log(response);
                if (button) button.text(btnOriginalText).prop('disabled', false);
                if (response.success) {
                    callback(null, response);
                } else {
                    // console.log(response);
                    callback(response, null);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR, textStatus, errorThrown);
                if (button) button.text(btnOriginalText).prop('disabled', false);
                // callback(errorThrown, null);
                callback({message: "An error occurred while processing your request. Please try again later.",}, null);
            },
        });
    };
});
