using System;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
//Written by M. Gibson Bethke
//Thank you for my sanity, Jesus!
public class MusicManager : MonoBehaviour
{

	public GUISkin guiSkin;
	StartupManager startupManager;
	PaneManager paneManager;
	
	internal bool showMusicManager = false;
	MusicViewer musicViewer;
	
	string currentDirectory;
	string [] currentDirectoryDirectories;
	string [] currentDirectoryFiles;
	internal bool checkForChanges = false;
	
	Vector2 scrollPosition;
	
	bool showNewFolderWindow = false;

	Rect newFolderWindowRect = new Rect ( 0, 0, 350, 70 );
	internal Rect musicManagerPosition = new Rect (-1000, 0, 800, 600);
	internal string musicManagerTitle;
	
	List<string> availableSorts = new List<string>();
	

	void Start ()
	{

		startupManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<StartupManager>();
		paneManager = GameObject.FindGameObjectWithTag ( "Manager" ).GetComponent<PaneManager>();
		musicViewer = GameObject.FindGameObjectWithTag ( "MusicViewer" ).GetComponent<MusicViewer>();
		
		musicManagerPosition.width = Screen.width;
		musicManagerPosition.height = Screen.height;
		musicManagerPosition.x = -musicManagerPosition.width + -musicManagerPosition.width / 4;
		
		newFolderWindowRect.x = musicManagerPosition.width/2 - newFolderWindowRect.width/2;
		newFolderWindowRect.y = musicManagerPosition.height/2 - newFolderWindowRect.height/2;
		
		currentDirectory = startupManager.lastDirectory;
		
		string[] tempAvailableSorts = Directory.GetDirectories ( startupManager.mediaPath );
		foreach ( string directoryName in tempAvailableSorts )
		{
			
			availableSorts.Add ( directoryName.Substring ( startupManager.mediaPath.Length ));
		}
		
		currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
		currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
		
		InvokeRepeating ( "UpdateDirectories", 0, 2 );
	}
	
	
	void UpdateDirectories ()
	{
		
		if ( paneManager.currentPane == PaneManager.pane.musicManager )
		{
			
			try
			{
				currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
				currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
		
			} catch ( Exception e ) {
				
				if ( startupManager.developmentMode == true )
					UnityEngine.Debug.LogWarning ( e );
					
				currentDirectory = startupManager.mediaPath + "Albums";
				
				currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
				currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
			}
		}
	}
	

	void OnGUI ()
	{

		if ( showMusicManager == true )
		{
			
			GUI.skin = guiSkin;
			musicManagerPosition = GUI.Window ( 4, musicManagerPosition, MusicMakerPane, musicManagerTitle );
		}
		
		if ( showNewFolderWindow == true )
		{
		
			GUI.skin.window.normal.background = startupManager.popupWindowTexture;
			GUI.Window ( 7, newFolderWindowRect, NewFolderWindow, "New Folder" );
		} else {
			
			GUI.skin.window.normal.background = null;
		}
		
		GUI.skin = guiSkin;
	}
	

	void MusicMakerPane ( int wid )
	{

		GUILayout.Space ( musicManagerPosition.height / 6);	

		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		
		foreach ( string directoryName in availableSorts )
		{
			
			if ( GUILayout.Button ( directoryName ))
			{
				
				currentDirectory = startupManager.mediaPath + directoryName;
				currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
				currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
			}
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		GUILayout.Space ( musicManagerPosition.width / 2 - 300 );
		GUILayout.BeginVertical ();
		
		scrollPosition = GUILayout.BeginScrollView ( scrollPosition, GUILayout.Width( 600 ), GUILayout.Height (  musicManagerPosition.height - ( musicManagerPosition.height / 6 + 56 )));
		
		for ( int i = 0; i < currentDirectoryDirectories.Length; i += 1 )
		{
			
			if ( GUILayout.Button ( currentDirectoryDirectories[i].Substring ( currentDirectory.Length + 1 ) ))
			{
				
				currentDirectory = currentDirectoryDirectories[i];
				currentDirectoryDirectories = Directory.GetDirectories ( currentDirectory ).ToArray ();
				currentDirectoryFiles = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
			}
		}
		
		GUILayout.Space ( 20 );
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		
		if ( currentDirectoryFiles.Length > 0 )
		{
			
			GUILayout.Box ( "All songs in " + currentDirectory.Substring ( startupManager.mediaPath.Length ));
			GUILayout.Space ( 10 );
			
			for ( int i = 0; i < currentDirectoryFiles.Length; i += 1 )
			{
				
				GUILayout.Label ( currentDirectoryFiles[i].Substring ( currentDirectory.Length + 1 ));
			}
		} else {
			
			GUILayout.Box ( "There are no songs in this directory!" );
			GUILayout.Label ( "If you want to add some music to this folder,\nclick the 'Open current directory' button bellow." +
				"\n\nThen, drop any .wav or .ogg files into the folder that will appear.\n\nTo listen to any of your music, navigate to the MusicViewer (press the right arrow key)." );
		}
		
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		GUILayout.Space ( 20 );
		
		if ( GUILayout.Button ( "New Folder" ))
			showNewFolderWindow = true;
		
		if ( GUILayout.Button ( "Set as active directory" ))
			SetMusicViewerMedia ();
			
		if ( GUILayout.Button ( "Open current directory" ))
			Process.Start ( currentDirectory );
			
		GUILayout.EndScrollView ();
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
		
		if ( showNewFolderWindow == true )
		{
			
			GUI.DrawTexture ( new Rect ( 0, 0, musicManagerPosition.width, musicManagerPosition.height ), startupManager.underlay );
		}
	}
	
	
	void NewFolderWindow ( int wid )
	{
		
		GUI.FocusWindow ( 7 );
		GUI.BringWindowToFront ( 7 );
		
		if ( GUI.Button ( new Rect ( 275, 20, 55, 20 ), "Close" ))
		{

			GUI.FocusWindow ( 0 );
			GUI.BringWindowToFront ( 0 );
			paneManager.popupBlocking = false;
			newFolderWindowRect.height = 70;
			showNewFolderWindow = false;
		}
		
		GUI.Label ( new Rect ( -36, 17, 340, 25 ), "Input the link to an audio file in the textfield." );
		
		if ( GUI.Button ( new Rect ( 6, 45, 35, 20 ), "Create" ))
		{

			
		}
	}
	
	
	void SetMusicViewerMedia ()
	{
		
		musicViewer.mediaPath = currentDirectory;
		if ( currentDirectoryFiles.Length > 0 )
		{
			musicViewer.clipList = Directory.GetFiles ( currentDirectory, "*.*" ).Where ( s => s.EndsWith ( ".wav" ) || s.EndsWith ( ".ogg" ) || s.EndsWith ( ".unity3d" )).ToArray ();
			musicViewer.clipListEmpty = false;
		} else {
			
			Array.Clear( musicViewer.clipList, 0, musicViewer.clipList.Length);
			musicViewer.clipListEmpty = true;
		}
	}
}