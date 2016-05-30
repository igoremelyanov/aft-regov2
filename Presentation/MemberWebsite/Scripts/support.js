
openLiveChat = function () {
    openWindow('https://server.iad.liveperson.net/hc/14408447/?cmd=file&file=visitorWantsToChat&site=14408447&SV!skill=--Default%20Skill--&leInsId=14408447971909872&skId=-1&leEngId=14408447_9f5218d6-66ac-4c0e-a472-3628f058db39&leEngTypeId=7&leEngName=8080WIN_INDEX_FLOAT_default&leRepAvState=3&referrer=(button%20dynamic-button:8080WIN_INDEX_FLOAT_default(%u5E78%u904B%u8349))%20http%3A//www.8080win.com/Home/Event');
}

openWindow = function (url, name, width, height, resizable) {
    var x = 0, y = 0, w = 800, h = 600, r = 0; // default value: width=800, height=600
    if (width) w = width;
    if (height) h = height;
    if (resizable) r = resizable;
    try {
        x = (window.screen.width - w) / 2;
        y = (window.screen.height - h) / 2;
    } catch (e) {
    }
    var features = "resizable=" + resizable + ", scrollbars=1, left=" + x + ", top=" + y + ", width=" + w + ", height=" + h;
    if (name) {
        name = name.replace(" ", "");
    }
    var win = window.top.open(url, name, features);
    if (win) win.focus();
    return win;
}