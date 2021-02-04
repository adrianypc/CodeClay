<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiTimeField.ascx.cs" Inherits="CodeClay.UiTimeField" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxCallbackPanel ID="dxTimePanel" ClientInstanceName="dxTimePanel" runat="server" OnCallback="dxTimePanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxTimePanel_Init" />
    <ClientSideEvents EndCallback="dxTimePanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxTimeEdit ID="dxTimeBox" ClientInstanceName="dxTimeBox" runat="server" Theme="" />
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>