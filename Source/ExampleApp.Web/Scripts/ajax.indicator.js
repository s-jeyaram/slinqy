// Adds an ajax namespace with functions for updating UI depending on AJAX operations.

ajax = function () {
    // Called when an AJAX request succeeds.
    function onAjaxRequestSucceeded(statusElement) {
        statusElement
            .addClass('ajax-succeeded')
            .text('$')
            .attr('title', 'AJAX request succeeded!');
    }

    // Called when an AJAX request fails.
    function onAjaxRequestFailed(request, type, message, statusElement) {
        statusElement
            .addClass('ajax-failed')
            .text('Failed : ' + request.status + ' : ' + type + ' : ' + message + ' : ' + request.responseText);
    }

    // Called when an aJAX request finished, regardless if it was successful or not.
    function onAjaxRequestCompleted(statusElement) {
        statusElement
            .addClass('ajax-completed')
            .removeClass('ajax-running');
    }

    // Registers an AJAX request to be monitored and reported.
    // ajaxRequest: A currently executing AJAX request.
    // statusElement: a jQuery element that should be used to display the status of the AJAX request to the user.
    function indicate(ajaxRequest, statusElement) {
        // First update the status element to indicate to the user that the request is running.
        statusElement
            .text('>')
            .attr('class', 'ajax-running') // reset the class attribute
            .attr('title', 'Request submitting, awaiting a response from the server.');

        // Register event handlers on the request.
        ajaxRequest
            .done(function()   { onAjaxRequestSucceeded(statusElement); })
            .always(function() { onAjaxRequestCompleted(statusElement); })
            .fail(function(request, type, message) {
                 onAjaxRequestFailed(request, type, message, statusElement);
            });
    }

    return {
        // Define what gets exported in the parent namespace
        indicate:indicate
    }
}();