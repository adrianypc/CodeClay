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

        public DataTable SelectTable(object table, object parameters)
        {
            DataRow drParams = parameters as DataRow;
            DataTable dt = null;

            CiTable ciTable = table as CiTable;
            if (ciTable != null)
            {
                if (ciTable.SelectMacro != null)
                {
                    dt = RunMacro(ciTable.SelectMacro, drParams);

                    if (MyWebUtils.GetNumberOfColumns(dt) > 0 && !dt.Columns.Contains("RowKey"))
                    {
                        dt.Columns.Add("RowKey").Expression = CreateRowKeyExpression(ciTable.RowKey);
                    }
                }
                else if (ciTable.DataSource != null)
                {
                    string dataSourceXML = ciTable.DataSource.OuterXml;
                    if (!MyUtils.IsEmpty(dataSourceXML))
                    {
                        dt = MyWebUtils.ToDataTable(ciTable.DataSource.OuterXml);
                    }
                }

                foreach (CiField ciField in ciTable.CiFields)
                {
                    string fieldName = ciField.FieldName;
                    if (!dt.Columns.Contains(fieldName))
                    {
                        string defaultValue = ciField.DefaultValue;
                        string expression = "";

                        if (ciField.Computed)
                        {
							expression = defaultValue;
						}
                        else
                        {
                            if (MyUtils.IsEmpty(defaultValue) && drParams != null && drParams.Table.Columns.Contains(fieldName))
                            {
                                defaultValue = drParams[fieldName].ToString();
                            }

                            expression = string.Format("'{0}'", defaultValue);
                        }

						DataColumn dc = dt.Columns.Add(fieldName);
                        dc.Expression = expression;
                    }
                }
            }

            return dt;
        }

        public void UpdateTable(object table, object parameters, string rowKey)
        {
            DataRow drParams = parameters as DataRow;

            CiTable ciTable = table as CiTable;
            if (ciTable != null)
            {
                RunMacro(ciTable.UpdateMacro, drParams);
            }
        }

        public void InsertTable(object table, object parameters, ref string rowKey)
        {
            DataRow drParams = parameters as DataRow;

            CiTable ciTable = table as CiTable;
            if (ciTable != null)
            {
                DataTable dt = RunMacro(ciTable.InsertMacro, drParams);
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

        public void DeleteTable(object table, object parameters, string rowKey)
        {
            DataRow drParams = parameters as DataRow;

            CiTable ciTable = table as CiTable;
            if (ciTable != null)
            {
                RunMacro(ciTable.DeleteMacro, drParams);
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

        private DataTable RunMacro(CiMacro ciMacro, DataRow drParams)
        {
            if (ciMacro != null)
            {
                return ciMacro.RunSQL(drParams);
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
    }
}