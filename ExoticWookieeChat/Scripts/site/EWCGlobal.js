function Logout() {
    document.cookie = 'WookieeAuthCookie=; Max-Age=-99999999;';
    window.location.href = "/Auth/Logout";
}


$(document).ready(function () {
    $("#sendMessageBtn").on("click", SendMessage);
    $('#messageInput').keyup(function (e) {
        if (e.keyCode === 13) {
            SendMessage();
        }
    });
});