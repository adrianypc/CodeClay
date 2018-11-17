﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiLinkField.ascx.cs" Inherits="CodeClay.UiLinkField" %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<style type="text/css">
    a.dxeHyperlink_Aqua:visited {
        color: #428bca;
    }
</style>

<dx:ASPxCallbackPanel ID="dxLinkPanel" ClientInstanceName="dxLinkPanel" runat="server" OnCallback="dxLinkPanel_Callback">
    <ClientSideEvents Init="dxLinkPanel_Init" />
    <ClientSideEvents EndCallback="dxLinkPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <table>
                <tr>
                    <td style="width:200px">
                        <dx:ASPxHyperLink ID="dxLink" ClientInstanceName="dxLink" runat="server" Theme="Aqua" Wrap="False" Target="_blank" BackColor="Transparent"/>
                        <dx:ASPxHyperLink ID="dxEditLink" ClientInstanceName="dxEditLink" runat="server" Theme="Aqua" Wrap="False" Target="_blank" BackColor="Transparent">
                            <ClientSideEvents Init="dxEditLink_Init" />
                        </dx:ASPxHyperLink>
                        <dx:ASPxUploadControl ID="dxUpload" ClientInstanceName="dxUpload" runat="server" Theme="Aqua" ShowUploadButton="true" OnFileUploadComplete="dxUpload_FileUploadComplete">
                            <ClientSideEvents Init="dxUpload_Init" />
                            <ClientSideEvents FileUploadComplete="dxUpload_FileUploadComplete" />
                        </dx:ASPxUploadControl>
                        <dx:ASPxTextBox ID="dxUpdateText" ClientInstanceName="dxUpdateText" runat="server" Theme="Aqua">
                            <ClientSideEvents Init="dxUpdateText_Init" />
                            <ClientSideEvents LostFocus="dxUpdateText_LostFocus" />
                        </dx:ASPxTextBox>
                    </td>
                    <td style="width:5px" />
                    <td style="width:15px">
                        <dx:ASPxButton ID="dxDelete" ClientInstanceName="dxDelete" runat="server" Theme="Aqua" Image-IconID="edit_delete_16x16" AutoPostBack="false" RenderMode="Link">
                            <ClientSideEvents Init="dxDelete_Init" />
                            <ClientSideEvents Click="dxDelete_Click" />
                        </dx:ASPxButton>
                    </td>
                </tr>
            </table>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>