var dxLinkPanel = null;
var dxDelete = null;
var dxEditLink = null;
var dxUpload = null;
var dxUpdateText = null;
var dxUpdateLink = null;

function dxLink_Init(sender, event) {
    // Do nothing
}

function dxLinkPanel_Init(sender, event) {
    dxLinkPanel = sender;

    RegisterPanel(dxLinkPanel);
}

function dxLinkPanel_EndCallback(sender, event) {
    RefreshNextFollower();
}

function dxDelete_Init(sender, event) {
    dxDelete = sender;
}

function dxDelete_Click(sender, event) {
    if (dxDelete) {
        dxDelete.SetVisible(false);
    }

    if (dxEditLink) {
        var tableName = dxEditLink.cpTableName;
        var fieldName = dxEditLink.cpFieldName;
        var fieldValue = "";

        SetField(tableName, fieldName, fieldValue);

        dxEditLink.SetVisible(false);
        dxEditLink.SetNavigateUrl("");

        RunExitMacro(dxEditLink);
    }

    if (dxUpload) {
        dxUpload.SetVisible(true);
    }

    if (dxUpdateText) {
        dxUpdateText.SetVisible(true);
    }

    if (dxUpdateLink) {
        dxUpdateLink.SetVisible(true);
    }
}

function dxEditLink_Init(sender, event) {
    dxEditLink = sender;

    var tableName = dxEditLink.cpTableName;
    var fieldName = dxEditLink.cpFieldName;
    var fieldValue = dxEditLink.cpShortNavigateUrl;

    InitField(tableName, fieldName, fieldValue);
}

function dxUpload_Init(sender, event) {
    dxUpload = sender;
}

function dxUpload_FileUploadComplete(sender, event) {
    var callbackDataString = event.callbackData;

    if (callbackDataString) {
        var callbackData = callbackDataString.split(LIST_SEPARATOR);
        var fileName = callbackData[0];
        var linkUrl = callbackData[1];

        if (dxDelete) {
            dxDelete.SetVisible(true);
        }

        if (dxEditLink) {
            var tableName = dxEditLink.cpTableName;
            var fieldName = dxEditLink.cpFieldName;
            var fieldValue = linkUrl;

            SetField(tableName, fieldName, fieldValue);

            dxEditLink.SetVisible(true);
            dxEditLink.SetText(fileName);
            dxEditLink.SetNavigateUrl(linkUrl);
        }

        if (dxUpload) {
            dxUpload.SetVisible(false);
        }
    }
}

function dxUpdateText_Init(sender, event) {
    dxUploadText = sender;
}

function dxUpdateText_ValueChanged(sender, event) {
    dxUploadText = sender;

    var tableName = dxUploadText.cpTableName;
    var fieldName = dxUploadText.cpFieldName;
    var fieldValue = dxUploadText.GetText();

    SetField(tableName, fieldName, fieldValue);
}