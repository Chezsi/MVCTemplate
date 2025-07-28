$(document).ready(function () {
    $("#createUserForm").on("submit", function (e) {
        e.preventDefault();

        const form = $(this);
        const actionUrl = form.attr("action");
        const formData = form.serialize();

        $.post(actionUrl, formData)
            .done(function (res) {
                if (res.success) {
                    $("#createUserError").addClass("d-none");
                    $("#createUserSuccess").removeClass("d-none").text(res.message);
                    setTimeout(() => location.reload(), 1000);
                } else {
                    $("#createUserError").removeClass("d-none").text(res.errors.join(", "));
                    $("#createUserSuccess").addClass("d-none");
                }
            })
            .fail(function () {
                $("#createUserError").removeClass("d-none").text("Something went wrong.");
                $("#createUserSuccess").addClass("d-none");
            });
    });
});