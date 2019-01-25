function dxMemo_Init(sender, event) {
    var dxMemo = sender;
    var tableName = dxMemo.cpTableName;
    var fieldName = dxMemo.name;
    var fieldValue = dxMemo.GetText();

    dxMemo.SetHeight(dxMemo.GetInputElement().scrollHeight + 5);

    InitField(tableName, fieldName, fieldValue);

    RegisterEditor(dxMemo,
		function () { return this.GetText(); },
		function (value) { this.SetText(value); }
	);
}

function dxMemo_KeyPress(sender, event) {
    var dxMemo = sender;

    if (dxMemo) {
        switch (event.htmlEvent.keyCode) {
            case 13:
                dxMemo.SetHeight(dxMemo.GetInputElement().scrollHeight + 15);
                break;
        }
    }
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