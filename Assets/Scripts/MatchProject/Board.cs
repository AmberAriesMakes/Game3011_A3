using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace MatchProject
{
	public sealed class Board : MonoBehaviour
	{
		[SerializeField] private float tweenDuration;

		[SerializeField] private Transform swappingOverlay;

		[SerializeField] private bool ensureNoStartingMatches;
		[SerializeField] public static bool Easy;
		[SerializeField] public static bool Medium;
		[SerializeField] public static bool Hard;
		[SerializeField] private TileTypeAsset[] tileTypes;

		[SerializeField] private Row[] rows;

		[SerializeField] private AudioClip matchSound;

		[SerializeField] private AudioSource audioSource;
		private int ScoreUp;
		

		private readonly List<Tile> _selection = new List<Tile>();

		private bool Swapped;
		private bool Match;
		private bool Shuffling;

		public event Action<TileTypeAsset, int> OnMatch;

		private TileData[,] Matrix
		{
			get
			{
				var width = rows.Max(row => row.tiles.Length);
				var height = rows.Length;

				var data = new TileData[width, height];

				for (var y = 0; y < height; y++)
					for (var x = 0; x < width; x++)
						data[x, y] = GetTile(x, y).Data;

				return data;
			}
		}

		private void Start()
		{
			
			print(Easy);
			ScoreUp = 0;
			for (var y = 0; y < rows.Length; y++)
			{
				for (var x = 0; x < rows.Max(row => row.tiles.Length); x++)
				{
					var tile = GetTile(x, y);

					tile.x = x;
					tile.y = y;

					tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

					tile.button.onClick.AddListener(() => Select(tile));
				}
			}

			if (ensureNoStartingMatches) StartCoroutine(RandCorrecter());

			OnMatch += (type, count) => Debug.Log($"Matched {count}x {type.name}.");
		}
		private async void Select(Tile tile)
		{
			if (Swapped || Match || Shuffling) return;

			if (!_selection.Contains(tile))
			{
				if (_selection.Count > 0)
				{
					if (Math.Abs(tile.x - _selection[0].x) == 1 && Math.Abs(tile.y - _selection[0].y) == 0
						|| Math.Abs(tile.y - _selection[0].y) == 1 && Math.Abs(tile.x - _selection[0].x) == 0)
						_selection.Add(tile);
					
				}
				else
				{
					_selection.Add(tile);
				}
			}

			if (_selection.Count < 2) return;

			await SwapAsync(_selection[0], _selection[1]);

			if (!await TryMatchAsync()) await SwapAsync(_selection[0], _selection[1]);

			var matrix = Matrix;

			while (TileDataMatrixUtility.FindBestMove(matrix) == null || TileDataMatrixUtility.FindBestMatch(matrix) != null)
			{
				Shuffle();

				matrix = Matrix;
			}

			_selection.Clear();
		}
		
		private void Update(TileData[,] tiles)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				var bestMove = TileDataMatrixUtility.FindBestMove(Matrix);

				if (bestMove != null)
				{
					Select(GetTile(bestMove.X1, bestMove.Y1));
					Select(GetTile(bestMove.X2, bestMove.Y2));
				}
			}

         

        }
        private void Update()
        {
           
          
		}

        public void DifEasy()
		{
			Easy = (true);
			Medium = false;
			Hard = false;
			print("Easy")
;		}

		public void DifMedium()
		{
			Easy = (false);
			Medium = true;
			Hard = false;
			print("medium");
		}
		public void DifHard()
		{
			Easy = (false);
			Medium = false;
			Hard = true;
			print("hard");
		}
		private async Task<bool> TryMatchAsync()
		{
			var MatchedTrue = false;

			Match = true;

			var match = TileDataMatrixUtility.FindBestMatch(Matrix);

			while (match != null)
			{
				MatchedTrue = true;

				var tiles = GetTiles(match.Tiles);
				
				var Delate = DOTween.Sequence();

				foreach (var tile in tiles) Delate.Join(tile.icon.transform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBack));

				audioSource.PlayOneShot(matchSound);

				await Delate.Play()
									 .AsyncWaitForCompletion();

				Score.Instance.Scores += ScoreUp + 10;
				if (Score.Instance.Scores >= 50 && Easy == true)
				{
					SceneManager.LoadScene("SceneMain");
					
				}
				if (Score.Instance.Scores >= 100 && Medium == true)
				{
					SceneManager.LoadScene("SceneMain");
				}
				if (Score.Instance.Scores >= 150 && Hard == true)
				{
					SceneManager.LoadScene("SceneMain");
				}

				var Inflate = DOTween.Sequence();

				foreach (var tile in tiles)
				{
					tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

					Inflate.Join(tile.icon.transform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutBack));
					
				}

				await Inflate.Play()
									 .AsyncWaitForCompletion();
				
				OnMatch?.Invoke(Array.Find(tileTypes, tileType => tileType.id == match.TypeId), match.Tiles.Length);

				match = TileDataMatrixUtility.FindBestMatch(Matrix);
			}

			Match = false;

			return MatchedTrue;
		}

		private IEnumerator RandCorrecter()
		{
			var wait = new WaitForEndOfFrame();

			while (TileDataMatrixUtility.FindBestMatch(Matrix) != null)
			{
				Shuffle();

				yield return wait;
			}
		}

		private Tile GetTile(int x, int y) => rows[y].tiles[x];

		private Tile[] GetTiles(IList<TileData> tileData)
		{
			var length = tileData.Count;

			var tiles = new Tile[length];

			for (var i = 0; i < length; i++) tiles[i] = GetTile(tileData[i].X, tileData[i].Y);

			return tiles;
		}

		

		private async Task SwapAsync(Tile tile1, Tile tile2)
		{
			Swapped = true;

			var icon1 = tile1.icon;
			var icon2 = tile2.icon;

			var ic1Tran = icon1.transform;
			var ic2Tran = icon2.transform;

			ic1Tran.SetParent(swappingOverlay);
			ic2Tran.SetParent(swappingOverlay);

			ic1Tran.SetAsLastSibling();
			ic2Tran.SetAsLastSibling();

			var sequence = DOTween.Sequence();

			sequence.Join(ic1Tran.DOMove(ic2Tran.position, tweenDuration).SetEase(Ease.OutBack))
			        .Join(ic2Tran.DOMove(ic1Tran.position, tweenDuration).SetEase(Ease.OutBack));

			await sequence.Play()
			              .AsyncWaitForCompletion();

			ic1Tran.SetParent(tile2.transform);
			ic2Tran.SetParent(tile1.transform);

			tile1.icon = icon2;
			tile2.icon = icon1;

			var tile1Item = tile1.Type;

			tile1.Type = tile2.Type;

			tile2.Type = tile1Item;

			Swapped = false;
		}

		
		private void Shuffle()
		{
			Shuffling = true;

			foreach (var row in rows)
				foreach (var tile in row.tiles)
					tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

			Shuffling = false;
		}
	}
}
