/**
 * Convert camel case string to disjoint sentence. Example: 'HelloWorld' to 'Hello World'
 * @param inputString
 * @returns {*}
 */
window.convertToDisjointSentence = function (inputString) {
    return inputString.replace(/([a-z])([A-Z])/g, '$1 $2');
}

/**
 * Convert datetime to date. Example: '2021-09-01T00:00:00' to '01 Sep 2021 00:00'
 * @param datetime
 * @returns {string}
 */
window.convertDatetimeToDate = function (datetime) {
    if (!datetime) return '';
    let date = new Date(datetime);
    return date.toLocaleString("en-GB", {
        day: "2-digit",
        month: "short",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
    });
}

/**
 * Convert date time to date picker format. Example: '2021-09-01T00:00:00' to '01/09/2021'
 * @param datetime
 * @returns {string}
 */
window.convertDatetimeToDatePickerFormat = function (datetime) {
    if (!datetime) return '';
    let date = new Date(datetime);
    return date.toLocaleString("en-GB", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
    });
}

/**
 * Populate a select element with data
 * @param list - array of objects [{id: 1, name: 'mango'}, {id: 2, name: 'apple'}]
 * @param selectId
 * @param defaultOptionText
 * @param selectedValue
 * @param optionValueAttribute
 * @param optionTextAttribute
 * @returns {void|*}
 */
window.populateSelectElement = function (list, selectId, defaultOptionText, selectedValue, optionValueAttribute, optionTextAttribute) {
    let selectElement = $('#' + selectId);
    if (!selectElement) selectElement = $('.' + selectId);
    selectElement.empty();
    selectElement.append('<option value="">' + defaultOptionText + '</option>');
    $.each(list, function (index, item) {
        selectElement.append('<option value="' + item[optionValueAttribute] + '">' + item[optionTextAttribute] + '</option>');
    });

    if (selectedValue) selectElement.val(selectedValue);
}

/**
 * Copy text to clipboard
 * @param text
 * @returns {void}
 */
window.copyToClipboard = function (text) {
    const el = document.createElement('textarea');
    el.value = text;
    document.body.appendChild(el);
    el.select();
    document.execCommand('copy');
    document.body.removeChild(el);
}

/**
 * Initialize tooltips
 * @type {HTMLElement[]}
 * @private
 * @param tooltipTriggerList
 */
let tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
let tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl)
});