function dxComboBox_Init(sender, event) {
    var dxComboBox = sender;
    var tableName = dxComboBox.cpTableName;
    var fieldName = dxComboBox.name;
    var fieldValue = dxComboBox.GetValue();

    InitField(tableName, fieldName, fieldValue);

    RegisterEditor(dxComboBox,
		function () { return this.GetText(); },
		function (value) { if (this.GetText() != value) this.SetText(value); }
	);
}

function dxComboBox_KeyPress(sender, event) {
    var key = event.keyCode || event.which;

    switch (key) {
        case 13:
            dxComboBox_ValueChanged(sender, event);
            break;
    }

    RunKeyPress(sender, key);
}

function dxComboBox_ValueChanged(sender, event) {
    var dxComboBox = sender;
    var tableName = dxComboBox.cpTableName;
    var fieldName = dxComboBox.name;
    var fieldValue = dxComboBox.GetValue();

    SetField(tableName, fieldName, fieldValue);

    RunExitMacro(dxComboBox);
}

function dxComboPanel_Init(sender, event) {
    var dxComboPanel = sender;

    RegisterPanel(dxComboPanel);
}

function dxComboPanel_EndCallback(sender, event) {
    RefreshNextFollower(sender.cpLeader);
}