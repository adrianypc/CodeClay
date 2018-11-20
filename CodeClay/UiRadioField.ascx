<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiRadioField.ascx.cs" Inherits="CodeClay.UiRadioField" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxCallbackPanel ID="dxRadioPanel" ClientInstanceName="dxRadioPanel" runat="server" OnCallback="dxRadioPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxRadioPanel_Init" />
    <ClientSideEvents EndCallback="dxRadioPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxRadioButtonList ID="dxRadioBox" ClientInstanceName="dxRadioBox" runat="server" Theme="Aqua" DataSourceID="MyRadioData" RepeatLayout="Table"
                RepeatDirection="Vertical" >
            </dx:ASPxRadioButtonList>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>

<asp:ObjectDataSource ID="MyRadioData" runat="server" TypeName="CodeClay.CiDataSource" SelectMethod="SelectDropdown"
    OnSelecting="MyRadioData_Selecting" OnSelected="MyRadioData_Selected">
    <SelectParameters>
        <asp:Parameter Name="Dropdown" Type="Object"/>
    </SelectParameters>
</asp:ObjectDataSource>
