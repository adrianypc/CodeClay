function dxTextBox_Init(sender, event) {
    var dxTextBox = sender;
    var tableName = dxTextBox.cpTableName;
    var fieldName = dxTextBox.name;
    var fieldValue = dxTextBox.GetText();

    InitField(tableName, fieldName, fieldValue);

    RegisterEditor(dxTextBox,
		function () { return this.GetText(); },
		function (value) { this.SetText(value); }
	);
}

function dxTextBox_KeyPress(sender, event) {
    var key = event.htmlEvent.keyCode;

    switch (key) {
        case 13:
            dxTextBox_ValueChanged(sender, event);
            break;
    }

    RunKeyPress(sender, key);
}

function dxTextBox_ValueChanged(sender, event) {
    var dxTextBox = sender;
    var tableName = dxTextBox.cpTableName;
    var fieldName = dxTextBox.name;
    var fieldValue = dxTextBox.GetText();

    SetField(tableName, fieldName, fieldValue);

    RunExitMacro(dxTextBox);
}

function dxTextPanel_Init(sender, event) {
    var dxTextPanel = sender;

    RegisterPanel(dxTextPanel);
}

function dxTextPanel_EndCallback(sender, event) {
    RefreshNextFollower(sender.cpLeader);
}