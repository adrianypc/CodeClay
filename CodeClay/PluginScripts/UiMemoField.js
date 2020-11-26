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

function dxHtmlMemo_Init(sender, event) {
    var dxHtmlMemo = sender;
    var tableName = dxHtmlMemo.cpTableName;
    var fieldName = dxHtmlMemo.name;
    var fieldValue = dxHtmlMemo.GetHtml();

    dxHtmlMemo.SetHeight(dxHtmlMemo.GetMainElement().scrollHeight + 5);

    InitField(tableName, fieldName, fieldValue);

    RegisterEditor(dxHtmlMemo,
        function () { return this.GetHtml(); },
        function (value) { this.SetHtml(value); }
    );
}

function dxMemo_KeyPress(sender, event) {
    var dxMemo = sender;
    var key = event.htmlEvent.keyCode;

    if (dxMemo) {
        switch (key) {
            case 10:
                dxMemo.SetHeight(dxMemo.GetInputElement().scrollHeight + 15);
                var oldText = dxMemo.GetText();
                var caretPos = dxMemo.GetCaretPosition();
                var newText1 = oldText.substr(0, caretPos);
                var newText2 = oldText.substr(caretPos, oldText.length - caretPos);
                dxMemo.SetText(newText1 + '\n' + newText2);
                dxMemo.SetCaretPosition(caretPos + 1);
                break;

            case 13:
                dxMemo_ValueChanged(sender, event);

            default:
                RunKeyPress(sender, key);
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

function dxHtmlMemo_HtmlChanged(sender, event) {
    var dxHtmlMemo = sender;
    var tableName = dxHtmlMemo.cpTableName;
    var fieldName = dxHtmlMemo.name;
    var fieldValue = dxHtmlMemo.GetHtml();

    SetField(tableName, fieldName, fieldValue);

    RunExitMacro(dxHtmlMemo);
}

function dxMemoPanel_Init(sender, event) {
    var dxMemoPanel = sender;

    RegisterPanel(dxMemoPanel);
}

function dxMemoPanel_EndCallback(sender, event) {
    RefreshNextFollower(sender.cpLeader);
}