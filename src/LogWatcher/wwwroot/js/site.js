function showAlert(alert, message) {
    alert.find("p").text(message);
    alert.show();
}

$(function () {

    var alert = $('#alert');

    // enable SignalR logging
    $.connection.hub.logging = true;

    // alert on slow connection
    $.connection.hub.connectionSlow(function () {
        showAlert(alert, 'We are currently experiencing difficulties with the SignalR connection');
    });

    // alert on connection error
    $.connection.hub.error(function (error) {
        showAlert(alert, 'SignalR error: ' + error);
    });

    // alert on reconnected
    $.connection.hub.reconnected(function () {
        showAlert(alert, 'Reconnected to SignalR hub, transport ' + $.connection.hub.transport.name);
    });
});