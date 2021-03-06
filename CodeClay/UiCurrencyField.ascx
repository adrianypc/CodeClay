﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiCurrencyField.ascx.cs" Inherits="CodeClay.UiCurrencyField" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
    
<dx:ASPxCallbackPanel ID="dxCurrencyPanel" ClientInstanceName="dxCurrencyPanel" runat="server" OnCallback="dxCurrencyPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxCurrencyPanel_Init" />
    <ClientSideEvents EndCallback="dxCurrencyPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxTextbox ID="dxCurrencyBox" ClientInstanceName="dxCurrencyBox" runat="server" Theme="">
                <ValidationSettings ErrorDisplayMode="ImageWithTooltip" />
            </dx:ASPxTextbox>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>