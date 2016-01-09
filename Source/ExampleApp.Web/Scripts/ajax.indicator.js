// Adds an ajax namespace with functions for updating UI depending on AJAX operations.

ajax = function () {
    // Called when an AJAX request succeeds.
    function onAjaxRequestSucceeded(statusElement) {
        statusElement
            .addClass('glyphicon-ok-circle')
            .addClass('ajax-succeeded')
            .attr('title', 'AJAX request succeeded!');
    }

    // Called when an AJAX request fails.
    function onAjaxRequestFailed(request, type, message, statusElement, autoShowError) {
        var failedText = 'Failed : ' + request.status + ' : ' + type + ' : ' + message + ' : ' + request.responseText;

        statusElement
            .addClass('glyphicon-remove-circle')
            .addClass('ajax-failed')
            .attr('title', failedText);

        if (autoShowError) {
            statusElement.text(failedText);
        }
    }

    // Called when an AJAX request finished, regardless if it was successful or not.
    function onAjaxRequestCompleted(statusElement) {
        statusElement
            .addClass('ajax-completed')
            .removeClass('ajax-running')
            .removeClass('glyphicon-circle-arrow-right');
    }

    // Registers an AJAX request to be monitored and reported.
    // ajaxRequest:   A currently executing AJAX request.
    // statusElement: A jQuery element that should be used to display the status of the AJAX request to the user.
    // autoShowError: A boolean value indicating whether error message content will be displayed (true) or require mouse-hover (false).
    function indicate(ajaxRequest, statusElement, autoShowError) {
        // First update the status element to indicate to the user that the request is running.
        statusElement
            .attr('class', 'glyphicon glyphicon-circle-arrow-right ajax-running')
            .attr('title', 'Request submitting, awaiting a response from the server.')
            .text('');

        // Register event handlers on the request.
        ajaxRequest
            .done(function()   { onAjaxRequestSucceeded(statusElement); })
            .always(function() { onAjaxRequestCompleted(statusElement); })
            .fail(function(request, type, message) {
                onAjaxRequestFailed(request, type, message, statusElement, autoShowError);
            });
    }

    return {
        // Define what gets exported in the parent namespace
        indicate:indicate
    }
}();