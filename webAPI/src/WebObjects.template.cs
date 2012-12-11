using System;
using System.Collections;
using System.Collections.Generic;


namespace Roar.WebObjects
{
<%
  _.each( data.modules, function(m,i,l) {
    var ns_name = underscoreToCamel(m.name)
%>
	//Namespace for typesafe arguments and responses to Roars <%= m.name%>/foo calls.
	namespace <%= ns_name %>
	{
<% _.each( m.functions, function(f,j,ll) { %>
		// Arguments to <%= m.name %>/<%= f.name %>
		public class <%= underscoreToCamel(f.name) %>Arguments
		{
<% _.each( f.arguments, function(arg, k, lll) {
%>			public <%= arg.type %> <%= arg.name %>;<% if("note" in arg) { %> // <%= arg.note %> <% } %>
<% } ) %>
			public Hashtable ToHashtable()
			{
				Hashtable retval = new Hashtable();
<% _.each( f.arguments, function(arg, k, lll) {
if( arg.type == "string" )
{
%>				retval["<%= arg.name %>"] = <%= arg.name %>;
<%
} else {
%>				retval["<%= arg.name %>"] = Convert.ToString(<%= arg.name %>);
<%
}
} )
%>				return retval;
			}
		}
		
		// Response from <%= m.name %>/<%= f.name %>
		public class <%= underscoreToCamel(f.name) %>Response
		{
<%
  if( "response" in f ) { _.each( f.response.members, function( member, member_index ){
%>			public <%= member.type %> <%= member.name %>;
<% } ) } %>
		}
<% } ) %>
	}
<% } ) %>
	
}
