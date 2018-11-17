function dxComboBox_Init(sender, event) {
    var dxComboBox = sender;
    var tableName = dxComboBox.cpTableName;
    var fieldName = dxComboBox.name;
    var fieldValue = dxComboBox.GetValue();

    SetField(tableName, fieldName, fieldValue);

    RegisterEditor(dxComboBox,
		function () { return this.GetText(); },
		function (value) { this.SetText(value); }
	);
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
    RefreshNextFollower();
}