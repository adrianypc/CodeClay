var LIST_SEPARATOR = "|";

function CopyState(dxHiddenField1, dxHiddenField2) {
    if (dxHiddenField1 && dxHiddenField2) {
        dxHiddenField2.Clear();

        for (var key in dxHiddenField1["properties"]) {
            var name = key.replace(ASPxClientHiddenField.TopLevelKeyPrefix, "");
            var value = dxHiddenField1["properties"][key];

            dxHiddenField2.Set(name, value);
        }
    }
}

function GetState(tableName) {
    if (tableName && dxClientState) {
        var contents = "";
        var tableNamePrefix = tableName + ".";

        var i = 0;
        for (var key in dxClientState["properties"]) {
            if (key) {
                var name = key.replace(ASPxClientHiddenField.TopLevelKeyPrefix, "");

                if (name && name.startsWith(tableNamePrefix)) {
                    var value = dxClientState["properties"][key];

                    if (i++ > 0) {
                        contents += "\n";
                    }

                    contents += name.substring(tableNamePrefix.length, name.length) + " = " + value;
                }
            }
        }
    }

    return contents;
}

function ClearState(tableName) {
    if (tableName && dxClientState) {
        var tableNamePrefix = tableName + ".";

        for (var key in dxClientState["properties"]) {
            var name = key.replace(ASPxClientHiddenField.TopLevelKeyPrefix, "");

            if (name.startsWith(tableNamePrefix) && !name.endsWith(".RowIndex")) {
                dxClientState.Remove(name);
            }
        }
    }
}

function SetCommand(tableName, commandName) {
    if (tableName && dxClientCommand) {
        dxClientCommand.Set(tableName, commandName);
    }
}