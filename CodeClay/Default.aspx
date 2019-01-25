<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CodeClay.Default" MasterPageFile="~/Site.Master"  %>
<%@ Register Assembly="DevExpress.Web.v18.1, Version=18.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <style>   
        input.dxeEditAreaSys,
        select.dxeEditAreaSys,
        textarea.dxeMemoEditAreaSys {
            max-width: 2000px;
        }
    </style>

    <script>
    	var postponedCallbackRequired = false;
        var iframe;

        function dxPopup_Init(sender, event) {
    		iframe = dxPopup.GetContentIFrame();

    		/* the "load" event is fired when the content has been already loaded */
    		if (iframe) {
    			ASPxClientUtils.AttachEventToElement(iframe, 'load', iframe_ContentLoaded);
    		}
    	}

    	function dxPopup_Shown(sender, event) {
    		if (dxPopup.IsVisible()) {
    			dxLoadingPanel.ShowInElement(iframe);
    		}
    	}

    	function iframe_ContentLoaded(event) {
    		dxLoadingPanel.Hide();
    	}

        function ShowPopup(url) {
            dxPopup.SetContentUrl(url);
            dxPopup.Show();
        }

        function HidePopup() {
            dxPopup.Hide();
        }

        function Navigate(puxFile) {
            if (myCallbackPanel.InCallback()) {
                postponedCallbackRequired = true;
            }
            else {
                myCallbackPanel.PerformCallback(puxFile);
            }
        }

        function myCallbackPanel_EndCallback(sender, event) {
            if (postponedCallbackRequired) {
                myCallbackPanel.PerformCallback();
                postponedCallbackRequired = false;
            }
        }
    </script>

    <asp:Panel ID ="myOuterPanel" runat="server">
        <dx:ASPxCallbackPanel ID="myCallbackPanel" ClientInstanceName="myCallbackPanel" runat="server" OnCallback="myCallbackPanel_Callback">
            <ClientSideEvents EndCallback="myCallbackPanel_EndCallback" />
            <SettingsLoadingPanel Enabled="true" />
            <PanelCollection>
                <dx:PanelContent>
                    <asp:Panel ID ="myInnerPanel" runat="server"></asp:Panel>
                </dx:PanelContent>
            </PanelCollection>
        </dx:ASPxCallbackPanel>
    </asp:Panel>

    <dx:ASPxLoadingPanel ID="dxLoadingPanel" runat="server" ClientInstanceName="dxLoadingPanel" Theme="Aqua" />

    <dx:ASPxPopupControl ID="dxPopup" ClientInstanceName="dxPopup" runat="server" ContentUrl="javascript:void(0);" CloseAction="CloseButton" CloseOnEscape="true" Modal="True" Theme="Aqua" AllowResize="true"
        PopupHorizontalAlign="WindowCenter" PopupVerticalAlign="WindowCenter" HeaderText="Popup" AllowDragging="True" PopupAnimationType="None" EnableViewState="False"
        Height="800" Width="1200">
        <ClientSideEvents Init="dxPopup_Init" />
        <ClientSideEvents Shown="dxPopup_Shown" />
    </dx:ASPxPopupControl>

</asp:Content>