using Axpo;
using AxpoChallengeZHY.Application.Managers;
using AxpoChallengeZHY.Domain.CustomError;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AxpoChallengeZHY.Application.Test;

public class TradeManagerTest
{
    private readonly Mock<IPowerService> _powerServiceMock;
    private readonly TradeManager _tradeManager;
    private const string iso8601Format = "o";

    public TradeManagerTest()
    {
        var _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"LocalTimeZone", "Central European Standard Time" }
            }).Build();
        _powerServiceMock = new();
        _tradeManager = new(_powerServiceMock.Object, _configuration);
    }

    [Theory]
    [InlineData(10, 24, 1000)]
    [InlineData(5, 25, 500)]
    public async Task GetTradeReportAsync_Should_SumVolumesByHour(int powerTrades, int periods, double volume)
    {
        // Arrange
        var testDate = new DateTime(2024, 11, 09, 13, 0, 0);
        _powerServiceMock.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>())).ReturnsAsync(GeneratePowerTrades(testDate.AddDays(1), powerTrades, periods, volume));

        // Act
        var report = await _tradeManager.GetTradeReportAsync(testDate);

        // Assert
        report.PowerPeriods.First().volumes.Should().BeGreaterThanOrEqualTo(volume * powerTrades);
        report.PowerPeriods.Should().NotBeNull();
        report.PowerPeriods.Should().HaveCount(periods);
        report.PowerPeriods.Should().BeInAscendingOrder(pp => pp.dateTime);

    }

    [Fact]
    public async Task GetTradeReportAsync_Throw_NoTradesException()
    {
        // Arrange
        var testDate = new DateTime(2024, 11, 09, 13, 0, 0);
        _powerServiceMock.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>())).ReturnsAsync([]);

        //Act & Assert
        var exception = await Assert.ThrowsAsync<NoTradesException>(async () =>
            await _tradeManager.GetTradeReportAsync(testDate));
        exception.Message.Should().Be("No data was retrieved from PowerService in TradeManager");
    }

    [Fact]
    public async Task GetTradeReportAsync_Throw_PowerServiceException()
    {
        // Arrange
        var testDate = new DateTime(2024, 11, 09, 13, 0, 0);
        _powerServiceMock.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>())).ThrowsAsync(new PowerServiceException("Error retrieving power volumes"));

        //Act & Assert
        var exception = await Assert.ThrowsAsync<PowerServiceException>(async () =>
            await _tradeManager.GetTradeReportAsync(testDate));
        exception.Message.Should().Be("Error retrieving power volumes");
    }

    [Theory]
    [InlineData("Central European Standard Time")]
    [InlineData("China Standard Time")]
    [InlineData("Tokyo Standard Time")]
    [InlineData("Central America Standard Time")]
    [InlineData("Pacific Standard Time")]
    public async Task GetTradeReportAsync_VariousLocalTimeZone_PowerPeriodsUtc(string localTimeZone)
    {
        // Arrange
        var configurationTest = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"LocalTimeZone", localTimeZone }
            }).Build();

        var testDate = new DateTime(2024, 12, 05);

        _powerServiceMock.Setup(x => x.GetTradesAsync(It.IsAny<DateTime>())).ReturnsAsync(GeneratePowerTrades(testDate, 5, 25, 500));
        
        TradeManager tradeManagerTest = new(_powerServiceMock.Object, configurationTest);

        // Act
        var report = await tradeManagerTest.GetTradeReportAsync(testDate);

        // Assert
        // Assert that the dateTime is in UTC and in ISO 8601 format
        var assertTestDate = GetLocalDateTime(testDate, localTimeZone);
        report.PowerPeriods.First().dateTime.Should().Be(assertTestDate.ToString(iso8601Format));
    }

    private static List<PowerTrade> GeneratePowerTrades(DateTime dateTime, int powerTrades, int periods, double volume)
    {
        var powerTradesTest = new List<PowerTrade>();
        for (int i = 0; i < powerTrades; i++)
        {
            var powerTrade = PowerTrade.Create(dateTime, periods);

            for (int j = 0; j < periods; j++)
            {
                powerTrade.Periods[j].SetVolume(volume);
            }

            powerTradesTest.Add(powerTrade);
        }

        return powerTradesTest;
    }

    // Not the best way to test this method, but it's a private method and it's not worth to make it public
    private DateTime GetLocalDateTime(DateTime powerPeriodDate, string timeZoneId)
    {
        // ToUniversalTime() already takes care of Daylight saving time 
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        // If the DateTime is not explicitly set as Unspecified, convert it to Unspecified
        if (powerPeriodDate.Kind != DateTimeKind.Unspecified)
        {
            powerPeriodDate = DateTime.SpecifyKind(powerPeriodDate, DateTimeKind.Unspecified);
        }

        var localDateTime = TimeZoneInfo.ConvertTimeToUtc(powerPeriodDate, timeZoneInfo);

        return localDateTime;
    }
}

