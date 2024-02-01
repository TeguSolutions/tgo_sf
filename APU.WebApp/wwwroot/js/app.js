//#region 

//#endregion


window.pingServer = async () => {
    try {
        // This would call some method that triggers a server-side action.
        await DotNet.invokeMethodAsync('APU.WebApp', 'Ping');
    } catch (e) {
        let x = e;
    }
};

//#region dev

var dev = dev || {};

dev.sw = {};

dev.sw.startTime = null;
dev.sw.endTime = null;

dev.sw.start = function () {
    dev.sw.startTime = new Date();
}
dev.sw.end = function (event) {
    dev.sw.endTime = new Date();
    const timeDiff = dev.sw.endTime - dev.sw.startTime; //in ms
    const seconds = Math.round(timeDiff);
    console.log(event + ": " + seconds + " ms");
}

//#endregion



window.blazorExtensions = {

    WriteCookie: function (name, value, days) {

        var expires;
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toGMTString();
        }
        else {
            expires = "";
        }
        document.cookie = name + "=" + value + expires + "; path=/";
    }
}

function openFileDialog() {
    document.getElementById('fileInput').click();
}

//#region ResizeObserver

var resizeObserver = resizeObserver || {};

resizeObserver.observer = null;

resizeObserver.start = function (dotnetObject, id, dotnetCallback) {
    const element = document.getElementById(id);

    if (resizeObserver.observer === null) {
        resizeObserver.observer = new ResizeObserver(entries => resizeObserver.callback(entries, dotnetObject, dotnetCallback));
    }

    resizeObserver.observer.observe(element);
}
resizeObserver.callback = function (entries, dotNetObject, dotnetCallback) {
    for (let entry of entries) {
        dotNetObject.invokeMethodAsync(dotnetCallback, entry.contentRect.width, entry.contentRect.height);
    }
}
resizeObserver.stop = function (id) {
    const element = document.getElementById(id);
    resizeObserver.observer.unobserve(element);
}

//#endregion

//#region IntersectionObserver

function observeIntersectionElement(dotNetObject, id, dotnetCallback, threshold) {

    let thresHold = 0.2;
    if (threshold !== null)
        thresHold = threshold;

    const options = {
        //root: document.documentElement,
        threshold: thresHold
    };

    const element = document.getElementById(id);

    const observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            /*console.log("element - " + id + " is in view: " + entry.intersectionRatio);*/
            if (entry.intersectionRatio > 0.2) {
                /*console.log(entry.intersectionRatio + " dotnet function called: " + dotnetCallback);*/
                dotNetObject.invokeMethodAsync(dotnetCallback);
                observer.unobserve(element);
            }

        });
    }, options);

    observer.observe(element);
}

//#endregion


//#region Width/Height

function getClientHeight(id) {
    try {
        const element = document.getElementById(id);
        return element.clientHeight;

    } catch (e) {
        return 0;
    }
}

function getScrollHeight(id) {
    try {
        const element = document.getElementById(id);
        return element.scrollHeight;

    } catch (e) {
        return 0;
    }
}

function getBoundingClientRect(element) {
    const clientRect = element.getBoundingClientRect();
    return clientRect;
}

//#endregion



function windowScrollToTop() {
    window.scrollTo(0, 0);
}



function openFileDialog(id) {
    document.getElementById(id).click();
}


//#region textarea

function textareaAddAutoResizeListener(id) {
    try {
        const textarea = document.getElementById(id);
        textarea.setAttribute("style", "height:" + (textarea.scrollHeight) + "px;overflow-y:hidden;");
        textarea.addEventListener("input", textareaOnInput, false);
    } catch (e) {

    } 

    //const tx = document.getElementsByTagName("textarea");
    //for (let i = 0; i < tx.length; i++) {
    //    tx[i].setAttribute("style", "height:" + (tx[i].scrollHeight) + "px;overflow-y:hidden;");
    //    tx[i].addEventListener("input", textareaOnInput, false);
    //}
}

function textareaAddAutoResizeListenerForAll() {
    const tx = document.getElementsByTagName("textarea");
    for (let i = 0; i < tx.length; i++) {
        tx[i].setAttribute("style", "height:" + (tx[i].scrollHeight) + "px;overflow-y:hidden;");
        tx[i].addEventListener("input", textareaOnInput, false);
    }
}

function textareaOnInput() {
    this.style.height = "auto";
    this.style.height = (this.scrollHeight) + "px";
}

//#endregion