<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiApplication.ascx.cs" Inherits="CodeClay.UiApplication" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register Src="UiMenu.ascx" TagPrefix="uc" TagName="UiMenu" %>

<dx:ASPxPanel ID="dxMenuPanel" ClientInstanceName="dxMenuPanel" runat="server">
    <PanelCollection>
        <dx:PanelContent>
            <uc:UiMenu ID="uiMenu" runat="server" />
            <br />
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxPanel>
<dx:ASPxHiddenField ID="dxClientState" ClientInstanceName="dxClientState" runat="server" SyncWithServer="true" />
<dx:ASPxHiddenField ID="dxBackupClientState" ClientInstanceName="dxBackupClientState" runat="server" SyncWithServer="true" />
<dx:ASPxHiddenField ID="dxClientCommand" ClientInstanceName="dxClientCommand" runat="server" SyncWithServer="true" />
