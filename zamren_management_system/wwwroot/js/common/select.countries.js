$(document).ready(function () {
    /**
     * Set the select field with the list of countries
     * @param selectField - The select field to populate with
     * @param selectedValue - Selected value to set in the select field
     */
    window.setSelectCountries = function (selectField, selectedValue) {
        ajaxDataRequest('/api/common/constants/get-countries', 'GET', null, null, function (err, data) {
            if (err) {
                console.error('An error occurred:', err);
            } else {
                let countries = data.countries.map((country) => ({
                    id: country.countryCode,
                    text: country.countryName
                }));

                $(selectField).select2({
                    data: countries,
                    templateResult: formatCountry,
                    templateSelection: formatCountrySelection
                });

                if (selectedValue) {
                    $(selectField).val(selectedValue).trigger('change');
                } else {
                    $.get("https://ipinfo.io?token=e8492ddd3d3fb4", function (response) {
                        if (response.country)
                            $(selectField).val(response.country).trigger('change');
                    }, "jsonp");
                }
            }
        });
    };

    // This function will be used to format the country options with flags
    function formatCountry(country) {
        if (!country.id) {
            return country.text;
        }
        return $(
            '<span><img src="/img/country-flags/' + country.element.value.toLowerCase() + '.png"  alt="' + country.text + '"/> ' + country.text + '</span>'
        );
    }

    // This function will be used to format the selected country with its flag
    function formatCountrySelection(country) {
        if (!country.id) {
            return country.text;
        }
        let $country = $(
            '<span><img src="/img/country-flags/' + country.element.value.toLowerCase() + '.png"  alt="' + country.text + '"/> ' + country.text + '</span>'
        );
        $('#flag-icon').html('<img src="/img/country-flags/' + country.element.value.toLowerCase() + '.png" alt="' + country.text + '"/>');
        return $country;
    }
});
