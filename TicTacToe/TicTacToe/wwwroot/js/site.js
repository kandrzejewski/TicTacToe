var interval;
function EmailConfirmation(email) {
    interval = setInterval(function() {
        CheckEmailConfirmationStatus(email);
    }, 1000);
}
function CheckEmailConfirmationStatus(email) {
    $.get("/CheckEmailConfirmationStatus?email=" + email,
        function (data) {
            if (data === "OK") {
                if (interval !== null)
                    clearInterval(interval);
                window.location.href = "/GameInvitation?email=" + email;
            }
        });
}