const SENDER_SUPPORT = "support";
const SENDER_CUSTOMER = "customer";

const SocketTypes = {
    MessageExchange: "messageExchange",
    CommandListener: "commandListener"
};


const port = window.location.port ? ':' + window.location.port : '';
const host = "http://" + window.location.hostname + port;
const wsHost = "ws://" + window.location.hostname + port + "/MessageCenter";
const commandListenerHost = "ws://" + window.location.hostname + port + "/MessageCenter?SocketType=" + SocketTypes.CommandListener;

const SocketMessageType = { Normal: 0, System: 1, Command: 2 };

const SocketMessageCommands = {
    ReloadConversationList: "reloadConversationList",
    ReloadSupportInterface: "reloadSupportInterface",
    AlertNewConversation: "alertNewIncomingConversation"
};


var WebSocketConnections = [];
var CurrentConnection;

/**
 * Load selected conversation
 * @param {any} conversationId Conversation Id 
 */
function LoadConversation(conversationId) {
    ClearMessageContainer();

    var conversationUrl = "http://" + window.location.hostname + ':' + window.location.port + "/Support/LoadConversation?ConversationId=" + conversationId;

    console.log(conversationUrl);

    fetch(conversationUrl, {
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        }
    })
    .then(
        function (response) {
            return response.json();
        }
    )
    .then(
        function (response) {
            let item;
            let conversation = response.conversation;

            for (var i = 0; i < response.messages.length; i++) {
                item = response.messages[i];

                let msg = new Message(item);
                $("#MessageContainer").append(msg.getHtmlContent());
            }

            let scrollHeight = document.getElementById('MessageContainer').scrollHeight;

            $("#MessageContainer").scrollTop(scrollHeight);
            $("#ConversationItem_" + conversationId).addClass("active");




            $("#openConnectionBtn").on("click", function () {
                if (conversation.State === 0 || conversation.State === 1) {
                    OpenWebSocket(item.Conversation.SocketGuid);
                } else {
                    alert("Conversation was closed!");
                }
            });

            for (var j = 0; j < WebSocketConnections.length; j++) {
                var connection = WebSocketConnections[j];
                if (connection.customerWebSocketGuid === customerWebSocketGuid) {
                    CurrentConnection = connection;
                    $("#messageInput").removeAttr("disabled");
                }
            }
        }
    );
}

/**
 * Open a WebSocket to customer
 * @param {any} customerWebSocketGuid Guid of Customer WebSocket Id
 */
function OpenWebSocket(customerWebSocketGuid) {

    for (var i = 0; i < WebSocketConnections.length; i++) {
        var connection = WebSocketConnections[i];
        if (connection.customerWebSocketGuid === customerWebSocketGuid) {
            CurrentConnection = connection;
        }
    }

    if (typeof (CurrentConnection) === "undefined" || CurrentConnection.customerWebSocketGuid !== customerWebSocketGuid) {
        var WebSocketHostToCustomer = wsHost + "?customerWebSocketGuid=" + customerWebSocketGuid;
        console.log(WebSocketHostToCustomer);
        var ws = new WebSocket(WebSocketHostToCustomer);

        ws.onopen = function () {
            $("#messageInput").removeAttr("disabled");
            $("#Status").text("Connected");
        };

        ws.onmessage = function (evt) {
            var msg = new Message(evt.data);
            $("#MessageContainer").append(msg.getHtmlContent());

            let scrollHeight = document.getElementById('MessageContainer').scrollHeight;
            $("#MessageContainer").scrollTop(scrollHeight);
        };

        ws.onerror = function (evt) {
            $("#Status").text(evt.message);
        };

        ws.onclose = function () {
            for (var i = 0; i < WebSocketConnections.length - 1; i++) {
                if (WebSocketConnections[i] === CurrentConnection) {
                    WebSocketConnections.splice(i, 1);
                }
            }
            $("#Status").text("Disconnected");
        };

        CurrentConnection = new WSConnection(customerWebSocketGuid, ws);
        WebSocketConnections.push(CurrentConnection);
    }
}

/**
 * Send employee message to customer
 */
function SendMessage() {
    console.log("send message");

    if (typeof (CurrentConnection) !== "undefined") {
        var ws = CurrentConnection.WebSocketObj;
        
        if (ws.readyState === WebSocket.OPEN && $("#messageInput").val() !== "") {

            var NewMessage = {
                Content: $("#messageInput").val()
            };
            
            ws.send(JSON.stringify(NewMessage));

            $("#messageInput").val("");
        } else {
            $("#status").text("Connection is closed");
        }
    }
}

function WSConnection(customerWebSocketGuid, WebSocketObj) {
    this.customerWebSocketGuid = customerWebSocketGuid;
    this.WebSocketObj = WebSocketObj;
}

function ClearMessageContainer() {
    $("#MessageContainer").html("");
    $(".list-group .active").removeClass("active");
}


function Message(rawSocketData) {
    var data = (typeof (rawSocketData) === "object") ? rawSocketData : JSON.parse(rawSocketData);

    this.Content = data.Content;
    this.Command = data.Command;
    this.Employee = data.Employee;
    this.Sender = data.Sender;
    this.Type = data.Type;

    if (this.Type === SocketMessageType.Command) {
        console.log("socket message is a command");
        this.ExecuteCommandMessage();
    }
}

Message.prototype.getHtmlContent = function () {

    if (this.Type === SocketMessageType.Command) {
        return;
    }

    let alignClass = "";

    if (this.Type === SocketMessageType.System)
    {
        alignClass = "message-center";
    }
    else
    {
        if (this.Sender === SENDER_SUPPORT) {
            alignClass = "message-right";
        }
        else if (this.Sender === SENDER_CUSTOMER)
        {
            alignClass = "message-left";
        }
    }

    return "<div class='message " + alignClass + "'>" +
        this.Content +
        "</div>" +
        "<div class='clearfix'></div>";
};

Message.prototype.ExecuteCommandMessage = function () {
    switch (this.Command) {
        case SocketMessageCommands.ReloadConversationList:
            ReloadConversationList();
            break;

        case SocketMessageCommands.ReloadSupportInterface:
            window.location.reload();
            break;

        case SocketMessageCommands.AlertNewConversation:
            alert("New incoming conversiont!");
            break;

        default:
            console.log("No commands availbale...");
            break;
    }
};


function ReloadConversationList() {
    // TODO implement
}

/**
 * Create a WebSocket for listen command messages
 */
function RegisterCommandListenerWebSocket() {

    var commandListenerWebSocket = new WebSocket(commandListenerHost);
    

    commandListenerWebSocket.onopen = function () {
    };

    commandListenerWebSocket.onmessage = function (evt) {
        var msg = new Message(evt.data);
    };

    commandListenerWebSocket.onerror = function (evt) {
    };

    commandListenerWebSocket.onclose = function () {
    };
}

function RemoveConversation(ConversationId) {
    const removeConversationUrl = host + "/Support/RemoveConversation?ConversationId=" + ConversationId;

    fetch(removeConversationUrl)
    .then(
        function (response) {
            return response.json();
        }
    )
    .then(
        function (response) {
            if (response.result === "success")
                $("#ConversationItem_" + ConversationId).remove();
        }
    );

}

$(document).ready(function () {
    $("#ConversationState").on("change", function () {
        console.log("ConversationState", this.value);
        window.location.href = "/Support?State=" + this.value;
    });

    RegisterCommandListenerWebSocket();
});

