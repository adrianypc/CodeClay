﻿function dxCheckBox_Init(sender, event) {
    var dxCheckBox = sender;
    var tableName = dxCheckBox.cpTableName;
    var fieldName = dxCheckBox.name;
    var fieldValue = dxCheckBox.GetValue();

    InitField(tableName, fieldName, fieldValue);

    RegisterEditor(dxCheckBox,
		function () { return this.GetChecked(); },
		function (value) { this.SetChecked(value); }
	);
}

function dxCheckBox_ValueChanged(sender, event) {
    var dxCheckBox = sender;
    var tableName = dxCheckBox.cpTableName;
    var fieldName = dxCheckBox.name;
    var fieldValue = dxCheckBox.GetValue();

    SetField(tableName, fieldName, fieldValue);

    RunExitMacro(dxCheckBox);
}

function dxCheckPanel_Init(sender, event) {
    var dxCheckPanel = sender;

    RegisterPanel(dxCheckPanel);
}

function dxCheckPanel_EndCallback(sender, event) {
    RefreshNextFollower();
}