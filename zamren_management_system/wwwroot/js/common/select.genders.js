$(document).ready(function () {
    /**
     * Set the select field with the list of genders
     * @param selectField - The select field to populate with
     * @param selectedValue - Selected value to set in the select field
     */
    window.setSelectGenders = function (selectField, selectedValue) {
        ajaxDataRequest('/api/common/constants/get-genders', 'GET', null, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                //populate select field
                $(selectField).empty(); // clear the select field
                $(selectField).append(`<option value="">- Select -</option>`);
                data.genders.forEach(function (gender) {
                    let selected = (selectedValue === gender) ? 'selected' : '';
                    $(selectField).append(`<option value="${gender}" ${selected}>${gender}</option>`);
                });
            }
        });
    };
});