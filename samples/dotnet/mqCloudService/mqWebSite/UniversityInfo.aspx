<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UniversityInfo.aspx.cs" Inherits="mqWebSite.UniversityInfo" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>University Student Information System</title>
    <link href="UniversityInfo.css" rel="stylesheet"/>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <div id="mainTitle">
        <h1>University Information</h1>
        <div id="buttonTag">
            <asp:LinkButton ID="refreshBtn" runat="server" Text="Refresh" OnClick="refreshBtn_Click" />
        </div>
    </div>
    <br />
    <br />
        <div>
            <asp:Label ID="infoLabel" Text="Disclaimer: This information listed here is fictitios in nature and is meant for demonstration purpose only" runat="server" Width="1016px" ForeColor="Red" BackColor="White"></asp:Label>
        </div>
    <br />
    <div id="main">
        <asp:Table runat="server" ID="collegeInfoHeaderTable">
            <asp:TableRow ForeColor="Teal" Font-Bold="true">
                <asp:TableCell Width="400px">Name</asp:TableCell>
                <asp:TableCell Width="500px">Address</asp:TableCell>
                <asp:TableCell Width ="100px">Rating</asp:TableCell>
            </asp:TableRow>
        </asp:Table>
        <asp:Table runat="server" ID="collegeInfoTable">
            <asp:TableRow ForeColor="Teal" Font-Bold="true">
                <asp:TableCell Width ="400px">Name</asp:TableCell>
                <asp:TableCell Width ="500px">Address</asp:TableCell>
                <asp:TableCell Width ="100px">Rating</asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </div>
    <div id="divTableStyle">
    </div>
    <div>
        <footer id="foot01"></footer>
    </div>
    <script src="Today.js"></script>       
    </form>
</body>
</html>
