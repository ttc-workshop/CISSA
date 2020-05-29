function SetFormValidation(formId) {
    $("#" + formId + " input[validatable=\"true\"]").each(function () {
        SetValidator(formId, $(this).attr("name"));
    });

    $("#" + formId).submit(function () {
        var hasErrors = false;
        $("span.field-validation-error").each(function () {
            if ($(this).text() != "") {
                hasErrors = true;
            }
        });
        if (hasErrors) {
            alert("Исправьте ошибки перед сохранением.");
        }
        return !hasErrors;
    });
}

function SetValidator(formId, elementId) {
    var input = $("#" + formId + " input[name=\"" + elementId + "\"]");
    var span = $("#" + formId + " span[name=\"" + elementId + "\"]");
    input.focusout(
        function () {
            $.getJSON("../Form/CheckField", { fieldId: elementId, value: input.val() }, function (json) {
                span.text(json.Error);
                if (json.Error != "") {
                    input.addClass("input-validation-error");
                }
                else {
                    input.removeClass("input-validation-error");
                }
            });
        }).focusout();
}