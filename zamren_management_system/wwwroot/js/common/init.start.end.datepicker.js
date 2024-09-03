$(document).ready(function () {

    let startDate = $("#startDate");
    let endDate = $("#endDate");

    startDate.datepicker({
        todayBtn: "linked",
        todayHighlight: true,
        format: 'dd/mm/yyyy',
        startDate: "today",
        autoclose: true
    }).on('changeDate', function () {
        // alert("start date changed")
        // endDate.datepicker("setStartDate", startDate.datepicker("getDate"));
    });

    endDate.datepicker({
        // todayBtn: "linked",
        todayHighlight: true,
        format: 'dd/mm/yyyy',
        startDate: "today",
        autoclose: true
    }).on('changeDate', function (e) {
    });

});