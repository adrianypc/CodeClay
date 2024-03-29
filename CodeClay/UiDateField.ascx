﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiDateField.ascx.cs" Inherits="CodeClay.UiDateField" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxCallbackPanel ID="dxDatePanel" ClientInstanceName="dxDatePanel" runat="server" OnCallback="dxDatePanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxDatePanel_Init" />
    <ClientSideEvents EndCallback="dxDatePanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxDateEdit ID="dxDateBox" ClientInstanceName="dxDateBox" runat="server" Theme="" />
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>