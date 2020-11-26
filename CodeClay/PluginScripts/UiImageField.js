var dxImagePanel = null;
var dxDeleteImage = null;
var dxEditImage = null;
var dxUploadImage = null;
var dxUpdateImage = null;

function dxImage_Init(sender, event) {
    // Do nothing
}

function dxImagePanel_Init(sender, event) {
    dxImagePanel = sender;

    RegisterPanel(dxImagePanel);
}

function dxImagePanel_EndCallback(sender, event) {
    RefreshNextFollower(sender.cpLeader);
}

function dxDeleteImage_Init(sender, event) {
    dxDeleteImage = sender;
}

function dxDeleteImage_Click(sender, event) {
    if (dxDeleteImage) {
        dxDeleteImage.SetVisible(false);
    }

    if (dxEditImage) {
        var tableName = dxEditImage.cpTableName;
        var fieldName = dxEditImage.cpFieldName;
        var fieldValue = "";

        SetField(tableName, fieldName, fieldValue);

        dxEditImage.SetVisible(false);
        dxEditImage.SetImageUrl("");

        RunExitMacro(dxEditImage);
    }

    if (dxUploadImage) {
        dxUploadImage.SetVisible(true);
    }

    if (dxUpdateImage) {
        dxUpdateImage.SetVisible(true);
    }
}

function dxEditImage_Init(sender, event) {
    dxEditImage = sender;

    var tableName = dxEditImage.cpTableName;
    var fieldName = dxEditImage.cpFieldName;
    var fieldValue = dxEditImage.cpShortImageUrl;

    InitField(tableName, fieldName, fieldValue);
}

function dxUploadImage_Init(sender, event) {
    dxUploadImage = sender;
}

function dxUploadImage_FileUploadComplete(sender, event) {
    var callbackDataString = event.callbackData;

    if (callbackDataString) {
        var callbackData = callbackDataString.split(LIST_SEPARATOR);
        var fileName = callbackData[0];
        var imageUrl = callbackData[1];

        if (dxDeleteImage) {
            dxDeleteImage.SetVisible(true);
        }

        if (dxEditImage) {
            var tableName = dxEditImage.cpTableName;
            var fieldName = dxEditImage.cpFieldName;
            var fieldValue = imageUrl;

            SetField(tableName, fieldName, fieldValue);

            dxEditImage.SetVisible(true);
            dxEditImage.SetImageUrl(imageUrl);
        }

        if (dxUploadImage) {
            dxUploadImage.SetVisible(false);
        }
    }
}