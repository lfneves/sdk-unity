using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Roar;
using Roar.DomainObjects;

public class RoarRankingsWidget : RoarUIWidget
{
	public enum WhenToFetch { OnEnable, Once, Occassionally, Manual };
	public WhenToFetch whenToFetch = WhenToFetch.OnEnable;
	public float howOftenToFetch = 60;
	
	public Rect rankingItemBounds;
	public float rankingItemSpacing;
	public string rankingEntryPlayerRankStyle = "LeaderboardRankingPlayerRank";
	public string rankingEntryPlayerNameStyle = "LeaderboardRankingPlayerName";
	public string rankingEntryPlayerScoreStyle = "LeaderboardRankingPlayerScore";

	//public string rankingNavigatePageValueStyle = "LabelPageValue";
	//public string rankingNavigateLeftButtonStyle = "ButtonNavigatePageLeft";
	//public string rankingNavigateRightButtonStyle = "ButtonNavigatePageRight";
	
	public string leaderboardKey = string.Empty;
	public int page = 1;
	
	private Roar.Components.ILeaderboards boards;	
	private bool isFetching;
	private IList<LeaderboardEntry> leaderboard;
	
	protected override void Awake()
	{
		base.Awake();
		ScrollViewContentWidth = rankingItemBounds.width;
	}
	
	protected override void OnEnable()
	{
		base.OnEnable();
		RoarLeaderboardsWidget.OnLeaderboardsFetchedStarted += OnLeaderboardsFetchedStarted;
		RoarLeaderboardsWidget.OnLeaderboardsFetchedComplete += OnLeaderboardsFetchedComplete;
		RoarLeaderboardsWidget.OnLeaderboardSelected += OnLeaderboardSelected;
		
		boards = roar.Leaderboards;
		
		FetchIfPossible();
	}
	
	protected override void OnDisable()
	{
		RoarLeaderboardsWidget.OnLeaderboardsFetchedStarted -= OnLeaderboardsFetchedStarted;
		RoarLeaderboardsWidget.OnLeaderboardsFetchedComplete -= OnLeaderboardsFetchedComplete;
		RoarLeaderboardsWidget.OnLeaderboardSelected -= OnLeaderboardSelected;
	}
	
	void OnLeaderboardsFetchedStarted()
	{}
	
	void OnLeaderboardsFetchedComplete()
	{
		FetchIfPossible();
	}
	
	void OnLeaderboardSelected(string leaderboardKey)
	{
		this.leaderboardKey = leaderboardKey;
		this.page = 1;
		FetchIfPossible();
	}
	
	void FetchIfPossible()
	{
		if (!string.IsNullOrEmpty(leaderboardKey))
		{
			leaderboard = boards.GetLeaderboard(leaderboardKey);
		}
		
		if (leaderboard == null)
		{
			Fetch();
		}
	}
	
	public void Fetch()
	{
		if (string.IsNullOrEmpty(leaderboardKey))
		{
			Debug.Log("leaderboardKey not set!");
		}
		boards.FetchBoard( leaderboardKey, OnRoarFetchLeaderboardComplete );
	}
	
	void OnRoarFetchLeaderboardComplete(Roar.CallbackInfo<ILeaderboardCache> cache)
	{
		/*
		whenLastFetched = Time.realtimeSinceStartup;
		isFetching = false;
		leaderboards = boards.BoardList();
		
		// set default labels
		foreach (LeaderboardInfo leaderboard in leaderboards)
		{
			if (string.IsNullOrEmpty(leaderboard.label))
				leaderboard.label = string.Format("Leaderboard{0} - {1}", leaderboard.board_id, leaderboard.ikey);
		}
		
		ScrollViewContentHeight = leaderboards.Count * (leaderboardItemBounds.height + leaderboardItemSpacing);
		
		if (OnLeaderboardsFetchedComplete != null)
			OnLeaderboardsFetchedComplete();
		*/
	}
	
	public void Fetch(int page)
	{
		this.page = page;
		Fetch();
	}
	
	public void Fetch(string leaderboardKey, int page)
	{
		this.page = page;
		this.leaderboardKey = leaderboardKey;
		Fetch();
	}
	
	
	protected override void DrawGUI(int windowId)
	{
		if (isFetching)
		{
			GUI.Label(new Rect(0,0,ContentWidth,ContentHeight), "Fetching leaderboard ranking data...", "StatusNormal");
			ScrollViewContentHeight = 0;
		}
		else
		{
			if (leaderboard == null || leaderboard.Count == 0)
			{
				GUI.Label(new Rect(0,0,ContentWidth,ContentHeight), "No ranking data.", "StatusNormal");
				ScrollViewContentHeight = 0;
			}
			else
			{
				ScrollViewContentHeight = leaderboard.Count * (rankingItemBounds.height + rankingItemSpacing);
				Rect entryRect = rankingItemBounds;
				foreach (LeaderboardEntry leaderboardEntry in leaderboard)
				{
					GUI.Label(entryRect, leaderboardEntry.rank.ToString(), rankingEntryPlayerRankStyle);
					//GUI.Label(entry, leaderboardEntry.playerName, rankingEntryPlayerNameStyle);
					GUI.Label(entryRect, leaderboardEntry.value.ToString(), rankingEntryPlayerScoreStyle);
					
					entryRect.y += entryRect.height + rankingItemSpacing;
				}
				//useScrollView = utilizeScrollView && ((entry.y + entry.height) > contentBounds.height);
			}
		}
	}
	
	/*
	void GUIPageNavigator(Rect rect)
	{
		GUIStyle navigateButtonStyle;
		float w = rect.width;
		//float h = rect.height;

		GUI.BeginGroup(rect);
		rect.x = 0;
		rect.y = 0;
		
		navigateButtonStyle = skin.FindStyle(rankingNavigateLeftButtonStyle);
		rect.width = navigateButtonStyle.fixedWidth;
		rect.height = navigateButtonStyle.fixedHeight;
		if (activeLeaderboard.HasPrevious)
		{
			if (GUI.Button(rect, string.Empty, rankingNavigateLeftButtonStyle))
			{
				FetchRankings(activeLeaderboard, activeLeaderboard.page + 1);
			}
		}
		
		rect.width = w;
		if (activeLeaderboard.HasPrevious || activeLeaderboard.HasNext)
			GUI.Label(rect, activeLeaderboard.page.ToString(), rankingNavigatePageValueStyle);

		navigateButtonStyle = skin.FindStyle(rankingNavigateRightButtonStyle);
		rect.width = navigateButtonStyle.fixedWidth;
		rect.height = navigateButtonStyle.fixedHeight;
		rect.x = w - rect.width;
		if (activeLeaderboard.HasNext)
		{
			if (GUI.Button(rect, string.Empty, rankingNavigateRightButtonStyle))
			{
				FetchRankings(activeLeaderboard, activeLeaderboard.page - 1);
			}
		}
		
		GUI.EndGroup();
	}
	*/
}
