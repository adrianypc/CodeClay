function dxButton_Click(sender, event) {
	var dxButton = sender;

    if (dxButton) {
        var tableName = dxButton.cpTableName;
        var fieldName = dxButton.cpFieldName;
        var itemIndex = dxButton.cpItemIndex;
        var formattedFieldName = tableName + "." + fieldName + "." + itemIndex;

        if (editorPanels && formattedFieldName) {
            var dxButtonPanel = editorPanels[formattedFieldName];

            if (dxButtonPanel) {
                dxButtonPanel.PerformCallback(fieldName + LIST_SEPARATOR + itemIndex);
            }
        }
	}
}

function dxButtonPanel_Init(sender, event) {
    var dxButtonPanel = sender;

    RegisterPanel(dxButtonPanel);
}

function dxButtonPanel_EndCallback(sender, event) {
    var dxButtonPanel = sender;

    if (dxButtonPanel) {
        var script = dxButtonPanel.cpScript;

        if (script) {
            eval(script);
        }
    }
}