<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UiMenu.ascx.cs" Inherits="CodeClay.UiMenu" %>
<%@ Register Assembly="DevExpress.Web.v20.2, Version=20.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<asp:LoginView ID="loginView" runat="server">
    <AnonymousTemplate>
        <dx:ASPxMenu ID="dxAnonymousMenu" ClientInstanceName="dxAnonymousMenu" runat="server" Theme="" AutoSeparators="RootOnly" AutoPostBack="false" Width="100%">
            <Items>
                <dx:MenuItem Name="Home" Text="Home" NavigateUrl="~" ItemStyle-Width="8%"></dx:MenuItem>
                <dx:MenuItem Name="About" Text="About" NavigateUrl="~/About" ItemStyle-Width="8%"></dx:MenuItem>
                <dx:MenuItem Name="Contact" Text="Contact" NavigateUrl="~/Contact" ItemStyle-Width="8%"></dx:MenuItem>
                <dx:MenuItem Name="Divider" Text=" " ItemStyle-Width="60%" Enabled="false"></dx:MenuItem>
                <dx:MenuItem Name="Register" Text="Register" NavigateUrl="~/Account/Register" ItemStyle-Width="8%"></dx:MenuItem>
                <dx:MenuItem Name="Login" Text="Log In" NavigateUrl="~/Account/Login" ItemStyle-Width="8%"></dx:MenuItem>
            </Items>
            <Paddings Padding="3px" />
        </dx:ASPxMenu>
    </AnonymousTemplate>
    <LoggedInTemplate>
        <dx:ASPxMenu ID="dxMenu" ClientInstanceName="dxMenu" runat="server" Theme="" AutoSeparators="RootOnly" AutoPostBack="false" Width="100%" OnInit="dxMenu_Init">
            <Items>
                <dx:MenuItem Name="Home" Text="Home" NavigateUrl="~/Default.aspx?Application=CPanel" ItemStyle-Width="8%"></dx:MenuItem>
                <dx:MenuItem Name="About" Text="About" NavigateUrl="~/About" ItemStyle-Width="8%"></dx:MenuItem>
                <dx:MenuItem Name="Contact" Text="Contact" NavigateUrl="~/Contact" ItemStyle-Width="8%"></dx:MenuItem>
                <dx:MenuItem Name="Divider" Text=" " ItemStyle-Width="60%" Enabled="false"></dx:MenuItem>
                <dx:MenuItem Name="User" Text="Aloha, Adrian" ItemStyle-Width="8%">
                    <Items>
                        <dx:MenuItem Name="Logout">
                            <Template>
                                <asp:LoginStatus runat="server" LogoutText="Log Out" LogoutAction="Redirect" LogoutPageUrl="~/Account/Login" OnLoggingOut="LoginStatus_LoggingOut"/>
                            </Template>
                        </dx:MenuItem>
                        <dx:MenuItem Name="SaveDesign" Text="Save page design"></dx:MenuItem>
                        <dx:MenuItem Name="SiteSettings" Text="Site settings"></dx:MenuItem>
                        <dx:MenuItem Name="Manage" Text="Manage" NavigateUrl="~/Account/Manage"></dx:MenuItem>
                    </Items>
                </dx:MenuItem>
            </Items>
            <Paddings Padding="3px" />
        </dx:ASPxMenu>
    </LoggedInTemplate>
</asp:LoginView>