﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiField.ascx.cs" Inherits="CodeClay.UiField" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxCallbackPanel ID="dxLabelPanel" ClientInstanceName="dxLabelPanel" runat="server" OnCallback="dxLabelPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxLabelPanel_Init" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxLabel ID="dxLabel" ClientInstanceName="dxLabel" runat="server" Theme="" EncodeHtml="false" />
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>