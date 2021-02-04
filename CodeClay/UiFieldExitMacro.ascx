<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiFieldExitMacro.ascx.cs" Inherits="CodeClay.UiFieldExitMacro" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<script>
	function dxTestField_Init(sender, event) {
		var dxTestField = sender;

		RegisterEditor(dxTestField,
			function () { return ""; },
			function (text) { alert(text); }
		);
	}

	function dxTestField_TextChanged(sender, event) {
		RunExitMacro(sender);
	}
</script>

<dx:ASPxTextBox ID="dxTestField" ClientInstanceName="dxTestField" runat="server" Theme="" AutoPostBack="false" ClientIDMode="Static">
	<ClientSideEvents Init="dxTestField_Init" />
	<ClientSideEvents TextChanged="dxTestField_TextChanged" />
</dx:ASPxTextBox>

<dx:ASPxCallbackPanel ID="dxFieldExitPanel" ClientInstanceName="dxFieldExitPanel" runat="server" OnCallback="dxFieldExitPanel_Callback">
    <ClientSideEvents Init="dxFieldExitPanel_Init" />
    <ClientSideEvents EndCallback="dxFieldExitPanel_EndCallback" />
</dx:ASPxCallbackPanel>