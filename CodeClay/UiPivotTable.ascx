<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiPivotTable.ascx.cs" Inherits="CodeClay.UiPivotTable" %>
<%@ Register assembly="DevExpress.Web.ASPxPivotGrid.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web.ASPxPivotGrid" tagprefix="dx" %>

<dx:ASPxPivotGrid ID="dxPivotGrid" runat="server" ClientIDMode="AutoID" DataSourceID="MyTableData" EnableTheming="True" OptionsData-DataProcessingEngine="Optimized" Theme="" OnHtmlFieldValuePrepared="dxPivotGrid_HtmlFieldValuePrepared">
    <OptionsPager RowsPerPage="100" ColumnsPerPage="100" />
    <OptionsView ShowColumnGrandTotalHeader="False" ShowColumnGrandTotals="False" ShowDataHeaders="False"
        ShowFilterHeaders="False" ShowRowGrandTotalHeader="False" ShowRowGrandTotals="False"
        ColumnTotalsLocation="Near" />
    <Groups>
        <dx:PivotGridWebGroup />
        <dx:PivotGridWebGroup />
    </Groups>
</dx:ASPxPivotGrid>

<asp:ObjectDataSource ID="MyTableData" runat="server" TypeName="CodeClay.CiDataSource" SelectMethod="SelectTable" OnSelecting="MyTableData_Selecting">
    <SelectParameters>
        <asp:Parameter Name="Table" Type="Object" />
        <asp:Parameter Name="View" Type="String" />
        <asp:Parameter Name="Parameters" Type="Object" />
        <asp:Parameter Name="Script" Type="String" Direction="Output" />
    </SelectParameters>
</asp:ObjectDataSource>
            