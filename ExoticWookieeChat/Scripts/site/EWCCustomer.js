/**
 * WebSocket object for customer
 */
var customerWebSocket;

/**
 * Send customer message on WebSocket
 */
function SendCustomerMessage() {
    if (typeof (customerWebSocket) === "undefined") {
        CreateCustomerWebSocket();
    }

    WaitForSocketConnection(customerWebSocket, function () {
        if (customerWebSocket.readyState === WebSocket.OPEN && $("#customerMessageInput").val() !== "") {
            var NewMessage = {
                Content: $("#customerMessageInput").val(),
                Sender: SENDER_CUSTOMER
            };

            customerWebSocket.send(JSON.stringify(NewMessage));

            $("#customerMessageInput").val("");
        } else {
            $("#CustomerConnectionStatus").text("Connection is closed");
        }
    });
}

/**
 * Wait for connection. If connection is connect, call callback function
 * @param {any} socket WebSocker object
 * @param {any} callback callback function
 */
function WaitForSocketConnection(socket, callback) {
    setTimeout(
        function () {
            if (socket.readyState === 1) {
                if (callback !== null) {
                    $("#CustomerConnectionStatus").text("Connected");
                    callback();
                }
                return;

            } else {
                WaitForSocketConnection(socket, callback);
            }

        }, 100);
}

/**
 * Create customer WebSocket for communication
 */
function CreateCustomerWebSocket() {
    customerWebSocket = new WebSocket(messageCenterHost);
    customerWebSocket.onopen = function () {
        isConnectionEstablished = true;
    };

    customerWebSocket.onmessage = function (evt) {
        var scrollHeight = document.getElementById('CustomerMessageContainer').scrollHeight;
        var msg = new CustomerMessage(evt.data);
        $("#CustomerMessageContainer").append(msg.getHtmlContent());
        $("#CustomerMessageContainer").scrollTop(scrollHeight);
    };

    customerWebSocket.onerror = function (evt) {
        $("#CustomerConnectionStatus").text(evt.message);
        isConnectionError = true;
    };

    customerWebSocket.onclose = function () {
        $("#CustomerConnectionStatus").text("Disconnected");
    };
}

/**
 * Customer message prototype
 * @param {any} rawSocketData raw data from the server
 */
function CustomerMessage(rawSocketData) {
    var data = (typeof(rawSocketData) === "object") ? rawSocketData : JSON.parse(rawSocketData);

    this.Content = data.Content;
    this.Sender = data.Sender;
}

/**
 * Extend customer prototype
 * @returns {any} Formatted html
 */
CustomerMessage.prototype.getHtmlContent = function () {
    let alignClass = this.Sender === SENDER_CUSTOMER ? "message-right" : "message-left";
    return "<div class='message " + alignClass + "'>" +
        this.Content +
        "</div>" +
        "<div class='clearfix'></div>";
};

/**
 * Handle customer input visibility
 */
function handleConversationVisibility() {
    $container = $(".ewc-container");
    if ($container.is(":visible")) {
        $container.fadeOut();
    } else {
        $container.fadeIn();
    }
}

/**
 * Register default behavior
 */
$(document).ready(function () {
    $("#sendCustomerMessageBtn").on("click", SendCustomerMessage);
    $('#customerMessageInput').keyup(function (e) {
        if (e.keyCode === 13) {
            SendCustomerMessage();
        }
    });
});