function dxCurrencyBox_Init(sender, event) {
    var dxCurrencyBox = sender;
    var tableName = dxCurrencyBox.cpTableName;
    var fieldName = dxCurrencyBox.name;
    var fieldValue = dxCurrencyBox.GetText();

    InitField(tableName, fieldName, fieldValue);

    RegisterEditor(dxCurrencyBox,
		function () { return this.GetText(); },
		function (value) { this.SetText(value); }
	);
}

function dxCurrencyBox_ValueChanged(sender, event) {
    var dxCurrencyBox = sender;
    var tableName = dxCurrencyBox.cpTableName;
    var fieldName = dxCurrencyBox.name;
    var fieldValue = dxCurrencyBox.GetText();

    SetField(tableName, fieldName, fieldValue);

    RunExitMacro(dxCurrencyBox);
}

function dxCurrencyPanel_Init(sender, event) {
    var dxCurrencyPanel = sender;

    RegisterPanel(dxCurrencyPanel);
}

function dxCurrencyPanel_EndCallback(sender, event) {
    RefreshNextFollower();
}