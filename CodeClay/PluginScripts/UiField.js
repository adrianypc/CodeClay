var editors = {};
var editorPanels = {};
var fieldExitPanels = {};
var fieldLoadingPanel = null;
var leader = null;
var followerFieldNames = null;
var followerFieldIndex = 0;

function dxLabel_Init(sender, event) {
    var dxLabel = sender;
    var tableName = dxLabel.cpTableName;
    var fieldName = dxLabel.name;
    var fieldValue = dxLabel.GetText();

    InitField(tableName, fieldName, fieldValue);
}

function dxLabelPanel_Init(sender, event) {
    var dxLabelPanel = sender;

    RegisterPanel(dxLabelPanel);
}

function dxFieldExitPanel_Init(sender, event) {
    var dxFieldExitPanel = sender;

    if (dxFieldExitPanel) {
        var tableName = dxFieldExitPanel.cpTableName;

        if (tableName) {
            fieldExitPanels[tableName] = dxFieldExitPanel;
        }
        else {
            fieldExitPanels[0] = dxFieldExitPanel;
        }
    }
}

function dxFieldExitPanel_EndCallback(sender, event) {
    var dxFieldExitPanel = sender;

    if (dxFieldExitPanel) {
        var script = dxFieldExitPanel.cpScript;
        if (script) {
            eval(script);
        }
    }

    if (leader) {
        RefreshFirstFollower(leader);
    }
}

function RegisterEditor(editor, getvalue_function, setvalue_function) {
	if (editor) {
		var tableName = editor.cpTableName;
        var fieldName = editor.name;
        var alternateFieldName = editor.cpAlternateName;

		if (tableName) {
			fieldName = tableName + "." + fieldName;
            alternateFieldName = tableName + "." + alternateFieldName;
		}

		editor.GetEditorValue = getvalue_function;
		editor.SetEditorValue = setvalue_function;

		if (editors && fieldName) {
			editors[fieldName] = editor;
		}

        if (editors && alternateFieldName) {
            editors[alternateFieldName] = editor;
        }
	}
}

function RegisterPanel(editorPanel) {
    if (editorPanel) {
        var formattedFieldName = editorPanel.cpTableName + "." + editorPanel.cpFieldName;

        if (editorPanel.cpItemIndex >= 0) {
            formattedFieldName += "." + editorPanel.cpItemIndex;
        }

		if (editorPanels && formattedFieldName) {
			editorPanels[formattedFieldName] = editorPanel;
        }
	}
}

function GetField(tableName, fieldName) {
	if (tableName && fieldName) {
		return dxClientState.Get(tableName + "." + fieldName);
	}

	return null;
}

function InitField(tableName, fieldName, fieldValue) {
    if (tableName && fieldName) {
        dxClientState.Set(tableName + "." + fieldName, fieldValue);
    }
}

function SetField(tableName, fieldName, fieldValue) {
    if (tableName && fieldName) {
        if (fieldValue == null || fieldValue.length == 0) {
            // Set empty field to alarm character
            fieldValue = String.fromCharCode(7);
        }
        dxClientState.Set(tableName + "." + fieldName, fieldValue);
    }
}

function GetEditorValue(tableName, fieldName) {
	var editor = editors[tableName + "." + fieldName];

	if (editor) {
		return editor.GetEditorValue();
	}

	return null;
}

function SetEditorValue(tableName, fieldName, fieldValue) {
	var editor = editors[tableName + "." + fieldName];

	if (editor) {
		SetField(tableName, fieldName, fieldValue);
		editor.SetEditorValue(fieldValue);
	}
}

function SetEditorFocus(tableName, fieldName) {
    var editor = editors[tableName + "." + fieldName];

    if (editor) {
        editor.Focus();
    }
}

function RunExitMacro(editor) {
    leader = editor;

    if (editor && editor.cpHasFieldExitMacro) {
        var tableName = editor.cpTableName;
        var dxFieldExitPanel = null;

        if (tableName) {
            dxFieldExitPanel = fieldExitPanels[tableName];
        }
        else {
            dxFieldExitPanel = fieldExitPanels[0];
        }

        if (dxFieldExitPanel) {
            dxFieldExitPanel.PerformCallback(editor.name);
        }
    }
    else if (leader) {
        RefreshFirstFollower(leader);
    }
}

function RefreshFirstFollower(leader) {
	// Refresh the 1st follower field in list of names
	var followerFields = leader.cpFollowerFields;

	if (followerFields) {
		followerFieldNames = followerFields.split(LIST_SEPARATOR);
	}
	else {
		followerFieldNames = null;
	}

	followerFieldIndex = -1;
	RefreshNextFollower();
}

function RefreshNextFollower() {
    // Refresh the next follower field in list of names
    // This 'chaining' approach forces each refresh to occur
    // sequentially rather than in parallel
    if (followerFieldNames && ++followerFieldIndex < followerFieldNames.length) {
        var fieldName = followerFieldNames[followerFieldIndex];
        if (fieldName && editorPanels) {
            editorPanel = editorPanels[fieldName];

            var tableName = editorPanel.cpTableName;

            if (tableName) {
                fieldLoadingPanel = loadingPanels[tableName];

                if (fieldLoadingPanel && !fieldLoadingPanel.GetVisible()) {
                    fieldLoadingPanel.Show();
                }
            }

            if (editorPanel) {
                editorPanel.PerformCallback();
            }
        }
    }
    else if (fieldLoadingPanel && fieldLoadingPanel.GetVisible()) {
        fieldLoadingPanel.Hide();
    }
}

function RefreshFollowers(followerFieldNames) {
    return;

	var followerFields = followerFieldNames.split(LIST_SEPARATOR);

	for (var i = 0; i < followerFields.length; i++) {
		var fieldName = followerFields[i];
		if (fieldName && editorPanels) {
			editorPanel = editorPanels[fieldName];
			if (editorPanel) {
				editorPanel.PerformCallback();
			}
		}
	}
}