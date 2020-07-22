<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiCurrencyField.ascx.cs" Inherits="CodeClay.UiCurrencyField" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
    
<dx:ASPxCallbackPanel ID="dxCurrencyPanel" ClientInstanceName="dxCurrencyPanel" runat="server" OnCallback="dxCurrencyPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxCurrencyPanel_Init" />
    <ClientSideEvents EndCallback="dxCurrencyPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxTextbox ID="dxCurrencyBox" ClientInstanceName="dxCurrencyBox" runat="server" Theme="Aqua">
                <ValidationSettings ErrorDisplayMode="ImageWithTooltip" />
            </dx:ASPxTextbox>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>