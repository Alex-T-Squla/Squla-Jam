using System.Collections.Generic;
using UnityEngine;

public class RankingHierarchy : MonoBehaviour
{

	[SerializeField] private Transform container;
	
	private int maxRank;
	private List<RankedTransform> rankedList = new List<RankedTransform>();

	private class RankedTransform
	{
		public int rank;
		public Transform transform;
	}

	public void SetRank(Transform child, int rank)
	{
		var index = rankedList.FindIndex((rt => rt.transform == child));

		RankedTransform rankedTransform;
		if (index == -1) {
			rankedTransform = new RankedTransform() {
				rank = rank,
				transform = child
			};
		} else {
			rankedTransform = rankedList[index];
			// If it's the same rank we ignore it
			if (rank == rankedTransform.rank) {
				return;
			}
			rankedList.RemoveAt(index);
			rankedTransform.rank = rank;
			
		}

		int newIndex = -1;
		for (int i = 0; i < rankedList.Count; i++) {
			if (rankedList[i].rank >= rank) {
				newIndex = i;
				rankedList.Insert(newIndex, rankedTransform);
				break;
			}
		}

		if (newIndex == -1) {
			rankedList.Add(rankedTransform);
			child.SetAsLastSibling();
		} else {
			child.SetSiblingIndex(newIndex);
		}
	}

}
