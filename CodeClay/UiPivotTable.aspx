<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UiPivotTable.aspx.cs" Inherits="CodeClay.UiPivotGrid" %>
<%@ Register assembly="DevExpress.Web.ASPxPivotGrid.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web.ASPxPivotGrid" tagprefix="dx" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="frmPipeline" runat="server">
        <asp:SqlDataSource ID="htSQLDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:HealthTreeConnectionString %>" SelectCommand="spPipelineRpt_sel" SelectCommandType="StoredProcedure">
            <SelectParameters>
                <asp:Parameter DefaultValue="localhost" Name="HostSite" Type="String" />
                <asp:Parameter DefaultValue="HT-SG" Name="BranchCode" Type="String" />
            </SelectParameters>
        </asp:SqlDataSource>
        <div>
            <br />
            <dx:ASPxPivotGrid ID="dxPivotGrid" runat="server" ClientIDMode="AutoID" DataSourceID="htSQLDataSource" EnableTheming="True" OptionsData-DataProcessingEngine="Optimized" Theme="Aqua" OnHtmlFieldValuePrepared="dxPivotGrid_HtmlFieldValuePrepared">
                <OptionsPager RowsPerPage="100" ColumnsPerPage="100" />
                <Fields>
                    <dx:PivotGridField ID="field" Area="ColumnArea" AreaIndex="0" Caption="Type" Name="field">
                        <DataBindingSerializable>
                            <dx:DataSourceColumnBinding ColumnName="OrderType" />
                        </DataBindingSerializable>
                    </dx:PivotGridField>
                    <dx:PivotGridField ID="field1" Area="ColumnArea" AreaIndex="1" Caption="Order #" Name="field1">
                        <DataBindingSerializable>
                            <dx:DataSourceColumnBinding ColumnName="OrderNo" />
                        </DataBindingSerializable>
                    </dx:PivotGridField>
                    <dx:PivotGridField ID="field2" Area="ColumnArea" AreaIndex="2" Caption="Date" Name="field2">
                        <ValueFormat FormatString="d" FormatType="DateTime" />
                        <DataBindingSerializable>
                            <dx:DataSourceColumnBinding ColumnName="OrderDate" />
                        </DataBindingSerializable>
                    </dx:PivotGridField>
                    <dx:PivotGridField ID="field3" Area="ColumnArea" AreaIndex="3" Caption="Tracking #" Name="field3">
                        <DataBindingSerializable>
                            <dx:DataSourceColumnBinding ColumnName="CourierTracking" />
                        </DataBindingSerializable>
                    </dx:PivotGridField>
                    <dx:PivotGridField ID="field4" Area="ColumnArea" AreaIndex="4" Caption="Completion Date" Name="field4">
                        <ValueFormat FormatString="d" FormatType="DateTime" />
                        <DataBindingSerializable>
                            <dx:DataSourceColumnBinding ColumnName="CompletionDate" />
                        </DataBindingSerializable>
                    </dx:PivotGridField>
                    <dx:PivotGridField ID="field5" Area="ColumnArea" AreaIndex="5" Name="field5">
                        <DataBindingSerializable>
                            <dx:DataSourceColumnBinding ColumnName="Comments" />
                        </DataBindingSerializable>
                    </dx:PivotGridField>
                    <dx:PivotGridField ID="field6" Area="ColumnArea" AreaIndex="6" Name="field6">
                        <ValueStyle Wrap="True">
                        </ValueStyle>
                        <DataBindingSerializable>
                            <dx:DataSourceColumnBinding ColumnName="Link" />
                        </DataBindingSerializable>
                    </dx:PivotGridField>
                    <dx:PivotGridField ID="field7" Area="ColumnArea" AreaIndex="7" Caption="Name" Name="field7">
                        <DataBindingSerializable>
                            <dx:DataSourceColumnBinding ColumnName="Name" />
                        </DataBindingSerializable>
                    </dx:PivotGridField>
                    <dx:PivotGridField ID="field8" Area="DataArea" AreaIndex="0" Name="field8" Visible="False">
                        <DataBindingSerializable>
                            <dx:DataSourceColumnBinding ColumnName="ProductId" />
                        </DataBindingSerializable>
                    </dx:PivotGridField>
                    <dx:PivotGridField ID="field9" Area="DataArea" AreaIndex="0" Name="field9">
                        <DataBindingSerializable>
                            <dx:DataSourceColumnBinding ColumnName="Quantity" />
                        </DataBindingSerializable>
                    </dx:PivotGridField>
                    <dx:PivotGridField ID="field10" Area="RowArea" AreaIndex="0" Caption="Product" GroupIndex="1" InnerGroupIndex="0" Name="field10">
                        <DataBindingSerializable>
                            <dx:DataSourceColumnBinding ColumnName="ProductName" />
                        </DataBindingSerializable>
                    </dx:PivotGridField>
                </Fields>
                <OptionsView ShowColumnGrandTotalHeader="False" ShowColumnGrandTotals="False" ShowDataHeaders="False"
                    ShowFilterHeaders="False" ShowRowGrandTotalHeader="False" ShowRowGrandTotals="False"
                    ColumnTotalsLocation="Near" />
                <OptionsData DataProcessingEngine="Optimized"></OptionsData>
                <Groups>
                    <dx:PivotGridWebGroup />
                    <dx:PivotGridWebGroup />
                </Groups>
            </dx:ASPxPivotGrid>
        </div>
    </form>
</body>
</html>
