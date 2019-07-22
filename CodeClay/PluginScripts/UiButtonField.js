function dxButton_Click(sender, event) {
	var dxButton = sender;

    if (dxButton) {
        var formattedFieldName = GetFormattedFieldName(dxButton);

        if (editorPanels && formattedFieldName) {
            var dxButtonPanel = editorPanels[formattedFieldName];

            if (dxButtonPanel) {
                var fieldName = dxButton.cpFieldName;
                var itemIndex = dxButton.cpItemIndex;

                if (fieldName) {
                    if (itemIndex >= 0) {
                        dxButtonPanel.PerformCallback(fieldName + LIST_SEPARATOR + itemIndex);
                    }
                    else {
                        dxButtonPanel.PerformCallback(fieldName);
                    }
                }
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