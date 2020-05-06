var interval;
function EmailConfirmation(email) {
    if (window.WebSocket) {
        alert("Gniazda WebSockets są aktywne");
        openSocket(email, "Email");
    }
    else {
        alert("Gniazda WebSockets nie są aktywne")
        interval = setInterval(function () {
            CheckEmailConfirmationStatus(email);
        }, 1000);
    }
}