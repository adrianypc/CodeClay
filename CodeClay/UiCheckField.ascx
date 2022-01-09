<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiCheckField.ascx.cs" Inherits="CodeClay.UiCheckField" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxCallbackPanel ID="dxCheckPanel" ClientInstanceName="dxCheckPanel" runat="server" OnCallback="dxCheckPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxCheckPanel_Init" />
    <ClientSideEvents EndCallback="dxCheckPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <table>
                <tr>
                    <td>
                        <dx:ASPxCheckBox ID="dxCheckBox" ClientInstanceName="dxCheckBox" runat="server" Theme="" />
                    </td>
                </tr>
            </table>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>