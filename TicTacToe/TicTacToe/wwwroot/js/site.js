var interval;
function EmailConfirmation(email) {
    if (window.WebSocket) {
        //alert("Gniazda WebSockets są aktywne");
        openSocket(email, "Email");
    }
    else {
        //alert("Gniazda WebSockets nie są aktywne");
        interval = setInterval(function() {
            CheckEmailConfirmationStatus(email);
        }, 5000);
    }
}

function GameInvitationConfirmation(id) {
    if (window.WebSocket) {
        //alert("Gniazda WebSockets są aktywne");
        openSocket(id, "GameInvitation");
    }
    else {
        //alert("Gniazda WebSockets nie są aktywne");
        interval = setInterval(function() {
            CheckGameInvitationConfirmationStatus(id);
        }, 5000);
    }
}
function CheckEmailConfirmationStatus(email) {
    $.get("/CheckEmailConfirmationStatus?email=" + email, function (data) {
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
    else if (strAction == "GameInvitation") {
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
            var data = $.parseJSON(response.data);

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

function CheckGameInvitationConfirmationStatus(id) {
    $.get("/GameInvitationConfirmation?id=" + id, function (data) {
        if (data.result === "OK") {
            if (interval !== null) {
                clearInterval(interval);
            }
            window.location.href = "/GameSession/Index/" + id;
        }  
    });
}
function SetGameSession(gdSessionId, strEmail) {
    window.GameSessionId = gdSessionId;
    window.EmailPlayer = strEmail;
    window.TurnNumber = 0;
}

$(document).ready(function () {
    $(".btn-SetPosition").click(function () {
        var intX = $(this).attr("data-X");
        var intY = $(this).attr("data-Y");
        SendPosition(window.GameSessionId, window.EmailPlayer, intX, intY);
    })
})

function SendPosition(gdSession, strEmail, intX, intY) {
    var port = document.location.port ? (":" + document.location.port) : "";
    var url = document.location.protocol + "//" + document.location.hostname + port + "/restapi/v1/SetGamePosition/" + gdSession;
    var obj = {
        "Email": strEmail, "x": intX, "y": intY
    };

    var json = JSON.stringify(obj);
    $.ajax({
        'url': url,
        'accepts': "application/json; charset=utf-8",
        'contentType': "application/json",
        'data': json,
        'dataType': "json",
        'type': "POST"
    });
}
function EnableCheckTurnIsFinished() {
    interval = setInterval(function () {
        CheckTurnIsFinished();
    }, 2000);
}

function CheckTurnIsFinished() {
    var port = document.location.port ? (":" + document.location.port) : "";
    var url = document.location.protocol + "//" + document.location.hostname + port + "/restapi/v1/GetGameSession/" + window.GameSessionId;

    $.get(url, function (data) {
        if (data.turnFinished === true && data.turnNumber >= window.TurnNumber) {
            CheckGameSessionIsFinished();
            ChangeTurn(data);
        }
    });
}

function ChangeTurn(data) {
    var turn = data.turns[data.turnNumber - 1];
    DisplayImageTurn(turn);

    $("#activeUser").text(data.activeUser.email);
    if (data.activeUser.email !== window.EmailPlayer) {
        DisableBoard(data.turnNumber);
    }
    else {
        EnableBoard(data.turnNumber);
    }
}

function DisableBoard(turnNumber) {
    var divBoard = $("#gameBoard");
    divBoard.hide();
    $("#divAlertWaitTurn").show();
    window.TurnNumber = turnNumber;
}

function EnableBoard(turnNumber) {
    var divBoard = $("#gameBoard");
    divBoard.show();
    $("#divAlertWaitTurn").hide();
    window.TurnNumber = turnNumber;
}

function DisplayImageTurn(turn) {
    var c = $("#c_" + turn.y + "_" + turn.x);
    var name;

    if (turn.iconNumber === "1") {
        name = 'radio-button-off-sharp';
    }
    else {
        name = 'close-sharp';
    }

    c.html('<ion-icon name="' + name + '"></ion-icon>');
}
function CheckGameSessionIsFinished() {
    var port = document.location.port ? (":" + document.location.port) : "";
    var url = document.location.protocol + "//" + document.location.hostname + port + "/restapi/v1/CheckGameSessionIsFinished/" + window.GameSessionId;

    $.get(url, function (data) {
        debugger;
        if (data.indexOf("wygrał") > 0 || data == "Gra zakończyła się remisem.") {
            alert(data);
            window.location.href = document.location.protocol + "//" + document.location.hostname + port;
        }
    });
}