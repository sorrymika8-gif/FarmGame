using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.Example
{
	// Generate Id:e6882432-e964-4d11-b150-8462e85c52c4
	public partial class MainUIPanel
	{
		public const string Name = "MainUIPanel";
		
		
		private MainUIPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			
			mData = null;
		}
		
		public MainUIPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		MainUIPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new MainUIPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
