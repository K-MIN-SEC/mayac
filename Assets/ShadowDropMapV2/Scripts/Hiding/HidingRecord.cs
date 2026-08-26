using UnityEngine;
using System.Collections.Generic;

// 플레이어가 어디에 숨었는지, 그게 정답이었는지 기록해두는 저장소
// 다른 스크립트에서 HidingRecord.History 로 언제든 조회 가능 (예: 조사 페이즈에서 "이전 행동 기록 회수"용)
public static class HidingRecord
{
    public struct Entry
    {
        public string spotName;
        public bool isCorrect;
    }

    public static List<Entry> History = new List<Entry>();

    public static void Add(string spotName, bool isCorrect)
    {
        History.Add(new Entry { spotName = spotName, isCorrect = isCorrect });
    }

    // 지금까지 정답 지점에 숨은 적이 몇 번인지
    public static int CorrectCount()
    {
        int count = 0;
        foreach (var entry in History)
        {
            if (entry.isCorrect) count++;
        }
        return count;
    }

    public static void ClearAll() // 게임 재시작 시 사용
    {
        History.Clear();
    }
}
