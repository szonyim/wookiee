const newItemSign = "<i class='far fa-circle green state-sign' title='New'></i>";
const InProgressItemSign = "<i class='far fa-dot-circle orange state-sign' title='In progress'></i>";
const ClosedItemSign = "<i class='fas fa-circle red state-sign' title='Closed'></i>";

const SocketTypes = {
    MessageExchange: "messageExchange",
    CommandListener: "commandListener"
};

const commandListenerHost = messageCenterHost + "?SocketType=" + SocketTypes.CommandListener;
const SocketMessageType = { Normal: 0, System: 1, Command: 2 };

const SocketMessageCommands = {
    ReloadConversationList: "reloadConversationList",
    ReloadSupportInterface: "reloadSupportInterface",
    AlertNewConversation: "alertNewIncomingConversation",
    AddNewConversation: "addNewConversation"
};

const fetchHeaders = {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
};

var WebSocketConnections = [];
var CurrentConnection;

/**
 * Load selected conversation
 * @param {any} conversationId Conversation Id 
 */
function LoadConversation(conversationId) {
    ClearMessageContainer();
    $("#messageInput").attr("disabled", "disabled");
    var conversationUrl = "http://" + window.location.hostname + ':' + window.location.port + "/Support/LoadConversation?ConversationId=" + conversationId;

    fetch(conversationUrl, {
        headers: fetchHeaders
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
        var WebSocketHostToCustomer = messageCenterHost + "?customerWebSocketGuid=" + customerWebSocketGuid;
        var ws = new WebSocket(WebSocketHostToCustomer);

        ws.onopen = function () {
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

    if (CurrentConnection !== "undefined")
        $("#messageInput").removeAttr("disabled");
}

/**
 * Send employee message to customer
 */
function SendMessage() {
    if (typeof (CurrentConnection) !== "undefined") {
        var ws = CurrentConnection.WebSocketObj;
        
        if (ws.readyState === WebSocket.OPEN && $("#messageInput").val() !== "") {

            var NewMessage = {
                Content: $("#messageInput").val(),
                Sender: SENDER_SUPPORT
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

/**
 * Clear message container
 */
function ClearMessageContainer() {
    $("#MessageContainer").html("");
    $(".list-group .active").removeClass("active");
}

/**
 * Message prototype
 * @param {any} rawSocketData raw data from the server
 */
function Message(rawSocketData) {
    var data = (typeof (rawSocketData) === "object") ? rawSocketData : JSON.parse(rawSocketData);

    this.Content = data.Content;
    this.Command = data.Command;
    this.Employee = data.Employee;
    this.Sender = data.Sender;
    this.Type = data.Type;

    if (this.Type === SocketMessageType.Command) {
        this.ExecuteCommandMessage();
    }
}

/**
 * Extend customer prototype
 * @returns {any} Formatted html
 */
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

/**
 * Extend Message prototype. Execute incoming command messages
 */
Message.prototype.ExecuteCommandMessage = function () {
    switch (this.Command) {
        case SocketMessageCommands.ReloadConversationList:
            ReloadConversationList();
            break;

        case SocketMessageCommands.ReloadSupportInterface:
            window.location.reload();
            break;

        case SocketMessageCommands.AlertNewConversation:
            AlertNewConversation();
            break;

        case SocketMessageCommands.AddNewConversation:
            AddNewConversation(this.Content);
            break;

        default:
            console.log("No commands availbale...");
            break;
    }
};

/**
 * Query new conversation data from server, and insert to list
 * @param {any} conversationId Id of conversation
 */
function AddNewConversation(conversationId) {
    var GetConversationUrl = host + "/Support/GetConversation?ConversationId=" + conversationId;

    fetch(GetConversationUrl)
    .then(function (response) {
        return response.json();
    })
    .then(function (response) {
                const itemHtml =
            "<div id='ConversationItem_" + response.Id + "' class='list-group-item list-group-item-action'>" +
                "<div class='row'>" +
                    "<div class='col-1'>" +
                        "<i class='far fa-circle green state-sign' title='New'></i>" +
                    "</div>" +
                    "<div class='col-10'>" +
                        "<a href='javascript:void(0)' id='conversation_" + response.Id + "' onclick='LoadConversation(" + response.Id + ")'>" +
                            response.SocketGuid +
                        "</a><br/>" +
                        "<small>Now</small>"+
                    "</div>" +
                    "<div class='col-1'>" +
                        "<button href='javascript:void(0)' onclick='RemoveConversation(" + response.Id + ")' class='btn btn-danger btn-sm float-right' title='Delete conversation' style='display:inline'> <i class='far fa-trash-alt'></i> </button>" +
                    "</div>" +
                "</div>" +
            "</div>";

        $("#ConversationList").prepend(itemHtml);
    });
}

/**
 * Show and hide notification about new connection
 */
function AlertNewConversation() {
    $("#newIncomingConversation").slideDown().delay(3000).slideUp();
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

/**
 * Remove one selected conversation from database and view
 * @param {any} ConversationId Id of selected conversation
 */
function RemoveConversation(ConversationId) {
    // TODO Close connections
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
        ReloadConversationList(this.value);
        //window.location.href = "/Support?State=" + this.value;
    });

    RegisterCommandListenerWebSocket();

    $("#sendMessageBtn").on("click", SendMessage);
    $('#messageInput').keyup(function (e) {
        if (e.keyCode === 13) {
            SendMessage();
        }
    });
});

/**
 * Create html item for list
 * @param {any} item Conversation json item
 * @returns {any} Conversation html element
 */
function CreateConversationItemHtml(item) {
    var itemSign;

    if (item.State === ConversationStates.New) {
        itemSign = newItemSign;
    } else if (item.State === ConversationStates.InProgress) {
        itemSign = InProgressItemSign;
    } else {
        itemSign = ClosedItemSign;
    }

    var createdAt = new Date(item.CreatedAt);

    return "<div id='ConversationItem_" + item.Id + "' class='list-group-item list-group-item-action'>" +
            "<div class='row'>" +
                "<div class='col-1'>" +
                    itemSign +
                "</div>" +
                "<div class='col-10'>" +
                    "<a href='javascript:void(0)' id='conversation_" + item.Id + "' onclick='LoadConversation(" + item.Id + ")'>" +
                        item.SocketGuid +
                    "</a><br/>" +
                    "<small>" + createdAt.toLocaleString() + "</small>" +
                "</div>" +
                "<div class='col-1'>" +
                    "<button href='javascript:void(0)' onclick='RemoveConversation(" + item.Id + ")' class='btn btn-danger btn-sm float-right' title='Delete conversation' style='display:inline'> <i class='far fa-trash-alt'></i> </button>" +
                "</div>" +
            "</div>" +
        "</div>";
}

/**
 * Reload conversation list in background
 * @param {any} selectedState Selected state in view
 */
function ReloadConversationList(selectedState) {

    const listConversationUrl = host + "/Support/GetConversationList?State=" + selectedState;

    fetch(listConversationUrl)
    .then(
        function (response) {
            return response.json();
        }
    )
    .then(
        function (response) {
            var htmlItems = $.map(response, CreateConversationItemHtml);
            $("#ConversationList").html(htmlItems);
        }
    );
}