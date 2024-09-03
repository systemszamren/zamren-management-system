$(document).ready(function () {
    /**
     * Make an AJAX call to the server using the provided URL, method, data, button object, and callback function
     * @param url - The URL to send the AJAX request to
     * @param method - The HTTP method to be used in the AJAX call
     * @param form - The form element to be sent to the server as FormData. Button text will be changed to "Loading..." and disabled during the AJAX call.
     * @param callback - A function to be executed after the AJAX call is complete. The function should take two parameters: error and response.
     */
    window.ajaxFormDataSubmit = function (url, method, form, callback) {
        // console.log(form);

        let formData = new FormData(form[0]);
        let button = form.find('button[type="submit"]');

        let btnOriginalText = button.text();
        if (button) button.html(`Loading... <span>&#8635;</span>`).prop('disabled', true);

        $.ajax({
            url: url,
            type: method,
            data: formData,
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
