function dxMemo_Init(sender, event) {
    var dxMemo = sender;
    var tableName = dxMemo.cpTableName;
    var fieldName = dxMemo.name;
    var fieldValue = dxMemo.GetText();

    SetField(tableName, fieldName, fieldValue);

    RegisterEditor(dxMemo,
		function () { return this.GetText(); },
		function (value) { this.SetText(value); }
	);
}

function dxMemo_ValueChanged(sender, event) {
    var dxMemo = sender;
    var tableName = dxMemo.cpTableName;
    var fieldName = dxMemo.name;
    var fieldValue = dxMemo.GetText();

    SetField(tableName, fieldName, fieldValue);

    RunExitMacro(dxMemo);
}

function dxMemoPanel_Init(sender, event) {
    var dxMemoPanel = sender;

    RegisterPanel(dxMemoPanel);
}

function dxMemoPanel_EndCallback(sender, event) {
    RefreshNextFollower();
}