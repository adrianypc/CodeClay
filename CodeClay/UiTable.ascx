<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiTable.ascx.cs" Inherits="CodeClay.UiTable" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register Src="UiFieldExitMacro.ascx" TagPrefix="uc" TagName="UiFieldExitMacro" %>

<style type="text/css">
    .dxgvTable_Aqua caption
    {
        text-align: left;
    }

    .cssLoadingPanel
    {
        z-index: 1000 !important;
    }

    .cssSmallFont *
    {
        font-size:small !important;
    }

    .cssFieldBorderStyle
    {
        border: solid;
        border-color: lightgray;
        border-width: 1px;
        padding: 4px;
    }

    .dropZoneExternal > div,
    .dropZoneExternal > img {
        position: absolute;
    }

    .dropZoneExternal {
        position: relative;
        cursor: pointer;
        width: 100%;
    }

    #uploadedImage {
        width: 100%;
    }

    .dropZoneText {
        color: #808080;
        display: table-cell;
        vertical-align: middle;
        text-align: center;
        font-size: 20pt;
        width: 1200px;
        height: 100px;
    }

    #dropZone {
        top: 0;
        width: 100%;
        height: 100%;
        padding: 100px 25px;
        border: 1px dashed #f17f21 !important;
    }

    .uploadControlDropZone,
    .hidden {
        display: none;
    }

    .dxucInlineDropZoneSys span {
        color: #fff !important;
        font-size: 10pt;
        font-weight: normal !important;
    }

    .validationMessage {
        padding: 0 20px;
        text-align: center;
    }

    .uploadContainer {
        width: 100%;
        margin-top: 10px;
    }
</style>

<uc:UiFieldExitMacro ID="uiFieldExitMacro" runat="server" />

<dx:ASPxCardView ID="dxSearch" ClientInstanceName="dxSearch" runat="server" Theme="" AutoGenerateColumns="false" Width="100%" DataSourceID="MyTableData" KeyFieldName="RowKey" CssClass="cssSmallFont"
    OnInit="dxSearch_Init" OnLoad="dxSearch_Load" OnCustomJSProperties="dxSearch_CustomJSProperties"
    OnInitNewCard="dxSearch_InitNewCard" OnCardInserting="dxSearch_CardInserting" OnCancelCardEditing="dxSearch_CancelCardEditing">
    <ClientSideEvents Init="dxSearch_Init" />
    <ClientSideEvents EndCallback="dxSearch_EndCallback" />
    <ClientSideEvents ToolbarItemClick="dxSearch_ToolbarItemClick" />
    <Settings ShowTitlePanel="true" />
    <SettingsBehavior AllowFocusedCard="true" AllowSelectByCardClick="false" ConfirmDelete="true"  />
    <SettingsEditing Mode="EditForm" />
    <SettingsLoadingPanel Mode="Disabled" />
    <SettingsPager Position="Top">
        <SettingsTableLayout ColumnCount="1" RowsPerPage="1"/>
    </SettingsPager>
    <SettingsText Title="Loading screen ..." />
    <Styles>
        <LoadingPanel CssClass="cssLoadingPanel" HorizontalAlign="Center" VerticalAlign="Middle" />
        <TitlePanel Font-Bold="true" />
    </Styles>
    <Columns>
        <dx:CardViewColumn Name="RowKey" FieldName="RowKey" Visible="false" />
    </Columns>
    <Toolbars>
        <dx:CardViewToolbar ItemAlign="Right">
            <Items>
                <dx:CardViewToolbarItem Name="More" Text="More">
                    <Items>
                        <dx:CardViewToolbarItem Name="Inspect" Text="Inspect" />
                        <dx:CardViewToolbarItem Name="Designer" Text="Designer" Command="Custom" Target="_blank" />
                    </Items>
                </dx:CardViewToolbarItem>
                <dx:CardViewToolbarItem Name="Divider" Text="" Enabled="false" ItemStyle-Width="100%" />
                <dx:CardViewToolbarItem Name="Update" Command="Update" Text="Accept" Image-IconID="actions_apply_16x16" />
                <dx:CardViewToolbarItem Name="Cancel" Command="Cancel" Image-IconID="actions_cancel_16x16" />
            </Items>
        </dx:CardViewToolbar>
    </Toolbars>
</dx:ASPxCardView>

<dx:ASPxCardView ID="dxCard" ClientInstanceName="dxCard" runat="server" Theme="" DataSourceID="MyTableData" AutoGenerateColumns="false" Width="100%" KeyFieldName="RowKey" CssClass="cssSmallFont"
    OnInit="dxCard_Init" OnLoad="dxCard_Load" OnCustomJSProperties="dxCard_CustomJSProperties" OnCustomCallback="dxCard_CustomCallback" OnCustomColumnDisplayText="dxCard_CustomColumnDisplayText"
    OnInitNewCard ="dxCard_InitNewCard" OnCardInserting="dxCard_CardInserting"
    OnCancelCardEditing="dxCard_CancelCardEditing"  OnCardUpdating="dxCard_CardUpdating"
    OnToolbarItemClick="dxCard_ToolbarItemClick" OnCardValidating="dxCard_CardValidating">
    <ClientSideEvents Init="dxCard_Init" />
    <ClientSideEvents FocusedCardChanged="dxCard_FocusedCardChanged" />
    <ClientSideEvents BeginCallback="dxCard_BeginCallback" />
    <ClientSideEvents EndCallback="dxCard_EndCallback" />
    <ClientSideEvents ToolbarItemClick="dxCard_ToolbarItemClick" />
    <Settings ShowTitlePanel="true" ShowCardFooter="true" />
    <SettingsBehavior AllowFocusedCard="true" AllowSelectByCardClick="false" ConfirmDelete="true"  />
    <SettingsEditing Mode="EditForm" />
    <SettingsLoadingPanel Mode="ShowAsPopup" />
    <SettingsPager Position="Bottom">
        <SettingsTableLayout ColumnCount="1" RowsPerPage="1"/>
    </SettingsPager>
    <SettingsText Title="Loading screen ..." />
    <SettingsExport EnableClientSideExportAPI="true" CardWidth="600" />
    <Styles>
        <LoadingPanel CssClass="cssLoadingPanel" HorizontalAlign="Center" VerticalAlign="Middle" />
        <TitlePanel Font-Bold="true" />
    </Styles>
    <Columns>
        <dx:CardViewColumn Name="RowKey" FieldName="RowKey" Visible="false" />
    </Columns>
    <Toolbars>
        <dx:CardViewToolbar ItemAlign="Right">
            <Items>
                <dx:CardViewToolbarItem Name="More" Text="More">
                    <Items>
                        <dx:CardViewToolbarItem Name="Inspect" Text="Inspect" />
                        <dx:CardViewToolbarItem Name="Designer" Text="Designer" Command="Custom" Target="_blank" />
                        <dx:CardViewToolbarItem Name="ExportToPdf" Command="ExportToPdf" />
                    </Items>
                </dx:CardViewToolbarItem>
                <dx:CardViewToolbarItem Name="Search" Text="Search" Image-IconID="actions_search_16x16devav" />
                <dx:CardViewToolbarItem Name="Quit" Text="Quit" Image-IconID="actions_cancel_16x16" Target="_top" />
                <dx:CardViewToolbarItem Name="Divider" Text="" Enabled="false" ItemStyle-Width="100%" />
                <dx:CardViewToolbarItem Name="New" Command="New"/>
                <dx:CardViewToolbarItem Name="Edit" Command="Edit"/>
                <dx:CardViewToolbarItem Name="Delete" Command="Delete"/>
                <dx:CardViewToolbarItem Name="Update" Command="Update" Text="Accept" Image-IconID="actions_apply_16x16" />
                <dx:CardViewToolbarItem Name="Cancel" Command="Cancel" Image-IconID="actions_cancel_16x16" />
            </Items>
        </dx:CardViewToolbar>
    </Toolbars>
    <Templates>
        <CardFooter>
            <table style="width:100%">
                <tr>
                    <td style="width:15px"></td>
                    <td>
                        <dx:ASPxPageControl ID="pgCardTabs" ClientInstanceName="pgCardTabs" Theme="" runat="server" Width="100%" EnableCallBacks="true" SaveStateToCookies="true"
                            OnInit="pgCardTabs_Init">
                        </dx:ASPxPageControl>
                    </td>
                    <td style="width:15px"></td>
                </tr>
                <td style="height: 15px"></td>
            </table>
        </CardFooter>
    </Templates>
</dx:ASPxCardView>

<div id="externalDropZone" class="dropZoneExternal">
    <div id="dropZone" class="hidden">
        <span class="dropZoneText">Drop a CSV file here to import</span>
    </div>
    <dx:ASPxGridView ID="dxGrid" ClientInstanceName="dxGrid" runat="server" Theme="" DataSourceID="MyTableData" AutoGenerateColumns="false" Width="100%" KeyFieldName="RowKey" CssClass="cssSmallFont"
        OnInit="dxGrid_Init" OnLoad="dxGrid_Load" OnCustomJSProperties="dxGrid_CustomJSProperties" OnCustomCallback="dxGrid_CustomCallback" OnSummaryDisplayText="dxGrid_SummaryDisplayText" OnCustomColumnDisplayText="dxGrid_CustomColumnDisplayText"
        OnBeforeColumnSortingGrouping="dxGrid_BeforeColumnSortingGrouping" OnInitNewRow="dxGrid_InitNewRow" OnToolbarItemClick="dxGrid_ToolbarItemClick"
        OnRowValidating="dxGrid_RowValidating" OnRowUpdating="dxGrid_RowUpdating" OnRowInserting="dxGrid_RowInserting" OnStartRowEditing="dxGrid_StartRowEditing">
        <ClientSideEvents Init="dxGrid_Init" />
        <ClientSideEvents BeginCallback="dxGrid_BeginCallback" />
        <ClientSideEvents EndCallback="dxGrid_EndCallback" />
        <ClientSideEvents DetailRowExpanding="dxGrid_DetailRowExpanding" />
        <ClientSideEvents DetailRowCollapsing="dxGrid_DetailRowCollapsing" />
        <ClientSideEvents FocusedRowChanged="dxGrid_FocusedRowChanged" />
        <ClientSideEvents ToolbarItemClick="dxGrid_ToolbarItemClick" />
        <ClientSideEvents RowDblClick="dxGrid_RowDblClick" />
        <ClientSideEvents ContextMenu="dxGrid_ContextMenu" />
        <Settings ShowGroupPanel="true" ShowGroupedColumns="true" ShowTitlePanel="true" ShowFooter="true" />
        <SettingsBehavior ColumnResizeMode="Control" AllowSelectSingleRowOnly="true" AllowFocusedRow="true" AllowSelectByRowClick="true" ConfirmDelete="true" />
        <SettingsDetail AllowOnlyOneMasterRowExpanded="true" />
        <SettingsEditing Mode="Inline" NewItemRowPosition="Bottom" />
        <SettingsLoadingPanel Mode="ShowAsPopup" />
        <SettingsPager Mode="ShowAllRecords" />
        <SettingsText Title="Loading screen ..." />
        <SettingsExport EnableClientSideExportAPI="true" ExcelExportMode="WYSIWYG" />
        <Styles>
            <LoadingPanel CssClass="cssLoadingPanel" HorizontalAlign="Center" VerticalAlign="Middle" />
            <TitlePanel Font-Bold="true" />
        </Styles>
        <Columns>
            <dx:GridViewDataColumn Name="RowKey" FieldName="RowKey" Visible="false" />
        </Columns>
        <Toolbars>
            <dx:GridViewToolbar ItemAlign="Right">
                <Items>
                    <dx:GridViewToolbarItem Name="More" Text="More">
                        <Items>
                            <dx:GridViewToolbarItem Name="Inspect" Text="Inspect" />
                            <dx:GridViewToolbarItem Name="Designer" Text="Designer" Command="Custom" Target="_blank" />
                            <dx:GridViewToolbarItem Name="ExportToPdf" Command="ExportToPdf" />
                            <dx:GridViewToolbarItem Name="ExportToXlsx" Command="ExportToXlsx" />
                        </Items>
                    </dx:GridViewToolbarItem>
                    <dx:GridViewToolbarItem Name="Search" Text="Search" Image-IconID="actions_search_16x16devav"  />
                    <dx:GridViewToolbarItem Name="Quit" Text="Quit" Image-IconID="actions_cancel_16x16" />
                    <dx:GridViewToolbarItem Name="Divider" Text="" Enabled="false"  ItemStyle-Width="100%" />
                    <dx:GridViewToolbarItem Name="New" Command="New" />
                </Items>
            </dx:GridViewToolbar>
        </Toolbars>
        <Templates>
            <DetailRow>
                <dx:ASPxPageControl ID="pgGridTabs" ClientInstanceName="pgGridTabs" Theme="" runat="server" Width="100%" OnInit="pgGridTabs_Init">
                </dx:ASPxPageControl>
            </DetailRow>
        </Templates>    
    </dx:ASPxGridView>
</div>

<asp:ObjectDataSource ID="MyTableData" runat="server" TypeName="CodeClay.CiDataSource"
    SelectMethod="SelectTable" UpdateMethod="UpdateTable" InsertMethod="InsertTable" DeleteMethod="DeleteTable"
    OnSelecting="MyTableData_Selecting" OnUpdating="MyTableData_Updating" OnInserting="MyTableData_Inserting" OnDeleting="MyTableData_Deleting"
    OnSelected="MyTableData_Selected" OnUpdated="MyTableData_Updated" OnInserted="MyTableData_Inserted" OnDeleted="MyTableData_Deleted">
    <SelectParameters>
        <asp:Parameter Name="Table" Type="Object" />
        <asp:Parameter Name="View" Type="String" />
        <asp:Parameter Name="Parameters" Type="Object" />
        <asp:Parameter Name="Script" Type="String" Direction="Output" />
    </SelectParameters>
    <UpdateParameters>
        <asp:Parameter Name="Table" Type="Object"  />
        <asp:Parameter Name="View" Type="String" />
        <asp:Parameter Name="Parameters" Type="Object" />
        <asp:Parameter Name="RowKey" Type="String" />
        <asp:Parameter Name="Script" Type="String" Direction="Output" />
        <asp:Parameter Name="IsInvalid" Type="Boolean" Direction="Output" />
    </UpdateParameters>
    <InsertParameters>
        <asp:Parameter Name="Table" Type="Object"  />
        <asp:Parameter Name="View" Type="String" />
        <asp:Parameter Name="Parameters" Type="Object" />
        <asp:Parameter Name="RowKey" Type="String" Direction="InputOutput" />
        <asp:Parameter Name="Script" Type="String" Direction="Output" />
        <asp:Parameter Name="IsInvalid" Type="Boolean" Direction="Output" />
    </InsertParameters>
    <DeleteParameters>
        <asp:Parameter Name="Table" Type="Object"  />
        <asp:Parameter Name="View" Type="String" />
        <asp:Parameter Name="Parameters" Type="Object" />
        <asp:Parameter Name="RowKey" Type="String" />
        <asp:Parameter Name="Script" Type="String" Direction="Output" />
        <asp:Parameter Name="IsInvalid" Type="Boolean" Direction="Output" />
    </DeleteParameters>
</asp:ObjectDataSource>
            
<dx:ASPxCallbackPanel ID="dxOpenMenuPanel" ClientInstanceName="dxOpenMenuPanel" runat="server" Theme="" OnCallback="dxOpenMenuPanel_Callback">
    <SettingsLoadingPanel Enabled="false" />
    <ClientSideEvents Init="dxOpenMenuPanel_Init" />
    <ClientSideEvents EndCallback="dxOpenMenuPanel_EndCallback" />
    <PanelCollection>
        <dx:PanelContent>
            <dx:ASPxPopupMenu ID="dxPopupMenu" ClientInstanceName="dxPopupMenu" runat="server" Theme="">
                <ClientSideEvents Init="dxPopupMenu_Init" />
                <ClientSideEvents ItemClick="dxPopupMenu_ItemClick" />
            </dx:ASPxPopupMenu>
        </dx:PanelContent>
    </PanelCollection>
</dx:ASPxCallbackPanel>

<dx:ASPxCallback ID="dxClickMenuPanel" ClientInstanceName="dxClickMenuPanel" runat="server" OnCallback="dxClickMenuPanel_Callback">
    <ClientSideEvents Init="dxClickMenuPanel_Init" />
    <ClientSideEvents EndCallback="dxClickMenuPanel_EndCallback" />
</dx:ASPxCallback>

<dx:ASPxLoadingPanel ID="dxLoadingPanel" ClientInstanceName="dxLoadingPanel" runat="server">
    <ClientSideEvents Init="dxLoadingPanel_Init" />
</dx:ASPxLoadingPanel>

<dx:ASPxUploadControl ID="dxImportCSV" ClientInstanceName="dxImportCSV" runat="server" Theme="" Width="100%"
    OnFileUploadComplete="dxImportCSV_FileUploadComplete" AutoStartUpload="true" CssClass="hidden">
    <AdvancedModeSettings EnableDragAndDrop="True" EnableFileList="False" EnableMultiSelect="False" ExternalDropZoneID="externalDropZone" DropZoneText="" />
    <ValidationSettings AllowedFileExtensions=".csv" ErrorStyle-CssClass="validationMessage" />
    <BrowseButton Text="Select a file for upload..." />
    <DropZoneStyle CssClass="uploadControlDropZone" />
    <ClientSideEvents
        DropZoneEnter="function(s, e) { if(e.dropZone.id == 'externalDropZone') setElementVisible('dropZone', true); }"
        DropZoneLeave="function(s, e) { if(e.dropZone.id == 'externalDropZone') setElementVisible('dropZone', false); }"
        FileUploadComplete="dxImportCSV_FileUploadComplete">
    </ClientSideEvents>
</dx:ASPxUploadControl>