<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiButtonField.ascx.cs" Inherits="CodeClay.UiButtonField" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxCallbackPanel ID="dxButtonPanel" ClientInstanceName="dxButtonPanel" runat="server" OnCallback="dxButtonPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxButtonPanel_Init" />
    <ClientSideEvents EndCallback="dxButtonPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxButton ID="dxButton" ClientInstanceName="dxButton" runat="server" Theme="" AutoPostBack="false">
                <ClientSideEvents Click="dxButton_Click" />
            </dx:ASPxButton>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>