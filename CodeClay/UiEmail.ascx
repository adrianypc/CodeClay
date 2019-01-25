<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiEmail.ascx.cs" Inherits="CodeClay.UiEmail" %>
<%@ Register Src="UiTable.ascx" TagPrefix="uc" TagName="UiTable" %>

<uc:UiTable ID="uiTable" runat="server">
    <CiTable>
        <TableCaption>Emails sent</TableCaption>
        <CiTextField>
            <FieldName>EmailAddress</FieldName>
        </CiTextField>
        <CiTextField>
            <FieldName>EmailStatus</FieldName>
        </CiTextField>
    </CiTable>
</uc:UiTable>