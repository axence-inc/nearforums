<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Admin - Website status
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="HeadContent">
	<script type="text/javascript" src="/scripts/jquery-1.3.2.min.js"></script>
	<script type="text/javascript">
		function tm(id)
		{
			$("#m-" + id).slideToggle();
			return false;
		}
	</script>
	<style type="text/css">
		p.message
		{
			padding: 0 0 5px 5px;
			position: relative;
			top: -5px;
			font-size: 12px;
			color: #666666;
			display: none;
		}
	</style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<ul class="path floatContainer">
		<li class="first"><%=Html.ActionLink("Forums", "List", "Forums") %></li>
		<li><%=Html.ActionLink("Admin", "Dashboard", "Admin") %></li>
	</ul>
    <h1>Website status</h1>
<%
	if (ViewData["WillNotRun"] != null)
	{
%>
		<p style="color: red;"><strong>The site will not run, check the full report below.</strong></p>
<%
	}	
%>
	<h2>Webserver</h2>
	<p><strong>Machine name</strong>: <%=ViewData["MachineName"] %></p>
    <p><strong><a href="#" onclick="return tm('connectivity');">Connectivity test</a></strong>: <%=ViewData["Connectivity"]%></p>
    <p id="m-connectivity" class="message">
		The test is a http get request from the webserver to google.com. If it fails, check: network connectivity; Proxy settings (plus credentials) on web.config.
    </p>
    <p><strong><a href="#" onclick="return tm('proxy');">Proxy - enabled</a></strong>: <%=ViewData["Proxy"]%></p>
    <p id="m-proxy" class="message">
		Can be configured in the web.config file: system.net/defaultProxy. Remember to consider the credentials executing the application.
    </p>
    <p><strong>Proxy - address</strong>: <%=ViewData["Proxy-Address"]%></p>
    <p><strong><a href="#" onclick="return tm('mail');">Mail client</a></strong>: <%=ViewData["Mail"]%></p>
    <p id="m-mail" class="message">
		Can be configured in the web.config file: system.net/mailSettings/smtp. Deliverymethod and From attributes.
    </p>
    
	<h2>Database</h2>
	<p><strong><a href="#" onclick="return tm('connString');">Connection string</a></strong>: <%=ViewData["ConnectionString"] %></p>
	<p id="m-connString" class="message">
		Must be configured in the web.config, connectionStrings section, with the key &quot;Forums&quot;.
    </p>
	<p><strong>Provider</strong>: <%=ViewData["ConnectionStringProvider"] %></p>
	<p><strong>Test</strong>: <%=ViewData["DatabaseTest"] %></p>
	
    
    <h2>Website</h2>
    <p><strong>Nearforums version</strong>: <%=ViewData["Version"] %></p>
	<p><strong>Webpages compilation debug</strong>: <%=ViewData["Debug"] %></p>
	<p><strong>Custom errors</strong>: <%=ViewData["CustomErrors"] %></p>
    <p><strong>Notifications - Subscriptions - enabled</strong>: <%=ViewData["Subscriptions"]%></p>
	<p><strong>Logging - enabled</strong>: <%=ViewData["LoggingEnabled"] %></p>
    
    <h2>Authorization providers</h2>
    <p><strong>Facebook - configured</strong>: <%=ViewData["Facebook"]%></p>
    <p><strong>Twitter - configured</strong>: <%=ViewData["Twitter"]%></p>
    <p><strong><a href="#" onclick="return tm('twitter');">Twitter - test</a></strong>: <%=ViewData["Twitter-Test"]%></p>
	<p id="m-twitter" class="message">
		The test consists on getting a request token from twitter. If it fails, check: the application configuration on twitter.com; the network connectivity; the apikey and secret on site.config file.
    </p>
    <p><strong>SSO through open id - configured</strong>: <%=ViewData["SSOOpenId"]%></p>
    <p><strong><a href="#" onclick="return tm('ssoopenid');">SSO through open id - test</a></strong>: <%=ViewData["SSOOpenId-Test"]%></p>
	<p id="m-ssoopenid" class="message">
		The test consists on getting a request token from service provider. If it fails, check: the service provider; the network connectivity; the identifier url on site.config file.
    </p>
    
<%
	if (ViewData["StatusError"] != null)
	{
%>
		<p style="padding-top: 20px;color: red;"><em>There were errors while executing the status. Below you can see the error message (a full description of the error was logged):</em></p>
		<p style="color: red;"><%=ViewData["StatusError"]%></p>
<%
	}
%>
</asp:Content>