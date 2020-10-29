using System.Data;

using CodistriCore;

namespace CodeClay
{
    public class CiDataSource
    {
        // --------------------------------------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------------------------------------

        public DataTable SelectDropdown(object dropdown)
        {
            UiComboField uiComboField = dropdown as UiComboField;
            if (uiComboField != null)
            {
                return SelectComboDropdown(uiComboField);
            }

            UiRadioField uiRadioField = dropdown as UiRadioField;
            if (uiRadioField != null)
            {
                return SelectRadioDropdown(uiRadioField);
            }

            return null;
        }

        public DataTable SelectTable(object table, string view, object parameters, ref string script)
        {
            DataRow drParams = parameters as DataRow;
            DataTable dt = null;

            CiTable ciTable = table as CiTable;
            if (ciTable != null)
            {
                if (parameters != null && parameters.GetType() == typeof(bool))
                {
                    // Do nothing - but can't remember what this is for???
                    throw new System.Exception("Aha!");
                }
                else if (ciTable.SelectMacro != null)
                {
                    CiMacro ciMacro = ciTable.SearchMacro;
                    if (ciMacro != null)
                    {
                        ciMacro.Run(drParams, true);
                        dt = ciMacro.ResultTable;
                        script = ciMacro.ResultScript;
                    }
                }
                else if (ciTable.DataSource != null)
                {
                    dt = ciTable.DataTable;
                }

                if (dt == null)
                {
                    dt = new DataTable();
                }

                AddExpressionColumns(ciTable, dt, drParams);

                if (MyWebUtils.GetNumberOfColumns(dt) > 0 && !dt.Columns.Contains("RowKey"))
                {
                    dt.Columns.Add("RowKey").Expression = CreateRowKeyExpression(ciTable.RowKey);
                }
            }

            return dt;
        }

        public void UpdateTable(object table, string view, object parameters, string rowKey, ref string script, ref bool isInvalid)
        {
            DataRow drParams = parameters as DataRow;

            CiTable ciTable = table as CiTable;
            if (ciTable != null && drParams != null)
            {
                CiMacro ciMacro = ciTable.UpdateMacro;
                if (ciMacro != null)
                {
                    ciMacro.Run(drParams);
                    script = ciMacro.ResultScript;
                    isInvalid = !MyUtils.IsEmpty(ciMacro.ErrorMessage);
                }
            }
        }

        public void InsertTable(object table, string view, object parameters, ref string rowKey, ref string script, ref bool isInvalid)
        {
            DataRow drParams = parameters as DataRow;

            CiTable ciTable = table as CiTable;
            if (ciTable != null)
            {
                CiMacro ciMacro = ciTable.InsertMacro;
                if (ciMacro != null)
                {
                    ciMacro.Run(drParams);
                    script = ciMacro.ResultScript;
                    isInvalid = !MyUtils.IsEmpty(ciMacro.ErrorMessage);

                    DataTable dt = ciMacro.ResultTable;
                    if (MyWebUtils.GetNumberOfRows(dt) > 0)
                    {
                        DataRow dr = dt.Rows[0];
                        int i = 0;
                        foreach (string key in ciTable.RowKeyNames)
                        {
                            if (i++ > 0)
                            {
                                rowKey += ",";
                            }

                            if (dt.Columns.Contains(key))
                            {
                                rowKey += MyUtils.Coalesce(dr[key], "").ToString();
                            }
                        }
                    }
                }
            }
        }

        public void DeleteTable(object table, string view, object parameters, string rowKey, ref string script, ref bool isInvalid)
        {
            DataRow drParams = parameters as DataRow;

            CiTable ciTable = table as CiTable;
            if (ciTable != null)
            {
                CiMacro ciMacro = ciTable.DeleteMacro;
                if (ciMacro != null)
                {
                    ciMacro.Run(drParams);
                    script = ciMacro.ResultScript;
                    isInvalid = !MyUtils.IsEmpty(ciMacro.ErrorMessage);
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private DataTable SelectComboDropdown(UiComboField uiComboField)
        {
            if (uiComboField != null)
            {
                CiComboField ciComboField = uiComboField.CiField as CiComboField;
                if (ciComboField != null)
                {
                    string sql = ciComboField.DropdownSQL;
                    if (!MyUtils.IsEmpty(sql))
                    {
                        DataRow drParams = uiComboField.GetState();
                        DataTable dt = MyWebUtils.GetBySQL(sql, drParams);

                        if (dt != null)
                        {
                            // Add a blank row, otherwise exception is sometimes thrown in grid view
                            dt.Rows.InsertAt(dt.NewRow(), 0);
                        }

                        return dt;
                    }
                    else
                    {
                        return MyWebUtils.ToDataTable(ciComboField.DataSource.OuterXml);
                    }
                }
            }

            return null;
        }

        private DataTable SelectRadioDropdown(UiRadioField uiRadioField)
        {
            if (uiRadioField != null)
            {
                CiRadioField ciRadioField = uiRadioField.CiField as CiRadioField;
                if (ciRadioField != null)
                {
                    string sql = ciRadioField.DropdownSQL;
                    if (!MyUtils.IsEmpty(sql))
                    {
                        DataRow drParams = uiRadioField.GetState();
                        DataTable dt = MyWebUtils.GetBySQL(sql, drParams);

                        return dt;
                    }
                    else
                    {
                        return MyWebUtils.ToDataTable(ciRadioField.DataSource.OuterXml);
                    }
                }
            }

            return null;
        }

        private string CreateRowKeyExpression(string keyName)
        {
            string rowKeyExpression = "";

            if (!MyUtils.IsEmpty(keyName))
            {
                string[] keyFields = keyName.Split(',');

                int i = 0;
                foreach (string keyField in keyFields)
                {
                    if (i++ > 0)
                    {
                        rowKeyExpression += string.Format(" + '{0}' + ", "\t");
                    }

                    rowKeyExpression += string.Format("isnull({0}, '')", keyField.Trim());
                }
            }

            return rowKeyExpression;
        }

        private void AddExpressionColumns(CiTable ciTable, DataTable dt, DataRow drParams)
        {
            if (ciTable != null)
            {
                foreach (CiField ciField in ciTable.CiFields)
                {
                    string fieldName = ciField.FieldName;
                    string defaultValue = ciField.Value;

                    if (!dt.Columns.Contains(fieldName))
                    {
                        DataColumn dc = dt.Columns.Add(fieldName);
                        string expression = "";

                        if (ciField.Computed)
                        {
                            expression = defaultValue;
                        }
                        else
                        {
                            if (MyUtils.IsEmpty(defaultValue) && drParams != null && drParams.Table.Columns.Contains(fieldName))
                            {
                                defaultValue = drParams[fieldName].ToString().Replace("'", "''");
                            }

                            expression = string.Format("'{0}'", defaultValue);
                        }

                        dc.Expression = expression;
                    }
                    else if (ciField.GetType() == typeof(CiField) && !MyUtils.IsEmpty(defaultValue))
                    {
                        dt.Columns[fieldName].Expression = string.Format("'{0}'", defaultValue);
                    }
                }
            }
        }
    }
}