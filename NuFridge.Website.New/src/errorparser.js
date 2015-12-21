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
                        var message = responseMessage.response;
                        if (responseMessage.responseType === "json" && message) {
                            message = JSON.parse(message);
                        }
                        return self.returnMessage("There was an error processing the request.", message);
                    }
                } else if (responseMessage.responseText) {
                    return self.returnMessage(responseMessage.responseText);
                } else {
                    return self.returnMessage("There was an error processing the request. Check the server logs for more information.");
                }
            } else if (responseMessage.statusCode === 405) {
                return self.returnMessage("The server does not accept requests on " + responseMessage.requestMessage.url + ". Check the server logs for more information or raise this as an issue on GitHub.");
            }  else if (responseMessage.statusCode === 400) {
                var responseText = responseMessage.response;
                if (responseMessage.responseType === "json" && responseText) {
                    responseText = JSON.parse(responseText);
                }
                return self.returnMessage("The server has rejected the request. Check the server logs for more information or raise this as an issue on GitHub.", responseText);
            }
            else if (responseMessage.statusCode === 409) {
                if (responseMessage.responseType === "json" && responseMessage.response) {
                    return self.returnMessage(JSON.parse(responseMessage.response));
                } else {
                     return self.returnMessage("There was an error processing the request. The resource already exists."); 
                }
            }
            else if (responseMessage.statusCode === 404) {
                if (responseMessage.responseType === "json" && responseMessage.response) {
                    return self.returnMessage(JSON.parse(responseMessage.response));
                } else {
                    return self.returnMessage("There was an error processing the request. The resource does not exist."); 
                }
            }
            else if (responseMessage.statusCode === 401) {
                    if (responseMessage.responseType === "json" && responseMessage.response) {
                        return self.returnMessage(JSON.parse(responseMessage.response));
                    } else {
                        return self.returnMessage("There was an error processing the request. You are unauthorized."); 
                    }
            }
            else {
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