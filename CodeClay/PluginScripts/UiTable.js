var rootTable = null;
var childTable = null;
var tables = {};
var popupMenus = {};
var openMenuPanels = {};
var clickMenuPanels = {};
var loadingPanels = {};

// --------------------------------------------------------------------------------------------------
// Search functions
// --------------------------------------------------------------------------------------------------

function dxSearch_Init(sender, event) {
    var dxSearch = sender;
    var tableName = dxSearch.cpTableName;

    if (tableName) {
        SetField(tableName, "_View", "Search");
    }

    CopyState(dxClientState, dxBackupClientState);
}

function dxSearch_EndCallback(sender, event) {
    var dxSearch = sender;
    var command = dxSearch.Command;
    var tableName = dxSearch.cpTableName;

    switch (command) {
        case "Cancel":
            // For cosmetic purposes when clicking on Inspect button
            ClearState(tableName);
    }
}

function dxSearch_ToolbarItemClick(sender, event) {
	ClickToolbar(sender, event, true);
}

// --------------------------------------------------------------------------------------------------
// Card functions
// --------------------------------------------------------------------------------------------------

function dxCard_Init(sender, event) {
    var dxCard = sender;
    var isRootTable = dxCard.cpIsRootTable;
    var puxFile = dxCard.cpPuxFile;
    var tableName = dxCard.cpTableName;

    if (tableName) {
        SetField(tableName, "_View", "Card");
        tables[tableName] = dxCard;
    }

    if (isRootTable) {
        rootTable = dxCard;
    }

    dxCard.SetFocusedCardIndex(0);
    InitToolbar(dxCard, dxCard.cpDisabledMacros);
}

function dxCard_FocusedCardChanged(sender, event) {
    var dxCard = sender;

    InitToolbar(dxCard, dxCard.cpDisabledMacros);
}

function dxCard_BeginCallback(sender, event) {
	var dxCard = sender;
    var tableName = dxCard.cpTableName;
    var command = dxCard.Command;

    if (typeof command == 'undefined') {
        command = event.command;
    }

    SetCommand(tableName, command);

    if (command == "Update" && dxCard.IsNewCardEditing()) {
        dxCard.Command = "UpdateNew";
    }
}

function dxCard_EndCallback(sender, event) {
    var dxCard = sender;
    var tableName = dxCard.cpTableName;
    var command = dxCard.Command;
    var script = dxCard.cpScript;

    switch (command) {
    	case "New":
        case "Search":
    	case "Edit":
            // Do nothing
    		break;

        case "UpdateNew":
            var insertedRowIndex = dxCard.cpInsertedRowIndex;

            if (insertedRowIndex && insertedRowIndex >= 0) {
                dxCard.GotoPage(insertedRowIndex);
            }
            else {
                insertedRowIndex = 0;
            }

            dxCard.SetFocusedCardIndex(insertedRowIndex);
            dxCard.Command = null;
            childTable = null;
            break;

        case "Update":
            if (rootTable && rootTable.name != dxCard.name && dxCard.cpBubbleUpdate && !dxCard.cpIsInvalid) {
                rootTable.PerformCallback(tableName);
                childTable = dxCard;
            }
            else {
                childTable = null;
            }

    	case "Cancel":
    	case "Delete":
    		// For cosmetic purposes when clicking on Inspect button
            if (!dxCard.cpIsInvalid) {
                ClearState(tableName);
            }
    		break;

    	default:
    		if (command) {
    			dxCard.Command = null;
    			dxCard.Refresh();
    		}
    		break;
    }

    InitAllToolbars(dxCard);

    if (script) {
        dxCard.cpScript = null;
        eval(script);
    }
}

function dxCard_ToolbarItemClick(sender, event) {
    ClickToolbar(sender, event, false);
}

// --------------------------------------------------------------------------------------------------
// Grid functions
// --------------------------------------------------------------------------------------------------

function dxGrid_Init(sender, event) {
	var dxGrid = sender;
    var isRootTable = dxGrid.cpIsRootTable;
    var puxFile = dxGrid.cpPuxFile;
    var tableName = dxGrid.cpTableName;

    if (tableName) {
        SetField(tableName, "_View", "Grid");
    	tables[tableName] = dxGrid;
    }

    if (isRootTable) {
    	rootTable = dxGrid;
    }

    dxGrid.SetFocusedRowIndex(0);
    InitToolbar(dxGrid, dxGrid.cpDisabledMacros);
}

function dxGrid_DetailRowExpanding(sender, event) {
    var dxGrid = sender;
    var rowIndex = event.visibleIndex;

    dxGrid.UnselectAllRowsOnPage();
    dxGrid.SetFocusedRowIndex(rowIndex);
    dxGrid.ExpandedRowIndex = rowIndex;
}

function dxGrid_DetailRowCollapsing(sender, event) {
    var dxGrid = sender;

    dxGrid.ExpandedRowIndex = -1;
}

function dxGrid_BeginCallback(sender, event) {
	var dxGrid = sender;
    var tableName = dxGrid.cpTableName;
    var command = dxGrid.Command;

    if (typeof command == 'undefined') {
        command = event.command;
    }

	SetCommand(tableName, command);

    if (command == "Update" && dxGrid.IsNewRowEditing()) {
        dxGrid.Command = "UpdateNew";
    }
}

function dxGrid_EndCallback(sender, event) {
	var dxGrid = sender;
    var tableName = dxGrid.cpTableName;
    var quickInsert = dxGrid.cpQuickInsert;
    var command = dxGrid.Command;
    var script = dxGrid.cpScript;
    var expandedRowIndex = dxGrid.ExpandedRowIndex;

    switch (command) {
        case "New":
            // Do nothing
            if (expandedRowIndex >= 0) {
                dxGrid.CollapseDetailRow(expandedRowIndex);
            }
            break;

        case "Edit":
            if (expandedRowIndex >= 0 && expandedRowIndex != dxGrid.GetFocusedRowIndex()) {
                dxGrid.CollapseAllDetailRows();
            }
            break;

        case "UpdateNew":
            if (quickInsert && !dxGrid.cpIsInvalid && confirm("Do you wish to add a blank row?")) {
                dxGrid.Command = "New";
                dxGrid.AddNewRow();
            }
        case "Update":
            if (rootTable && rootTable.name != dxGrid.name && dxGrid.cpBubbleUpdate && !dxGrid.cpIsInvalid) {
                rootTable.PerformCallback(tableName);
                childTable = dxGrid;
            }
            else {
                childTable = null;
            }

        case "Cancel":
        case "Delete":
            if (!dxGrid.cpIsInvalid) {
                ClearState(tableName);
            }
            break;

        default:
            if (command) {
                dxGrid.Command = null;
                dxGrid.Refresh();
            }
            break;
    }

    InitAllToolbars(dxGrid);

    if (script) {
        dxGrid.cpScript = null;
        eval(script);
    }
}

function dxGrid_FocusedRowChanged(sender, event) {
	var dxGrid = sender;
    var tableName = dxGrid.cpTableName;
    var checkFocusedRow = dxGrid.cpCheckFocusedRow;

    if (checkFocusedRow && (dxGrid.IsEditing() || dxGrid.IsNewRowEditing())) {
        if (dxGrid.ExpandedRowIndex < 0) {
        	dxGrid.Command = "Update";
        	alert("Saving changes on the last edited record");
            dxGrid.UpdateEdit();
		}
        else {
        	dxGrid.Command = "Cancel";
        	alert("Discarding changes on the last edited record");
            dxGrid.CancelEdit();
            dxGrid.ExpandDetailRow(dxGrid.ExpandedRowIndex);
		}
    }
}

function dxGrid_ToolbarItemClick(sender, event) {
    ClickToolbar(sender, event, false);
}

function dxGrid_ContextMenu(sender, event) {
	var dxGrid = sender;
	var xPos = ASPxClientUtils.GetEventX(event.htmlEvent);
	var yPos = ASPxClientUtils.GetEventY(event.htmlEvent);
	var rowIndex = event.index;
	var tableName = dxGrid.cpTableName;

	dxGrid.UnselectAllRowsOnPage();
	dxGrid.SetFocusedRowIndex(rowIndex);

	if (tableName) {
		var dxOpenMenuPanel = openMenuPanels[tableName];
		var dxLoadingPanel = loadingPanels[tableName];

		if (dxOpenMenuPanel) {
			dxOpenMenuPanel.Grid = dxGrid;
			dxOpenMenuPanel.PerformCallback(xPos + LIST_SEPARATOR + yPos);
		}

		if (dxLoadingPanel) {
			dxLoadingPanel.ShowAtPos(xPos, yPos);
		}
	}
}

function dxGrid_RowDblClick(sender, event) {
	var dxGrid = sender;
	var xPos = ASPxClientUtils.GetEventX(event.htmlEvent);
	var yPos = ASPxClientUtils.GetEventY(event.htmlEvent);
	var tableName = dxGrid.cpTableName;

	if (tableName) {
		var dxOpenMenuPanel = openMenuPanels[tableName];
		var dxLoadingPanel = loadingPanels[tableName];

		if (dxOpenMenuPanel) {
			dxOpenMenuPanel.Grid = dxGrid;
			dxOpenMenuPanel.PerformCallback("");
		}

		if (dxLoadingPanel) {
			dxLoadingPanel.ShowAtPos(xPos, yPos);
		}
	}
}

// --------------------------------------------------------------------------------------------------
// Toolbar functions
// --------------------------------------------------------------------------------------------------

function InitAllToolbars(table) {
    var parentTableName = table.cpParentTableName;

    if (parentTableName) {
        var parentTable = tables[parentTableName];

        if (parentTable) {
            InitToolbar(parentTable, table.cpParentDisabledMacros);
        }
    }

    InitToolbar(table, table.cpDisabledMacros);

    if (childTable) {
        InitToolbar(childTable, childTable.cpDisabledMacros);
    }
}

function InitToolbar(table, disabledButtonList) {
    var toolbar = table.GetToolbar(0);
    var disabledButtons = null;

    if (disabledButtonList) {
        disabledButtons = disabledButtonList.split(LIST_SEPARATOR);
    }

    if (toolbar && disabledButtons) {
        for (var i = 0; i < toolbar.GetItemCount() ; i++) {
            var button = toolbar.GetItem(i);
            var buttonName = button.name;
            var isDisabled = disabledButtons.indexOf(buttonName) > -1;

            button.SetEnabled(!isDisabled);
        }
    }
}

function ClickToolbar(table, event, isSearching) {
    var tableName = table.cpTableName;
    var puxFile = table.cpPuxFile;
    var command = event.item.name;

    if (command != "Inspect") {
        table.Command = command;
        SetCommand(tableName, command);
    }

    switch (command) {
        case "Inspect":
            var message = "";

            var clientCommandContents = dxClientCommand.Get(tableName);
            if (clientCommandContents && clientCommandContents.length > 0) {
                message += "Command = " + clientCommandContents + "\n";
            }
            else {
                message += "Command = -\n";
            }

            message += "------------------------------\n";

            var clientStateContents = GetState(tableName);
            if (clientStateContents && clientStateContents.length > 0) {
                message += clientStateContents + "\n";
            }
            else {
                message += "No fields\n";
            }

            alert(message);
            break;

        case "Search":
            Navigate(puxFile);
            break;

        case "New":
        case "Edit":
        case "Delete":
            // Do nothing on the client, but process on the server
            break;

        case "Update":
            if (isSearching) {
                Navigate(puxFile);
            }
            break;

        case "Cancel":
            CopyState(dxBackupClientState, dxClientState);
            if (isSearching) {
                Navigate(puxFile);
            }
            break;

        case "More":
        case "ExportToPdf":
        case "ExportToXlsx":
            event.processOnServer = false;
            break;

        default:
        	event.processOnServer = true;
            break;
    }
}

// --------------------------------------------------------------------------------------------------
// Popup menu functions
// --------------------------------------------------------------------------------------------------

function dxOpenMenuPanel_Init(sender, event) {
	var dxOpenMenuPanel = sender;

	if (dxOpenMenuPanel) {
		var tableName = dxOpenMenuPanel.cpTableName;

		if (tableName) {
			openMenuPanels[tableName] = sender;
		}
	}
}

function dxOpenMenuPanel_EndCallback(sender, event) {
	var dxOpenMenuPanel = sender;

	if (dxOpenMenuPanel) {
		var tableName = dxOpenMenuPanel.cpTableName;
		var script = dxOpenMenuPanel.cpScript;

		if (tableName) {
			var dxLoadingPanel = loadingPanels[tableName];

			if (script) {
				eval(script);
			}
			else {
				var dxPopupMenu = popupMenus[tableName];
                var mouseCoordinates = dxOpenMenuPanel.cpMouseCoordinates;
                if (mouseCoordinates) {
                    var tokens = mouseCoordinates.split(LIST_SEPARATOR);
                    var xPos = parseInt(tokens[0]);
                    var yPos = parseInt(tokens[1]);

                    if (dxPopupMenu) {
                        dxPopupMenu.Grid = dxOpenMenuPanel.Grid;
                        dxPopupMenu.ShowAtPos(xPos, yPos);
                    }
                }
			}

			if (dxLoadingPanel) {
				dxLoadingPanel.Hide();
			}
		}
	}
}

function dxPopupMenu_Init(sender, event) {
    popupMenus[sender.cpTableName] = sender;
}

function dxPopupMenu_ItemClick(sender, event) {
	var dxPopupMenu = sender;
	var menuItem = event.item;

    if (dxPopupMenu && menuItem) {
        var command = menuItem.name;
		var tableName = dxPopupMenu.cpTableName;
		var dxClickMenuPanel = clickMenuPanels[tableName];
        var dxLoadingPanel = loadingPanels[tableName];
        var dxTable = tables[tableName];
        var isCrudMacro = true;

        if (dxTable) {
            dxTable.Command = command;
            var rowIndex = dxTable.GetFocusedRowIndex();

            switch (command) {
                case "New":
                    dxTable.AddNewRow();
                    break;

                case "Edit":
                    dxTable.StartEditRow(rowIndex);
                    break;

                case "Delete":
                    dxTable.DeleteRow(rowIndex);
                    break;

                default:
                    isCrudMacro = false;
                    if (dxClickMenuPanel) {
                        dxClickMenuPanel.Grid = dxPopupMenu.Grid;
                        dxClickMenuPanel.PerformCallback(command);
                    }

                    if (dxLoadingPanel) {
                        dxLoadingPanel.Show();
                    }
                    break;
            }

            if (isCrudMacro) {
                ClickToolbar(dxTable, event, false);
            }
        }
	}
}

function dxClickMenuPanel_Init(sender, event) {
    clickMenuPanels[sender.cpTableName] = sender;
}

function dxClickMenuPanel_EndCallback(sender, event) {
	var dxClickMenuPanel = sender;

	if (dxClickMenuPanel) {
		var tableName = dxClickMenuPanel.cpTableName;

		if (tableName) {
			var dxLoadingPanel = loadingPanels[tableName];

			var script = dxClickMenuPanel.cpScript;
			dxClickMenuPanel.cpScript = null;

			if (dxLoadingPanel) {
				dxLoadingPanel.Hide();
			}

			if (script) {
				eval(script);
			}
			else {
				var dxGrid = dxClickMenuPanel.Grid;
				if (dxGrid) {
					dxGrid.Refresh();
				}
			}
		}
    }
}

function dxLoadingPanel_Init(sender, event) {
	var dxLoadingPanel = sender;

	if (dxLoadingPanel) {
		var tableName = dxLoadingPanel.cpTableName;

		if (tableName) {
			loadingPanels[tableName] = dxLoadingPanel;
		}
	}
}