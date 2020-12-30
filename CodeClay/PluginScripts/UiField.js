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
}

function RegisterEditor(editor, getvalue_function, setvalue_function) {
    if (editor) {
        FormatEditor(editor);

		var tableName = editor.cpTableName;
        var fieldName = editor.name;
        var textFieldName = editor.cpTextFieldName;

		if (tableName) {
			fieldName = tableName + "." + fieldName;
            textFieldName = tableName + "." + textFieldName;
		}

		editor.GetEditorValue = getvalue_function;
		editor.SetEditorValue = setvalue_function;

		if (editors && fieldName) {
			editors[fieldName] = editor;
		}

        if (editors && textFieldName) {
            editors[textFieldName] = editor;
        }
	}
}

function RegisterPanel(editorPanel) {
    var formattedFieldName = GetFormattedFieldName(editorPanel);

    if (editorPanels && formattedFieldName) {
        editorPanels[formattedFieldName] = editorPanel;
    }
}

function GetFormattedFieldName(widget) {
    var formattedFieldName = "";

    if (widget) {
        var tableName = widget.cpTableName;
        var fieldName = widget.cpFieldName
        var itemIndex = widget.cpItemIndex;

        if (tableName) {
            formattedFieldName += tableName + "."
        }

        if (fieldName) {
            formattedFieldName += fieldName;
        }

        if (itemIndex >= 0) {
            formattedFieldName += "." + itemIndex;
        }
    }

    return formattedFieldName;
}

function GetField(tableName, fieldName) {
	if (tableName && fieldName) {
        var fieldValue = dxClientState.Get(tableName + "." + fieldName);
        if (fieldValue != null) {
            return fieldValue;
        }
	}

	return '';
}

function InitField(tableName, fieldName, fieldValue) {
    if (tableName && fieldName) {
        dxClientState.Set(tableName + "." + fieldName, fieldValue);
    }
}

function SetField(tableName, fieldName, fieldValue) {
    if (tableName && fieldName) {
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

function SetEditorEditable(tableName, fieldName, editable) {
    var editor = editors[tableName + "." + fieldName];

    if (editor) {
        editor.cpEditable = editable;

        FormatEditor(editor);
    }
}

function FormatEditor(editor) {
    if (editor) {
        var mandatory = editor.cpMandatory;
        var editable = editor.cpEditable;
        var transparent = editor.cpTransparent;
        var visible = editor.cpVisible;
        var autoBlank = editor.cpAutoBlank
        var tableName = editor.cpTableName;
        var isHtml = editor.cpIsHtml
        var fieldName = editor.name;

        var mainElement = editor.GetMainElement();
        var inputElement = isHtml ? null : editor.GetInputElement();

        if (typeof isHtml == 'undefined') { isHtml = false; }

        if (mainElement) {
            if (visible) {
                editor.SetVisible(true);
                if (editable) {
                    if (transparent) {
                        mainElement.style.backgroundColor = "Transparent";
                        if (!isHtml) {
                            inputElement.style.backgroundColor = "Transparent";
                        }
                    }
                    else if (mandatory) {
                        mainElement.style.backgroundColor = "LightPink";
                        if (!isHtml) {
                            inputElement.style.backgroundColor = "LightPink";
                        }
                    }
                    else {
                        mainElement.style.backgroundColor = "PaleGoldenrod";
                        if (!isHtml) {
                            inputElement.style.backgroundColor = "PaleGoldenrod";
                        }
                    }
                    if (!isHtml) editor.SetReadOnly(false);
                }
                else {
                    mainElement.style.backgroundColor = "Transparent";
                    if (!isHtml) {
                        inputElement.style.backgroundColor = "Transparent";
                    }
                    editor.SetReadOnly(true);
                    if (autoBlank) {
                        SetEditorValue(tableName, fieldName, '');
                    }
                }
            }
            else {
                editor.SetVisible(false);
            }
        }
    }
}

function FormatField(tableName, fieldName, mandatory, editable, visible) {
    var editor = editors[tableName + "." + fieldName];

    editor.cpMandatory = mandatory;
    editor.cpEditable = editable;
    editor.cpVisible = visible;

    FormatEditor(editor);
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
            var fieldName = editor.name;
            dxFieldExitPanel.PerformCallback(fieldName + LIST_SEPARATOR + GetField(tableName, fieldName));
        }
    }
    else if (leader) {
        RefreshFirstFollower(leader);
    }
}

function RunKeyPress(sender, key) {
    var dxField = sender;
    var tableName = dxField.cpTableName;
    var dxTable = tables[tableName];

    if (dxTable && dxTable.IsEditing()) {
        switch (key) {
            case 13:
                dxTable.Command = "Update";
                dxTable.UpdateEdit();
                break;
            case 27:
                dxTable.Command = "Cancel";
                dxTable.CancelEdit();
                break;
        }
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
	RefreshNextFollower(leader);
}

function RefreshNextFollower(leader) {
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
            }

            if (editorPanel) {
                var followerName = editorPanel.cpFieldName;
                var followerValue = GetField(tableName, followerName);
                var followerNameValuePair = followerName + LIST_SEPARATOR + followerValue;

                var leaderName = "";
                var leaderValue = "";
                var leaderNameValuePair = "";
                if (leader) {
                    leaderName = leader.name;
                    leaderValue = GetField(tableName, leaderName);
                    leaderNameValuePair = LIST_SEPARATOR + leaderName + LIST_SEPARATOR + leaderValue;
                    editorPanel.cpLeader = leader;
                }

                if (fieldLoadingPanel && !fieldLoadingPanel.GetVisible()) {
                    fieldLoadingPanel.Show();
                }

                editorPanel.PerformCallback(followerNameValuePair + leaderNameValuePair);
            }
        }
    }
    else if (fieldLoadingPanel && fieldLoadingPanel.GetVisible()) {
        fieldLoadingPanel.Hide();
    }
}