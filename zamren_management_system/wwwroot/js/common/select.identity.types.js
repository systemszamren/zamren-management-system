$(document).ready(function () {
    /**
     * Set the select field with the list of identity types
     * @param selectField - The select field to populate with
     * @param selectedValue - Selected value to set in the select field
     */
    window.setSelectIdentityTypes = function (selectField, selectedValue) {
        ajaxDataRequest('/api/common/constants/get-identity-types', 'GET', null, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                //populate select field
                $(selectField).empty(); // clear the select field
                $(selectField).append(`<option value="">- Select -</option>`);
                data.identityTypes.forEach(function (identityType) {
                    let selected = (selectedValue === identityType.value) ? 'selected' : '';
                    $(selectField).append(`<option value="${identityType.value}" ${selected}>${identityType.text}</option>`);
                });
            }
        });
    };
});
