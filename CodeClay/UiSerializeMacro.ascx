<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiSerializeMacro.ascx.cs" Inherits="CodeClay.UiSerializeMacro" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<dx:ASPxUploadControl ID="dxImportPUX" ClientInstanceName="dxImportPUX" runat="server" Theme="" Width="100%" ShowUploadButton="true" OnFileUploadComplete="dxImportPUX_FileUploadComplete">
    <ClientSideEvents FileUploadComplete="dxImportPUX_FileUploadComplete" />
</dx:ASPxUploadControl>
