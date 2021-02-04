<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiLinkField.ascx.cs" Inherits="CodeClay.UiLinkField" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<style type="text/css">
    a.dxeHyperlink_Aqua:visited {
        color: #428bca;
    }
</style>

<dx:ASPxCallbackPanel ID="dxLinkPanel" ClientInstanceName="dxLinkPanel" runat="server" OnCallback="dxLinkPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxLinkPanel_Init" />
    <ClientSideEvents EndCallback="dxLinkPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <table style="width:100%">
                <tr>
                    <td>
                        <dx:ASPxHyperLink ID="dxLink" ClientInstanceName="dxLink" runat="server" Theme="" Wrap="False" Target="_blank" BackColor="Transparent"/>
                        <dx:ASPxHyperLink ID="dxEditLink" ClientInstanceName="dxEditLink" runat="server" Theme="" Wrap="False" Target="_blank" BackColor="Transparent">
                            <ClientSideEvents Init="dxEditLink_Init" />
                        </dx:ASPxHyperLink>
                        <dx:ASPxUploadControl ID="dxUpload" ClientInstanceName="dxUpload" runat="server" Theme="" Width="100%" ShowUploadButton="true" OnFileUploadComplete="dxUpload_FileUploadComplete">
                            <ClientSideEvents Init="dxUpload_Init" />
                            <ClientSideEvents FileUploadComplete="dxUpload_FileUploadComplete" />
                        </dx:ASPxUploadControl>
                        <dx:ASPxTextBox ID="dxUpdateText" ClientInstanceName="dxUpdateText" runat="server" Theme="" />
                    </td>
                    <asp:Panel ID="deleteButtonPanel" runat="server">
                        <td style="width:5px" />
                        <td style="width:15px">
                            <dx:ASPxButton ID="dxDelete" ClientInstanceName="dxDelete" runat="server" Theme="" Image-IconID="edit_delete_16x16" AutoPostBack="false" RenderMode="Link">
                                <ClientSideEvents Init="dxDelete_Init" />
                                <ClientSideEvents Click="dxDelete_Click" />
                            </dx:ASPxButton>
                        </td>
                    </asp:Panel>
                </tr>
            </table>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>