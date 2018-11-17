function dxSpinEdit_Init(sender, event) {
    var dxSpinEdit = sender;
    var tableName = dxSpinEdit.cpTableName;
    var fieldName = dxSpinEdit.name;
    var fieldValue = dxSpinEdit.GetText();

    SetField(tableName, fieldName, fieldValue);

    RegisterEditor(dxSpinEdit,
		function () { return this.GetNumber(); },
		function (value) { this.SetNumber(value); }
	);
}

function dxSpinEdit_ValueChanged(sender, event) {
    var dxSpinEdit = sender;
    var tableName = dxSpinEdit.cpTableName;
    var fieldName = dxSpinEdit.name;
    var fieldValue = dxSpinEdit.GetText();

    SetField(tableName, fieldName, fieldValue);

    RunExitMacro(dxSpinEdit);
}

function dxSpinEditPanel_Init(sender, event) {
    var dxSpinEditPanel = sender;

    RegisterPanel(dxSpinEditPanel);
}

function dxSpinEditPanel_EndCallback(sender, event) {
    RefreshNextFollower();
}