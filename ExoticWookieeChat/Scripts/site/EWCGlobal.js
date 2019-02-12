// This file hold global constants and functions

const SENDER_SUPPORT = "support";
const SENDER_CUSTOMER = "customer";

const port = window.location.port ? ':' + window.location.port : '';
const host = "http://" + window.location.hostname + port;
const messageCenterHost = "ws://" + window.location.hostname + port + "/MessageCenter";

const ConversationStates = {
    New: 0,
    InProgress: 1,
    Closed: 2
};

function Logout() {
    document.cookie = 'WookieeAuthCookie=; Max-Age=-99999999;';
    window.location.href = "/Auth/Logout";
}
