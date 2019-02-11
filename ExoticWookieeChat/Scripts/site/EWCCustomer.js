const SENDER_CUSTOMER = "customer";
const port = window.location.port ? (':' + window.location.port) : '';
const wsHost = "ws://" + window.location.hostname + port + "/MessageCenter";

var ws;

function SendMessage() {
    if (typeof(ws) === "undefined") {
        CreateWebSocket();
    }

    waitForSocketConnection(ws, function () {
        if (ws.readyState === WebSocket.OPEN && $("#messageInput").val() !== "") {
            var NewMessage = {
                Content: $("#messageInput").val()
            };

            ws.send(JSON.stringify(NewMessage));

            $("#messageInput").val("");
        } else {
            $("#Status").text("Connection is closed");
        }
    });
}

function waitForSocketConnection(socket, callback) {
    setTimeout(
        function () {
            if (socket.readyState === 1) {
                if (callback !== null) {
                    $("#Status").text("Connected");
                    callback();
                }
                return;

            } else {
                waitForSocketConnection(socket, callback);
            }

        }, 100);
}

function CreateWebSocket() {
    ws = new WebSocket(wsHost);
    ws.onopen = function () {
        isConnectionEstablished = true;
    };

    ws.onmessage = function (evt) {
        var scrollHeight = document.getElementById('MessageContainer').scrollHeight;
        var msg = new Message(evt.data);
        $("#MessageContainer").append(msg.getHtmlContent());
        $("#MessageContainer").scrollTop(scrollHeight);

        /*var h1 = document.getElementById('MessageContainer').clientHeight;
        var h2 = document.getElementById('MessageContainer').offsetHeight;
        var h3 = document.getElementById('MessageContainer').scrollHeight;*/

    };

    ws.onerror = function (evt) {
        $("#Status").text(evt.message);
        isConnectionError = true;
    };

    ws.onclose = function () {
        $("#Status").text("Disconnected");
    };
}

function Message(rawSocketData) {
    var data = (typeof(rawSocketData) === "object") ? rawSocketData : JSON.parse(rawSocketData);

    this.Content = data.Content;
    this.Sender = data.Sender;
}

Message.prototype.getHtmlContent = function () {
    let alignClass = this.Sender === SENDER_CUSTOMER ? "message-right" : "message-left";
    return "<div class='message " + alignClass + "'>" +
        this.Content +
        "</div>" +
        "<div class='clearfix'></div>";
};

function handleConversationVisibility() {
    $container = $(".ewc-container");
    if ($container.is(":visible")) {
        $container.fadeOut();
    } else {
        $container.fadeIn();
    }
}