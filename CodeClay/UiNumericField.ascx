<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiNumericField.ascx.cs" Inherits="CodeClay.UiNumericField" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxCallbackPanel ID="dxSpinEditPanel" ClientInstanceName="dxSpinEditPanel" runat="server" OnCallback="dxSpinEditPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxSpinEditPanel_Init" />
    <ClientSideEvents EndCallback="dxSpinEditPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxSpinEdit ID="dxSpinEdit" ClientInstanceName="dxSpinEdit" runat="server" Theme="">
                <ValidationSettings ErrorDisplayMode="ImageWithTooltip" />
            </dx:ASPxSpinEdit>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>