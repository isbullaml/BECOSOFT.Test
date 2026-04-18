function ConvertImageSourceToBase64(src) {
    var promise = new Promise(function(resolve, reject) {
        var request = new XMLHttpRequest();
        request.open('GET', src, true);
        request.responseType = 'blob';
        request.onload = function () {
            var req = this;
            ConvertFileUploadToBase64(req.response).then(resolve, reject);
        };

        request.send();
    });

    return promise;
}