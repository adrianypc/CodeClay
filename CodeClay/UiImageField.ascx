<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiImageField.ascx.cs" Inherits="CodeClay.UiImageField" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<style type="text/css">
    a.dxeHyperlink_Aqua:visited {
        color: #428bca;
    }
</style>

<dx:ASPxCallbackPanel ID="dxImagePanel" ClientInstanceName="dxImagePanel" runat="server" OnCallback="dxImagePanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxImagePanel_Init" />
    <ClientSideEvents EndCallback="dxImagePanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <table style="width:100%">
                <tr>
                    <td>
                        <dx:ASPxImage ID="dxImage" ClientInstanceName="dxImage" runat="server" Theme="Aqua" BackColor="Transparent" />
                        <dx:ASPxImage ID="dxEditImage" ClientInstanceName="dxEditImage" runat="server" Theme="Aqua" BackColor="Transparent">
                            <ClientSideEvents Init="dxEditImage_Init" />
                        </dx:ASPxImage>
                        <dx:ASPxUploadControl ID="dxUploadImage" ClientInstanceName="dxUploadImage" runat="server" Theme="Aqua" Width="100%" ShowUploadButton="true" OnFileUploadComplete="dxUploadImage_FileUploadComplete">
                            <ClientSideEvents Init="dxUploadImage_Init" />
                            <ClientSideEvents FileUploadComplete="dxUploadImage_FileUploadComplete" />
                        </dx:ASPxUploadControl>
                    </td>
                    <asp:Panel ID="deleteButtonPanel" runat="server">
                        <td style="width:5px" />
                        <td style="width:15px">
                            <dx:ASPxButton ID="dxDeleteImage" ClientInstanceName="dxDeleteImage" runat="server" Theme="Aqua" Image-IconID="edit_delete_16x16" AutoPostBack="false" RenderMode="Link">
                                <ClientSideEvents Init="dxDeleteImage_Init" />
                                <ClientSideEvents Click="dxDeleteImage_Click" />
                            </dx:ASPxButton>
                        </td>
                    </asp:Panel>
                </tr>
            </table>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>