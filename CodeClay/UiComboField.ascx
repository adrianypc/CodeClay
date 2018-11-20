<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiComboField.ascx.cs" Inherits="CodeClay.UiComboField" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxCallbackPanel ID="dxComboPanel" ClientInstanceName="dxComboPanel" runat="server" OnCallback="dxComboPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxComboPanel_Init" />
    <ClientSideEvents EndCallback="dxComboPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxComboBox ID="dxComboBox" ClientInstanceName="dxComboBox" runat="server" Theme="Aqua" DropDownStyle="DropDownList" DataSourceID="MyComboData"
                TextFormatString="{0}" OnTextChanged="dxComboBox_TextChanged">
            </dx:ASPxComboBox>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>

<asp:ObjectDataSource ID="MyComboData" runat="server" TypeName="CodeClay.CiDataSource" SelectMethod="SelectDropdown"
    OnSelecting="MyComboData_Selecting" OnSelected="MyComboData_Selected">
    <SelectParameters>
        <asp:Parameter Name="Dropdown" Type="Object"/>
    </SelectParameters>
</asp:ObjectDataSource>