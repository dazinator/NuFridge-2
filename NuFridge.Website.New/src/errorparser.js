import {bindingMode, customElement, bindable, inject} from 'aurelia-framework';

export class errorParser{
    
    constructor() {

    }

    parseResponse(responseMessage) {
        var self = this;
        if (responseMessage.statusCode) {
            if (responseMessage.statusCode === 500) {
                if (responseMessage.response) {
                    if (responseMessage.response.indexOf('<!DOCTYPE html PUBLIC') === 0) {
                        var elements = $(responseMessage.response);
                        var stackTraceElement = $('#errorContents', elements);
                        var errorText = null;
                        if (stackTraceElement.length === 1) {
                            errorText = $(stackTraceElement).text();
                            var nancyStart = errorText.indexOf("Oh noes! --->");
                            if (nancyStart > 0) {
                                errorText = errorText.substr(nancyStart + 14);
                            }
                        }
                        return self.returnMessage("There was an error processing the request.", errorText);
                    } else {
                        return self.returnMessage(responseMessage.response);
                    }
                } else if (responseMessage.responseText) {
                    return self.returnMessage(responseMessage.responseText);
                } else {
                    return self.returnMessage("There was an error processing the request. Check the server logs for more information.");
                }
            } else if (responseMessage.statusCode === 405) {
                return self.returnMessage("The server has rejected the request. Check the server logs for more information.");
            } else {
                return self.returnMessage("There was an error processing the request. Check the server logs for more information.");
            }
        } else {
            return self.returnMessage("There was an error processing the request. Check the server logs for more information.");
        }
    }

    returnMessage(text, stackTrace) {
        return { Message: text, StackTrace: stackTrace };
    }
}