using Axpo;
using AxpoChallengeZHY.Application.Managers;
using AxpoChallengeZHY.Domain;
using AxpoChallengeZHY.Domain.CustomError;
using AxpoChallengeZHY.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Polly;
using Polly.Registry;

namespace AxpoChallengeZHY.Application.Test;

public class ReportManagerTest
{
    private readonly Mock<ITradeManager> _tradeManagerMock;
    private IConfiguration _configurationMock;
    private readonly Mock<IReportRepository> _reportRepository;
    private readonly ReportManager _reportManager;
    private readonly Mock<ResiliencePipelineProvider<string>> _pipelineProviderMock;

    public ReportManagerTest()
    {
        _tradeManagerMock = new();
        _reportRepository = new();
        _configurationMock = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"CsvHelperConfig:PublishPath", "testPath" }
            }).Build();

        _pipelineProviderMock = new();
        _pipelineProviderMock.Setup(p => p.GetPipeline("retryPipeline")).Returns(ResiliencePipeline.Empty);

        _reportManager = new(
            _tradeManagerMock.Object,
            _configurationMock,
            _reportRepository.Object, NullLogger<ReportManager>.Instance,
            _pipelineProviderMock.Object);
    }

    [Fact]
    public async Task GenerateReportCsv_Verify_CallTradeManagerAndReportRepositoryOnce()
    {
        // Arrange
        _tradeManagerMock.Setup(x => x.GetTradeReportAsync(It.IsAny<DateTime>())).ReturnsAsync(GenerateReportDto());

        _reportRepository.Setup(x => x.SaveReportCsvAsync(It.IsAny<ReportDto>(), It.IsAny<string>()));
        // Act

        await _reportManager.GenerateReportCsvAsync(new DateTime(2024, 11, 27), Guid.NewGuid().ToString());

        // Assert
        _tradeManagerMock.Verify(x => x.GetTradeReportAsync(It.IsAny<DateTime>()), Times.Once);
        _reportRepository.Verify(x => x.SaveReportCsvAsync(It.IsAny<ReportDto>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GenerateReportCsv_Throw_PowerServiceException()
    {
        // Arrange
        _tradeManagerMock.Setup(x => x.GetTradeReportAsync(It.IsAny<DateTime>())).ThrowsAsync(new PowerServiceException("Error retrieving power volumes"));
        _pipelineProviderMock.Setup(p => p.GetPipeline("retryPipeline")).Returns(ResiliencePipeline.Empty);

        //Act & Assert
        await _reportManager.Invoking(async x =>
            await x.GenerateReportCsvAsync(new DateTime(2024, 11, 27), Guid.NewGuid().ToString()))
        .Should()
        .ThrowAsync<PowerServiceException>()
        .WithMessage("Error retrieving power volumes");

    }

    [Fact]
    public async Task GenerateReportCsv_Throw_NoTradesError()
    {
        // Arrange
        _tradeManagerMock.Setup(x => x.GetTradeReportAsync(It.IsAny<DateTime>())).ThrowsAsync(new NoTradesException("No data was retrieved from PowerService in TradeManager"));
        _pipelineProviderMock.Setup(p => p.GetPipeline("retryPipeline")).Returns(ResiliencePipeline.Empty);

        //Act & Assert
        await _reportManager.Invoking(async x =>
            await x.GenerateReportCsvAsync(new DateTime(2024, 11, 27), Guid.NewGuid().ToString()))
        .Should()
        .ThrowAsync<NoTradesException>()
        .WithMessage("No data was retrieved from PowerService in TradeManager");
    }

    [Fact]
    public void GenerateReportCsv_Throw_ArgumentNullException()
    {
        // Arrange
        var configuationTest = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"CsvHelperConfig:PublishPath", null}
            }).Build(); ;

        //Act
        Action act = () => new ReportManager(_tradeManagerMock.Object,
            configuationTest,
            _reportRepository.Object, NullLogger<ReportManager>.Instance,
            _pipelineProviderMock.Object);

        //Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Null configuration section (Parameter 'ReportManager')");

    }

    private static ReportDto GenerateReportDto()
    {
        var date = new DateTime(2024, 11, 27);
        const int hours = 24;

        var powerPeriodTest = new List<(DateTime dateTime, double volumes)>();
        for (int i = 0; i < hours; i++)
        {
            double testVolume = 100 * i + 100;
            powerPeriodTest.Add((date.ToUniversalTime(), testVolume));
            date = date.AddHours(1);
        }

        return new() { PowerPeriods = powerPeriodTest };
    }
}
