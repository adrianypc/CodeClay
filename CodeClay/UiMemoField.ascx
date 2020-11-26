<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiMemoField.ascx.cs" Inherits="CodeClay.UiMemoField" %>
<%@ Register Assembly="DevExpress.Web.ASPxHtmlEditor.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxHtmlEditor" TagPrefix="dxh" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxCallbackPanel ID="dxMemoPanel" ClientInstanceName="dxMemoPanel" runat="server" OnCallback="dxMemoPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxMemoPanel_Init" />
    <ClientSideEvents EndCallback="dxMemoPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxMemo ID="dxMemo" ClientInstanceName="dxMemo" runat="server" Theme="Aqua" BackColor="PaleGoldenrod" height="40px" MaxLength="4000">
                <ValidationSettings ErrorDisplayMode="ImageWithTooltip" />
            </dx:ASPxMemo>
            <dxh:ASPxHtmlEditor ID="dxHtmlMemo" ClientInstanceName="dxHtmlMemo" runat="server" Theme="Aqua">
                <ClientSideEvents Init="dxHtmlMemo_Init" />
                <ClientSideEvents HtmlChanged="dxHtmlMemo_HtmlChanged" />
            </dxh:ASPxHtmlEditor>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>