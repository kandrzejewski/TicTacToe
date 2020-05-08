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

function GameInvitationConfirmation(id) {
    if (window.WebSocket) {
        alert("Gniazda WebSockets są aktywne");
        openSocket(id, "GameInvitation");
    }
    else {
        alert("Gniazda WebSockets nie są aktywne");
        interval = setInterval(function () {
            CheckGameInvitationConfirmationStatus(id);
        }, 2000);
    }
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

var openSocket = function (parameter, strAction) {
    if (interval !== null)
        clearInterval(interval);
    var protocol = location.protocol === "https:" ? "wss:" : "ws:";
    var operation = "";
    var wsUri = "";

    if (strAction == "Email") {
        wsUri = protocol + "//" + window.location.host + "/CheckEmailConfirmationStatus";
        operation = "CheckEmailConfirmationStatus";
    }
    else if (strAction == "Gameinvitation") {
        wsUri = protocol + "//" + window.location.host + "/GameInvitationConfirmation";
        operation = "CheckGameInvitationConfirmationStatus";
    }

    var socket = new WebSocket(wsUri);
    socket.onmessage = function (response) {
        console.log(response);
        if (strAction == "Email" && response.data == "OK") {
            window.location.href = "/GameInvitation?email=" + parameter;
        }
        else if (strAction == "GameInvitation") {
            var data = $.praseJSON(response.data);
            if (data.Result == "OK")
                window.location.href = "/GameSession/Index/" + data.Id;
        }
    };

    socket.onopen = function () {
        var json = JSON.stringify({
            "Operation": operation,
            "Parameters": parameter
        });
        socket.send(json);
    };

    socket.onclose = function (event) {
    };
};