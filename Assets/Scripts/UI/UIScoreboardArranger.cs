using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIScoreboardArranger : MonoBehaviour
{
    [SerializeField] private float _spacingStride = 50.0f;

    private UIScoreBar[] _scorebars;

    void Start()
    {
        _scorebars = GetComponentsInChildren<UIScoreBar>();

        foreach (var scorebar in _scorebars)
        {
            scorebar.SpacingStride = _spacingStride;
        }
    }

    void Update()
    {
        var rankedBars = GetSortedBars();

        for (int ranking = 0; ranking < rankedBars.Count; ranking++)
        {
            foreach(var scorebar in _scorebars)
            {
                if (scorebar == rankedBars[ranking])
                    scorebar.ScoreRanking = ranking;
            }
        }
    }

    private List<UIScoreBar> GetSortedBars()
    {
        var unsortedList = _scorebars.Where(bar => !bar.IsDisabled).ToList();
        var sortedList = new List<UIScoreBar>();

        while(unsortedList.Any())
        {
            var highestScore = -1f;
            var highestScoreIndex = 0;

            for (var i = 0; i < unsortedList.Count; i++)
            {
                var bar = unsortedList[i];

                if (bar.Score > highestScore)
                {
                    highestScore = bar.Score;
                    highestScoreIndex = i;
                } 
            }

            sortedList.Add(unsortedList[highestScoreIndex]);
            unsortedList.RemoveAt(highestScoreIndex);
        }

        return sortedList;
    }
}
