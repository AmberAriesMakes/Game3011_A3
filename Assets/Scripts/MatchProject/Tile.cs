using UnityEngine;
using UnityEngine.UI;

namespace MatchProject
{
	public sealed class Tile : MonoBehaviour
	{
		public int x;
		public int y;

		private TileTypeAsset _type;
		public Image icon;

		public Button button;

		

		public TileTypeAsset Type
		{
			get => _type;

			set
			{
				if (_type == value) return;

				_type = value;

				icon.sprite = _type.sprite;
			}
		}

		public TileData Data => new TileData(x, y, _type.id);
	}
}
