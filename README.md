# AxpoChallengeZHYRepo
This is a report generation system designed to generate CSV reports based on specified time intervals. It includes features for resilience, event scheduling, logging, and testing, using widely adopted libraries and frameworks.

## Features
* CSV Report Generation: Generates CSV files using the CsvHelper library, which provides all necessary tools to structure the report data in the desired format.
* Event Scheduling: Uses the `Coravel` library for easy and readable event scheduling. Reports are generated based on configurable time intervals.
* Resilience: The system uses `Microsoft.Extensions.Resilience`, based on `Polly`, to handle retry logic and ensure system resilience in case of controlled failures.
* Logging: Integrated with `Serilog` for logging, configured to output logs both to a file and to the console when running in DEBUG mode.
* Testing: The application is tested using `XUnit`, `FluentAssertions`, and `Moq` to ensure reliability and proper functioning of the system.
  
## Libraries Used
* `CsvHelper`: A library for handling CSV file generation.
* `Coravel`: A simple and readable event scheduler.
* `Microsoft.Extensions.Resilience`: Provides resilience strategies for error handling.
* `Polly`: A resilience and transient-fault-handling library used by Microsoft.Extensions.Resilience.
* `Serilog`: A logging framework for structured logs.
* `XUnit`: A testing framework for .NET.
* `FluentAssertions`: Extends XUnit assertions for more readable and expressive tests.
* `Moq`: A mocking framework for creating test doubles.

## Configuration

### Event Scheduling
The frequency of report generation can be adjusted by setting the _CoravelPipelineConfig:MinuteInterval_ variable in appsettings.{ENVIRONMENT}.json. This defines the interval, in minutes, for generating the reports.

### Logging
The logging system is configured to log both to a file and to the console in DEBUG mode. This setup uses the `Serilog` library, which ensures logs are stored and displayed with the necessary detail.

### Resilience
The retry logic is handled by `Microsoft.Extensions.Resilience`, which is based on `Polly`. It is configured by default to retry up to 5 times when encountering a _PowerServiceException_ or _NoTradesException_. This ensures that temporary failures in these areas are handled gracefully.

### CSV File Path
The path where the CSV report is published can be customized by setting the _CsvHelperConfig:PublishPath_ variable in the _appsettings.json_ file. This allows you to specify a custom directory for saving the generated CSV reports.

## Annotations
* Logging Context with BeginScope: As mentioned above, the BeginScope method in Serilog can add context to logs, reducing redundancy. It allows you to structure logs with relevant information that should be included in every log entry for a particular class or context. However, for this feature to be fully effective, a log management system such as Kibana is recommended to observe and visualize the logs with proper context.

* Alternative Scheduling System (Hangfire + Mongo): Although Coravel is used in this implementation due to the simplicity and limited data size of the system, an alternative scheduling approach could have been implemented with Hangfire and MongoDB. This would have been a good option for larger systems with more complex scheduling needs. Hangfire also offers an out-of-the-box monitor for web applications, providing built-in support for scheduling, retries, and monitoring of background jobs.

## How to Use
1. Clone this repository to your local machine.
2. Install the necessary NuGet packages listed in the csproj file.
3. Configure the CoravelPipelineConfig:MinuteInterval variable in your Program.cs to adjust the report generation frequency.
4. Run the application, and reports will be generated according to the specified schedule in the folder specified, by default ..\PowerPositions\AxpoChallenge_{ENVIRONMENT}.
5. Logs will be generated according to the configuration, and can be found in the designated log file and console output in DEBUG mode.
