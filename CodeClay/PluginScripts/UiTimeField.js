function dxTimeBox_Init(sender, event) {
    var dxTimeBox = sender;
    var tableName = dxTimeBox.cpTableName;
    var fieldName = dxTimeBox.name;
    var fieldValue = dxTimeBox.GetDate();

    InitField(tableName, fieldName, fieldValue);

    RegisterEditor(dxTimeBox,
		function () { return this.GetDate(); },
        function (value) { if (value) { this.SetDate(value); } else { this.SetValue(null); } }
	);
}

function dxTimeBox_KeyPress(sender, event) {
    var key = event.keyCode || event.which;

    switch (key) {
        case 13:
            dxTimeBox_ValueChanged(sender, event);
            break;
    }

    RunKeyPress(sender, key);
}

function dxTimeBox_ValueChanged(sender, event) {
    var dxTimeBox = sender;
    var tableName = dxTimeBox.cpTableName;
    var fieldName = dxTimeBox.name;
    var fieldValue = dxTimeBox.GetDate();

    SetField(tableName, fieldName, fieldValue);

    RunExitMacro(dxTimeBox);
}

function dxTimePanel_Init(sender, event) {
    var dxTimePanel = sender;

    RegisterPanel(dxTimePanel);
}

function dxTimePanel_EndCallback(sender, event) {
    RefreshNextFollower(sender.cpLeader);
}