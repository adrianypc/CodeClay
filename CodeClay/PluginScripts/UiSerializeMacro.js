function dxImportPUX_FileUploadComplete(sender, event) {
    var script = sender.cpScript;

    if (script) {
        eval(script);
    }

    window.close();
    if (window.parent) {
        window.parent.HidePopup();
    }
}