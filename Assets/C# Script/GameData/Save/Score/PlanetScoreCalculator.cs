using System;

public static class PlanetScoreCalculator
{
    public const int PerfectWeight = 100;
    public const int GoodWeight = 70;
    public const int MissWeight = 0;

    public static PlanetRunResult Calculate(
        int planetId,
        int completedMinigameCount,
        int totalNode,
        int perfect,
        int good,
        int miss,
        bool isCleared)
    {
        perfect = Math.Max(0, perfect);
        good = Math.Max(0, good);
        miss = Math.Max(0, miss);

        int judgedTotal =
            perfect + good + miss;

        // 잘못된 집계가 들어와도 분모가 판정 수보다
        // 작아지지 않도록 보정한다.
        int safeTotalNode = Math.Max(
            Math.Max(0, totalNode),
            judgedTotal
        );

        int score = CalculateScore(
            safeTotalNode,
            perfect,
            good
        );

        RunEvaluation evaluation =
            safeTotalNode > 0
                ? CalculateEvaluation(score)
                : RunEvaluation.None;

        return new PlanetRunResult
        {
            planetId = planetId,

            completedMinigameCount =
                Math.Max(0, completedMinigameCount),

            totalNode = safeTotalNode,
            perfect = perfect,
            good = good,
            miss = miss,

            score = score,
            evaluation = evaluation,
            isCleared = isCleared
        };
    }

    public static int CalculateScore(
        int totalNode,
        int perfect,
        int good)
    {
        if (totalNode <= 0)
            return 0;

        double earnedScore =
            perfect * PerfectWeight +
            good * GoodWeight;

        int result = (int)Math.Round(
            earnedScore / totalNode,
            MidpointRounding.AwayFromZero
        );

        return Math.Clamp(result, 0, 100);
    }

    public static RunEvaluation CalculateEvaluation(
        int score)
    {
        score = Math.Clamp(score, 0, 100);

        if (score >= 90)
            return RunEvaluation.S;

        if (score >= 80)
            return RunEvaluation.A;

        if (score >= 70)
            return RunEvaluation.B;

        if (score >= 60)
            return RunEvaluation.C;

        return RunEvaluation.D;
    }
}