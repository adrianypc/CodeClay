<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiMacro.ascx.cs" Inherits="CodeClay.UiMacro" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<script>
function dxMacroLabel_Init(sender, event) {
    var script = sender.cpScript;

    if (script) {
        eval(script);
    }
}
</script>

<dx:ASPxLabel ID="dxMacroLabel" ClientInstanceName="dxMacroLabel" runat="server" Theme="" Text="Run macro">
    <ClientSideEvents Init="dxMacroLabel_Init" />
</dx:ASPxLabel>