$(document).ready(function () {
    $("#purchaseRequisitionForm").submit(function (e) {
        e.preventDefault();

        let form = $(this);

        ajaxFormDataSubmit("/api/procurement/purchase-requisition-good/submit-purchase-requisition-form", "POST", form, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                //redirect to /Admin/User
                window.location.href = "/BackOffice/Dashboard";
            }
        });
    });
});