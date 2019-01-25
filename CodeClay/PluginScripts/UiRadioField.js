function dxRadioBox_Init(sender, event) {
    var dxRadioBox = sender;
    var tableName = dxRadioBox.cpTableName;
    var fieldName = dxRadioBox.name;
    var fieldValue = dxRadioBox.GetValue();

    InitField(tableName, fieldName, fieldValue);

    RegisterEditor(dxRadioBox,
		function () { return this.GetValue(); },
		function (value) { this.SetValue(value); }
	);
}

function dxRadioBox_ValueChanged(sender, event) {
    var dxRadioBox = sender;
    var tableName = dxRadioBox.cpTableName;
    var fieldName = dxRadioBox.name;
    var fieldValue = dxRadioBox.GetValue();

    SetField(tableName, fieldName, fieldValue);

    RunExitMacro(dxRadioBox);
}

function dxRadioPanel_Init(sender, event) {
    var dxRadioPanel = sender;

    RegisterPanel(dxRadioPanel);
}

function dxRadioPanel_EndCallback(sender, event) {
    RefreshNextFollower();
}