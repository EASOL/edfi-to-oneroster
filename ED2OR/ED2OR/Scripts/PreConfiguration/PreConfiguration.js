var DatabaseSettings = new function () {
    this.TestDatabaseConnection = function (postActionUrl, resultElementClassSelector) {
        $(resultElementClassSelector).hide();
        var formData = $("form").serialize();
        $.ajax({
            url: postActionUrl,
            type: 'POST',
            data: formData,
            data_type: "json",
            success: function (result) {
                if (result["IsSuccessful"]) {
                    $(resultElementClassSelector).text("Connection Successful").removeClass("connectionFailed").addClass("connectionSuccessful");
                }
                else {
                    var errorMsg = result["ErrorMessage"];
                    $(resultElementClassSelector).text("Connection Failed: " + errorMsg).removeClass("connectionSuccessful").addClass("connectionFailed");
                }
                $(resultElementClassSelector).show();
            },
            error: function () {
            }
        });
    };
}
