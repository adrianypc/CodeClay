function dxDateBox_Init(sender, event) {
    var dxDateBox = sender;
    var tableName = dxDateBox.cpTableName;
    var fieldName = dxDateBox.name;
    var fieldValue = dxDateBox.GetDate();

    InitField(tableName, fieldName, fieldValue);

    RegisterEditor(dxDateBox,
		function () { return this.GetDate(); },
        function (value) { if (value) { this.SetDate(value); } else { this.SetValue(null); } }
	);
}

function dxDateBox_KeyPress(sender, event) {
    var key = event.keyCode || event.which;

    switch (key) {
        case 13:
            dxDateBox_ValueChanged(sender, event);
            break;
    }

    RunKeyPress(sender, key);
}

function dxDateBox_ValueChanged(sender, event) {
    var dxDateBox = sender;
    var tableName = dxDateBox.cpTableName;
    var fieldName = dxDateBox.name;
    var fieldValue = dxDateBox.lastSuccessText;

    SetField(tableName, fieldName, fieldValue);

    RunExitMacro(dxDateBox);
}

function dxDatePanel_Init(sender, event) {
    var dxDatePanel = sender;

    RegisterPanel(dxDatePanel);
}

function dxDatePanel_EndCallback(sender, event) {
    RefreshNextFollower(sender.cpLeader);
}