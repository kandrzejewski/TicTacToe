var interval;
function EmailConfirmation(email) {
    interval = setInterval(function() {
        CheckEmailConfirmationStatus(email);
    }, 1000);
}