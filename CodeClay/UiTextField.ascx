﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiTextField.ascx.cs" Inherits="CodeClay.UiTextField" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
    
<dx:ASPxCallbackPanel ID="dxTextPanel" ClientInstanceName="dxTextPanel" runat="server" OnCallback="dxTextPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxTextPanel_Init" />
    <ClientSideEvents EndCallback="dxTextPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxTextbox ID="dxTextBox" ClientInstanceName="dxTextBox" runat="server" Theme="">
                <ValidationSettings ErrorDisplayMode="ImageWithTooltip" />
            </dx:ASPxTextbox>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>