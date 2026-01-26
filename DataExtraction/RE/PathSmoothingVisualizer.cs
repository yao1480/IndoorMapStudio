using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathExtension.Geometry;
using System.Drawing;

namespace RE
{
    /// <summary>
    /// 路径平滑化演示和调试工具
    /// 用于可视化比较原始路径和平滑后路径
    /// </summary>
    public static class PathSmoothingVisualizer
    {
        /// <summary>
        /// 生成用于对比的路径数据
        /// </summary>
        /// <param name="originalPath">原始路径</param>
        /// <param name="smoothedPath">平滑后路径</param>
        /// <returns>对比分析结果</returns>
        public static PathComparisonResult ComparePaths(List<Point> originalPath, List<Point> smoothedPath)
        {
            if (originalPath == null || smoothedPath == null)
                return null;

            PathComparisonResult result = new PathComparisonResult();
            result.OriginalPath = originalPath;
            result.SmoothedPath = smoothedPath;

            // 计算基本统计信息
            result.OriginalPointCount = originalPath.Count;
            result.SmoothedPointCount = smoothedPath.Count;
            result.AdditionalPoints = smoothedPath.Count - originalPath.Count;

            // 计算路径长度
            result.OriginalLength = CalculatePathLength(originalPath);
            result.SmoothedLength = CalculatePathLength(smoothedPath);
            result.LengthDifference = result.SmoothedLength - result.OriginalLength;
            result.LengthIncreasePercent = (result.LengthDifference / result.OriginalLength) * 100;

            // 计算转弯角度
            result.OriginalMaxTurnAngle = CalculateMaxTurnAngle(originalPath);
            result.SmoothedMaxTurnAngle = CalculateMaxTurnAngle(smoothedPath);
            result.AngleImprovement = result.OriginalMaxTurnAngle - result.SmoothedMaxTurnAngle;

            // 计算平滑度评分 (0-100，越高越好)
            result.SmoothnessScore = CalculateSmoothnessScore(smoothedPath);

            return result;
        }

        /// <summary>
        /// 计算路径长度
        /// </summary>
        private static double CalculatePathLength(List<Point> path)
        {
            if (path == null || path.Count < 2)
                return 0;

            double length = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                length += GeometryHelper.Calc_Distance(path[i], path[i + 1]);
            }
            return length;
        }

        /// <summary>
        /// 计算路径最大转弯角度（角度越大，转弯越急）
        /// </summary>
        private static double CalculateMaxTurnAngle(List<Point> path)
        {
            if (path == null || path.Count < 3)
                return 0;

            double maxAngle = 0;
            for (int i = 1; i < path.Count - 1; i++)
            {
                Point prev = path[i - 1];
                Point current = path[i];
                Point next = path[i + 1];

                // 计算向量
                double v1x = prev.X - current.X;
                double v1y = prev.Y - current.Y;
                double v2x = next.X - current.X;
                double v2y = next.Y - current.Y;

                // 计算夹角
                double dotProduct = v1x * v2x + v1y * v2y;
                double len1 = Math.Sqrt(v1x * v1x + v1y * v1y);
                double len2 = Math.Sqrt(v2x * v2x + v2y * v2y);

                if (len1 > 0 && len2 > 0)
                {
                    double cosAngle = dotProduct / (len1 * len2);
                    cosAngle = Math.Max(-1, Math.Min(1, cosAngle)); // 限制在[-1, 1]范围内
                    double angle = Math.Acos(cosAngle) * 180 / Math.PI;
                    maxAngle = Math.Max(maxAngle, angle);
                }
            }
            return maxAngle;
        }

        /// <summary>
        /// 计算平滑度评分 (0-100, 100表示完全平滑)
        /// </summary>
        private static double CalculateSmoothnessScore(List<Point> path)
        {
            if (path == null || path.Count < 3)
                return 100;

            double totalAngleChange = 0;
            for (int i = 1; i < path.Count - 1; i++)
            {
                Point prev = path[i - 1];
                Point current = path[i];
                Point next = path[i + 1];

                double v1x = prev.X - current.X;
                double v1y = prev.Y - current.Y;
                double v2x = next.X - current.X;
                double v2y = next.Y - current.Y;

                double dotProduct = v1x * v2x + v1y * v2y;
                double len1 = Math.Sqrt(v1x * v1x + v1y * v1y);
                double len2 = Math.Sqrt(v2x * v2x + v2y * v2y);

                if (len1 > 0 && len2 > 0)
                {
                    double cosAngle = dotProduct / (len1 * len2);
                    cosAngle = Math.Max(-1, Math.Min(1, cosAngle));
                    double angle = Math.Acos(cosAngle) * 180 / Math.PI;
                    totalAngleChange += angle;
                }
            }

            // 平均角度变化越小，平滑度越高
            double avgAngleChange = totalAngleChange / (path.Count - 2);
            double smoothnessScore = Math.Max(0, 100 - avgAngleChange);
            return smoothnessScore;
        }

        /// <summary>
        /// 生成路径描述信息
        /// </summary>
        public static string GeneratePathReport(PathComparisonResult result)
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("========== 路径平滑化分析报告 ==========");
            report.AppendLine();
            report.AppendLine($"原始路径点数: {result.OriginalPointCount}");
            report.AppendLine($"平滑后路径点数: {result.SmoothedPointCount}");
            report.AppendLine($"增加点数: {result.AdditionalPoints} ({(double)result.AdditionalPoints / result.OriginalPointCount * 100:F1}%)");
            report.AppendLine();
            report.AppendLine($"原始路径长度: {result.OriginalLength:F2} mm");
            report.AppendLine($"平滑后路径长度: {result.SmoothedLength:F2} mm");
            report.AppendLine($"长度增加: {result.LengthDifference:F2} mm ({result.LengthIncreasePercent:F1}%)");
            report.AppendLine();
            report.AppendLine($"原始最大转弯角: {result.OriginalMaxTurnAngle:F1}°");
            report.AppendLine($"平滑后最大转弯角: {result.SmoothedMaxTurnAngle:F1}°");
            report.AppendLine($"角度改善: {result.AngleImprovement:F1}°");
            report.AppendLine();
            report.AppendLine($"平滑度评分: {result.SmoothnessScore:F1}/100");
            report.AppendLine();
            report.AppendLine($"综合评估: {GetEvaluationText(result.SmoothnessScore)}");
            report.AppendLine("========================================");

            return report.ToString();
        }

        private static string GetEvaluationText(double smoothnessScore)
        {
            if (smoothnessScore >= 90)
                return "优秀 - 路径非常平滑，转弯自然";
            else if (smoothnessScore >= 75)
                return "良好 - 路径平滑，行走体验佳";
            else if (smoothnessScore >= 60)
                return "一般 - 路径有所改善，但仍有优化空间";
            else
                return "较差 - 建议调整平滑参数";
        }
    }

    /// <summary>
    /// 路径对比分析结果
    /// </summary>
    public class PathComparisonResult
    {
        public List<Point> OriginalPath { get; set; }
        public List<Point> SmoothedPath { get; set; }

        // 点数统计
        public int OriginalPointCount { get; set; }
        public int SmoothedPointCount { get; set; }
        public int AdditionalPoints { get; set; }

        // 长度统计
        public double OriginalLength { get; set; }
        public double SmoothedLength { get; set; }
        public double LengthDifference { get; set; }
        public double LengthIncreasePercent { get; set; }

        // 转弯角度统计
        public double OriginalMaxTurnAngle { get; set; }
        public double SmoothedMaxTurnAngle { get; set; }
        public double AngleImprovement { get; set; }

        // 平滑度评分
        public double SmoothnessScore { get; set; }
    }

    /// <summary>
    /// 平滑参数优化助手
    /// 可自动寻找最佳平滑参数
    /// </summary>
    public static class SmoothFactorOptimizer
    {
        /// <summary>
        /// 自动优化平滑参数
        /// </summary>
        /// <param name="roomSP">房间语义多边形</param>
        /// <param name="targetSmoothness">目标平滑度 (60-90)</param>
        /// <returns>推荐的平滑参数</returns>
        public static OptimizationResult OptimizeSmoothFactor(SemanticPolygon roomSP, double targetSmoothness = 75)
        {
            // 测试不同的平滑因子
            double bestFactor = 0.3;
            double bestScoreDiff = double.MaxValue;
            PathComparisonResult bestResult = null;

            // 测试范围 0.1 到 0.8，步长 0.1
            for (double factor = 0.1; factor <= 0.8; factor += 0.1)
            {
                // 生成平滑和未平滑路径进行对比
                var smoothPaths = RouteExtractor.Calc_CommonRoomRoutes(ref roomSP, true, factor);
                var rawPaths = RouteExtractor.Calc_CommonRoomRoutes(ref roomSP, false, 0);

                if (smoothPaths != null && smoothPaths.Count > 0 && rawPaths != null && rawPaths.Count > 0)
                {
                    var comparison = PathSmoothingVisualizer.ComparePaths(rawPaths[0], smoothPaths[0]);
                    double scoreDiff = Math.Abs(comparison.SmoothnessScore - targetSmoothness);

                    if (scoreDiff < bestScoreDiff)
                    {
                        bestScoreDiff = scoreDiff;
                        bestFactor = factor;
                        bestResult = comparison;
                    }
                }
            }

            return new OptimizationResult
            {
                RecommendedFactor = bestFactor,
                ExpectedSmoothness = bestResult?.SmoothnessScore ?? 0,
                ComparisonResult = bestResult
            };
        }
    }

    /// <summary>
    /// 参数优化结果
    /// </summary>
    public class OptimizationResult
    {
        public double RecommendedFactor { get; set; }
        public double ExpectedSmoothness { get; set; }
        public PathComparisonResult ComparisonResult { get; set; }
    }
}
