<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="mqWebSite.Login" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1 class="auto-style1">University Information<br />
                    <br />
                    A demo of IBM MQ on Azure</h1>
            </hgroup>
        </div>
        
        <style type="text/css">
            .padl{padding-left:340px}
            .auto-style1 {
                font-weight: normal;
            }
            .auto-style2 {
                padding-left: 340px;
                width: 35px;
                padding-left:10px;
                padding-right:50px;
            }
            .auto-style3 {
                padding-left:540px;
                width: 581px;
            }
            .auto-style4 {
                padding-left: 540px;
                width: 581px;
                height: 61px;
            }
            </style>

        <div>
            <table>
            <tr>
                <td class="auto-style3">
                    <asp:Label ID="errorLabel" runat="server" Text="Label" Width="400px" ForeColor="#FF3300" Visible="False"></asp:Label>
                    </td>
                <td class="auto-style2">
                    &nbsp;</td>
                <td class="padl">
                    &nbsp;</td>
            </tr>
            <tr>
                <td class="auto-style4">
                    User name:<br />
                    <asp:TextBox ID="txtUserId" runat="server" Width="150px" Wrap="False"></asp:TextBox>
                    <br />
                    <br />
                    Password:<br />
                    <asp:TextBox ID="txtPassword" textmode="Password" runat="server" Width="150px" Wrap="False"></asp:TextBox>
                    &nbsp;
                    <asp:Button ID="Login_MQ" runat="server" Text="Login" onclick="LoginMQ_Click" Width="72px" />
                </td>
            </tr>
            </table>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
</asp:Content>
