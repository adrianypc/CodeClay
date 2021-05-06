<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerticalGrid.aspx.cs" Inherits="CodeClay.VerticalGrid" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            Hello World&nbsp;
            <br />
            <dx:ASPxVerticalGrid ID="ASPxVerticalGrid1" runat="server" AutoGenerateRows="False" DataSourceID="SQLVertical" Width="700px">
                <Rows>
                    <dx:VerticalGridTextRow FieldName="OrderType" ReadOnly="True" VisibleIndex="0">
                    </dx:VerticalGridTextRow>
                    <dx:VerticalGridTextRow FieldName="OrderNo" ReadOnly="True" VisibleIndex="1">
                    </dx:VerticalGridTextRow>
                    <dx:VerticalGridDateRow FieldName="OrderDate" ReadOnly="True" VisibleIndex="2">
                    </dx:VerticalGridDateRow>
                    <dx:VerticalGridTextRow FieldName="CourierTracking" ReadOnly="True" VisibleIndex="3">
                    </dx:VerticalGridTextRow>
                    <dx:VerticalGridDateRow FieldName="CompletionDate" ReadOnly="True" VisibleIndex="4">
                    </dx:VerticalGridDateRow>
                    <dx:VerticalGridTextRow FieldName="Name" ReadOnly="True" VisibleIndex="5">
                    </dx:VerticalGridTextRow>
                    <dx:VerticalGridTextRow FieldName="Comments" ReadOnly="True" VisibleIndex="6">
                    </dx:VerticalGridTextRow>
                    <dx:VerticalGridTextRow FieldName="ProductId" ReadOnly="True" VisibleIndex="8">
                    </dx:VerticalGridTextRow>
                    <dx:VerticalGridTextRow FieldName="Quantity" ReadOnly="True" VisibleIndex="9">
                    </dx:VerticalGridTextRow>
                    <dx:VerticalGridTextRow FieldName="ProductName" VisibleIndex="10">
                    </dx:VerticalGridTextRow>
                    <dx:VerticalGridTextRow FieldName="Inventory_QOH" VisibleIndex="11">
                    </dx:VerticalGridTextRow>
                    <dx:VerticalGridHyperLinkRow FieldName="Link" ReadOnly="True" VisibleIndex="7">
                    </dx:VerticalGridHyperLinkRow>
                </Rows>
<SettingsPager Mode="ShowPager"></SettingsPager>

<SettingsPopup>
<FilterControl AutoUpdatePosition="False"></FilterControl>
</SettingsPopup>
            </dx:ASPxVerticalGrid>
            <asp:SqlDataSource ID="SQLVertical" runat="server" ConnectionString="<%$ ConnectionStrings:HealthTreeConnectionString %>" OnSelecting="SQLVertical_Selecting" SelectCommand="spPipelineRpt_sel" SelectCommandType="StoredProcedure">
                <SelectParameters>
                    <asp:Parameter DefaultValue="localhost:60354" Name="HostSite" Type="String" />
                    <asp:Parameter DefaultValue="HT-SG" Name="BranchCode" Type="String" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
    </form>
</body>
</html>
