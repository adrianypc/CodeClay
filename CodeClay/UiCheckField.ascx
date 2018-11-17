<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiCheckField.ascx.cs" Inherits="CodeClay.UiCheckField" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxCallbackPanel ID="dxCheckPanel" ClientInstanceName="dxCheckPanel" runat="server" OnCallback="dxCheckPanel_Callback">
    <ClientSideEvents Init="dxCheckPanel_Init" />
    <ClientSideEvents EndCallback="dxCheckPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxCheckBox ID="dxCheckBox" ClientInstanceName="dxCheckBox" runat="server" Theme="Aqua" />
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>