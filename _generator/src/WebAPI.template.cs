/*
Copyright (c) 2012, Run With Robots
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of the roar.io library nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY RUN WITH ROBOTS ''AS IS'' AND ANY
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL MICHAEL ANDERSON BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebAPI : IWebAPI
{
	protected IRequestSender requestSender_;

	public WebAPI (IRequestSender requestSender)
	{
		requestSender_ = requestSender;

<% _.each( data.modules, function(m,i,l) {
     print( "\t\t" +m.name + "_ = new " + underscoreToCamel(m.name)+"Actions (requestSender);\n" );
     } );
%>	}

<% _.each( data.modules, function(m,i,l) {
     print( "\tpublic override I" + underscoreToCamel(m.name) + "Actions "+m.name+" { get { return "+m.name+"_; } }\n\n" );
     print( "\tpublic " + underscoreToCamel(m.name)+"Actions " +m.name + "_;\n\n" );
     } );
%>

	public class APIBridge
	{
		protected IRequestSender api;

		public APIBridge (IRequestSender caller)
		{
			api = caller;
		}
	}
<%
  _.each( data.modules, function(m,i,l) {
    var class_name = underscoreToCamel(m.name)+"Actions"
%>
	public class <%= class_name %> : APIBridge, I<%= class_name %>
	{
<% _.each( m.functions, function(f,j,ll) {
     var response  = "Roar.WebObjects."+underscoreToCamel(m.name)+"."+underscoreToCamel(f.name)+"Response"
     var converter = f.name + "_response_parser";
%>		public Roar.DataConversion.IXmlToObject<<%= response %>> <%= converter %>;
<% } ) %>
		public <%= class_name %> (IRequestSender caller) : base(caller)
		{
<% _.each( m.functions, function(f,j,ll) {
     var response  = "Roar.WebObjects."+underscoreToCamel(m.name)+"."+underscoreToCamel(f.name)+"Response"
     var converter = f.name + "_response_parser";
     var converter_type = "Roar.DataConversion.Responses."+underscoreToCamel(m.name)+"."+underscoreToCamel(f.name)
%>			<%= converter %> = new <%= converter_type %>();
<% } ) %>
		}

<% _.each( m.functions, function(f,j,ll) {
     var arg = "Roar.WebObjects."+underscoreToCamel(m.name)+"."+underscoreToCamel(f.name)+"Arguments"
     var response  = "Roar.WebObjects."+underscoreToCamel(m.name)+"."+underscoreToCamel(f.name)+"Response"
     url = f.url ? f.url : (m.name+"/"+f.name);
     obj = f.obj ? f.obj : "obj";
     var requires_auth = ("requires_auth" in f) ? f.requires_auth : true;
     var converter = f.name + "_response_parser";
%>		public void <%= fix_reserved_word(f.name) %>( <%= arg %> args, ZWebAPI.Callback<<%= response %>> cb)
		{
			api.MakeCall ("<%= url %>", args.ToHashtable(), new CallbackBridge<<%= response %>>(cb, <%= converter %>), <%= requires_auth ? "true":"false" %>);
		}

<% } ) %>	}
<% } ) %>

}

