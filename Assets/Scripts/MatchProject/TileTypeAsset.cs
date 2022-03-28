using UnityEngine;

namespace MatchProject
{
	[CreateAssetMenu(menuName = "Match 3 Project/Tile Type Asset")]
	public sealed class TileTypeAsset : ScriptableObject
	{
		public int id;

		public int value;

		public Sprite sprite;
	}
}
