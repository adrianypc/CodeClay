﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiReport.ascx.cs" Inherits="CodeClay.UiReport" %>
<%@ Register Assembly="DevExpress.XtraReports.v20.2.Web.WebForms, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.XtraReports.Web" TagPrefix="dx" %>

<dx:ASPxWebDocumentViewer ID="dxWebDocumentViewer" ClientInstanceName="dxWebDocumentViewer" runat="server" Height="600px" EnableViewState="False" />
<dx:ASPxReportDesigner ID="dxReportDesigner" ClientInstanceName="dxReportDesigner" runat="server" Height="600px" OnSaveReportLayout="dxReportDesigner_SaveReportLayout" ></dx:ASPxReportDesigner>
