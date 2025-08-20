using System;
using System.Collections.Generic;

namespace ElasoftCommunityManagementSystem.Dtos.StatisticsDtos
{
    public class SummaryStatisticsDto
    {
        public int TotalMembers { get; set; }
        public int TotalClubs { get; set; }
        public int TotalEvents { get; set; }
        public decimal TotalExpenses { get; set; }
        public GrowthRatesDto Growth { get; set; }
    }

    public class GrowthRatesDto
    {
        public double Members { get; set; }
        public double Clubs { get; set; }
        public double Events { get; set; }
        public double Expenses { get; set; }
    }

    public class ClubStatisticsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MemberCount { get; set; }
        public int EventCount { get; set; }
        public double ParticipationRate { get; set; }
    }

    public class MonthlyEventStatisticsDto
    {
        public string Name { get; set; }
        public int EventCount { get; set; }
        public int TotalParticipants { get; set; }
        public int AverageParticipation { get; set; }
    }

    public class EventStatisticsResponseDto
    {
        public List<MonthlyEventStatisticsDto> MonthlyStats { get; set; }
        public List<int> AvailableYears { get; set; }
    }

    public class ExpenseStatisticsDto
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public decimal CashSupport { get; set; }
        public decimal InKindSupport { get; set; }
        public string DokumanUrl { get; set; }
    }

    public class ExpenseStatisticsResponseDto
    {
        public List<ExpenseStatisticsDto> Expenses { get; set; }
        public decimal TotalCashSupport { get; set; }
        public decimal TotalInKindSupport { get; set; }
        public List<ClubBasicInfoDto> Clubs { get; set; }
    }

    public class ClubBasicInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
} 