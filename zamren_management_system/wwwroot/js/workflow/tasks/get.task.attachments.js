$(document).ready(function () {

    //get encrypted reference from url if url contains ?reference=xxx parameter
    let url = new URL(window.location.href);
    let reference = url.searchParams.get("reference");
    let data = {reference: reference ? reference : null};

    ajaxDataRequest('/api/workflow/task/attachment/get-by-reference', 'POST', data, null, function (err, data) {
        // console.log(err, data);
        if (err) {
            // Handle error
            console.error('An error occurred:', err);
        } else {
            // Handle success

            // Populate the comments
        }
    });

    

});