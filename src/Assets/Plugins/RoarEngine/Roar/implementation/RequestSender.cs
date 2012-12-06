using System.Collections;
using UnityEngine;

public class RequestSender : IRequestSender
{
	protected Roar.IConfig config;
	
	protected string GameKey { get { return config.Game; } }
	protected string RoarAuthToken { get { return config.AuthToken; } set { config.AuthToken = value; } }
	protected string RoarAPIUrl { get { return config.RoarAPIUrl; } }
	protected bool IsDebug { get { return config.IsDebug; } }
	
	protected IUnityObject unityObject;
	protected Roar.ILogger logger;

	public RequestSender( Roar.IConfig config, IUnityObject unityObject, Roar.ILogger logger )
	{
		this.config=config;
		this.unityObject = unityObject;
		this.logger = logger;
	}
		
	public void MakeCall( string apicall, Hashtable args, IRequestCallback cb )
	{
		unityObject.DoCoroutine( SendCore( apicall, args, cb ) );
	}
	
	protected IEnumerator SendCore( string apicall, Hashtable args, IRequestCallback cb )
	{
		if ( GameKey == "")
		{
			logger.DebugLog("[roar] -- No game key set!--");
			yield break;
		}
	
		logger.DebugLog("[roar] -- Calling: "+apicall);
		
		// Encode POST parameters
		WWWForm post = new WWWForm();
		if(args!=null)
		{
			foreach (DictionaryEntry param in args)
			{
				//Debug.Log(string.Format("{0} => {1}", param.Key, param.Value));
				post.AddField( param.Key as string, param.Value as string );
			}
		}
		// Add the auth_token to the POST
		post.AddField( "auth_token", RoarAuthToken );
		
		// Fire call sending event
		RoarManager.OnRoarNetworkStart();
		
		//Debug.Log ( "roar_api_url = " + RoarAPIUrl );
		if (Debug.isDebugBuild)
			Debug.Log ( "Requesting : " + RoarAPIUrl+GameKey+"/"+apicall+"/" );
		
		var xhr = new WWW( RoarAPIUrl+GameKey+"/"+apicall+"/", post);
		yield return xhr;
		
		OnServerResponse( xhr.text, apicall, cb );
	}
	
	
	protected void OnServerResponse( string raw, string apicall, IRequestCallback cb )
	{
		var uc = apicall.Split("/"[0]);
		var controller = uc[0];
		var action = uc[1];
		
		if (Debug.isDebugBuild)
			Debug.Log(raw);
		
		// Fire call complete event
		RoarManager.OnRoarNetworkEnd("no id");
		
		// -- Parse the Roar response
		// Unexpected server response 
		if (raw[0] != '<')
		{
			// Error: fire the error callback
			IXMLNode errorXml = IXMLNodeFactory.instance.Create("error", raw);
			if (cb!=null) cb.OnRequest( new Roar.RequestResult(errorXml, IWebAPI.FATAL_ERROR, "Invalid server response" ) );
			return;
		}
	
		IXMLNode rootNode = IXMLNodeFactory.instance.Create( raw );
		
		int callback_code;
		string callback_msg="";
		
		IXMLNode actionNode = rootNode.GetNode( "roar>0>"+controller+">0>"+action+">0" );
		// Hash XML keeping _name and _text values by default
		
		// Pre-process <server> block if any and attach any processed data
		IXMLNode serverNode = rootNode.GetNode( "roar>0>server>0" );
		RoarManager.NotifyOfServerChanges( serverNode );
		
		// Status on Server returned an error. Action did not succeed.
		string status = actionNode.GetAttribute( "status" );
		if (status == "error")
		{
			callback_code = IWebAPI.UNKNOWN_ERR;
			callback_msg = actionNode.GetFirstChild("error").Text;
			string server_error = actionNode.GetFirstChild("error").GetAttribute("type");
			if ( server_error == "0" )
			{
				if (callback_msg=="Must be logged in") { callback_code = IWebAPI.UNAUTHORIZED; }
				if (callback_msg=="Invalid auth_token") { callback_code = IWebAPI.UNAUTHORIZED; }
				if (callback_msg=="Must specify auth_token") { callback_code = IWebAPI.BAD_INPUTS; }
				if (callback_msg=="Must specify name and hash") { callback_code = IWebAPI.BAD_INPUTS; }
				if (callback_msg=="Invalid name or password") { callback_code = IWebAPI.DISALLOWED; }
				if (callback_msg=="Player already exists") { callback_code = IWebAPI.DISALLOWED; }

				logger.DebugLog(string.Format("[roar] -- response error: {0} (api call = {1})", callback_msg, apicall));
			}
			
			// Error: fire the callback
			// NOTE: The Unity version ASSUMES callback = errorCallback
			if (cb!=null) cb.OnRequest( new Roar.RequestResult(rootNode, callback_code, callback_msg) );
		}
		
		// No error - pre-process the result
		else
		{
			IXMLNode auth_token = actionNode.GetFirstChild("auth_token");
			if (auth_token!=null && !string.IsNullOrEmpty(auth_token.Text)) RoarAuthToken = auth_token.Text;
			
			callback_code = IWebAPI.OK;
			if (cb!=null) cb.OnRequest( new Roar.RequestResult( rootNode, callback_code, callback_msg) );
		}
		
		RoarManager.OnCallComplete( new RoarManager.CallInfo( rootNode, callback_code, callback_msg, "no id" ) );
	}
}