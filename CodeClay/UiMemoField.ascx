<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiMemoField.ascx.cs" Inherits="CodeClay.UiMemoField" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxCallbackPanel ID="dxMemoPanel" ClientInstanceName="dxMemoPanel" runat="server" OnCallback="dxMemoPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxMemoPanel_Init" />
    <ClientSideEvents EndCallback="dxMemoPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxMemo ID="dxMemo" ClientInstanceName="dxMemo" runat="server" Theme="Aqua" BackColor="PaleGoldenrod" height="300px" MaxLength="4000">
                <ValidationSettings ErrorDisplayMode="ImageWithTooltip" />
            </dx:ASPxMemo>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>