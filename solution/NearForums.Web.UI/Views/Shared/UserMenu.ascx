<%@ Control Language="C#" Inherits="NearForums.Web.UI.BaseViewUserControl" %>
<%
if (Config.AuthorizationProviders.Facebook.IsDefined)
{
%>
	<script src="http://static.ak.connect.facebook.com/js/api_lib/v0.4/FeatureLoader.js.php/<%=System.Threading.Thread.CurrentThread.CurrentUICulture %>" type="text/javascript"></script>
	<script type="text/javascript">
		FB.init('<%= Config.AuthorizationProviders.Facebook.ApiKey %>', '<%=Url.Action("FacebookReceiver", "Authentication") %>');
	</script>
<%
}
%>
<ul id="userMenu">
	<li class="toContent"><a href="#content" accesskey="1">Go to content</a></li>
<%
	if (this.User == null)
	{
		if (Config.AuthorizationProviders.Facebook.IsDefined)
		{
%>
		<li class="login">
			<a onclick="FB.Connect.requireSession(function(){window.location.reload();});return false;" href="#"><img src="http://static.ak.fbcdn.net/images/fbconnect/login-buttons/connect_light_medium_short.gif" alt="Login using facebook" /></a>
		</li>
<%
		}
		if (Config.AuthorizationProviders.Twitter.IsDefined)
		{
%>
		<li class="login"><a href="<%=Url.Action("TwitterStartLogin", "Authentication", new{returnUrl=this.Request.Url.PathAndQuery}) %>"><img src="http://a0.twimg.com/images/dev/buttons/sign-in-with-twitter-l-sm.png" alt="Sign in with twitter" /></a></li>
<%
		}
	}
	else
	{
		if (this.User.Group >= UserGroup.Moderator)
		{
%>
		<li class="admin"><%=Html.ActionLink("Admin dashboard", "Dashboard", "Admin")%></li>
<%
		}
%>
		<li class="userName"><%=Html.ActionLink(this.User.UserName, "Detail", "Users", new{id=User.Id}, null) %></li>
		<li class="logged"><%=Html.ActionLink("Edit Profile", "Edit", "Users", new{id=User.Id}, null) %></li>
<% 
		if (this.User.Provider == AuthenticationProvider.Facebook)
		{		
%>
			<li class="logout"><a href="#" onclick="FB.Connect.logoutAndRedirect('<%=Url.Action("Logout", "Authentication", new{returnUrl=Request.Url.PathAndQuery}) %>');return false;">Logout</a></li>
<%
		}
		else
		{
%>
			<li class="logout"><%=Html.ActionLink("Logout", "Logout", "Authentication")%></li>
<%		
		}
	}
%>
</ul>