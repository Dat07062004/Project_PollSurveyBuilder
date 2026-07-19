# Multi-stage Docker build for PollSurveyBuilder Web API & Application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for caching restore layers
COPY ["PollSurveyBuilder.slnx", "./"]
COPY ["PollSurveyBuilder.Domain/PollSurveyBuilder.Domain.csproj", "PollSurveyBuilder.Domain/"]
COPY ["PollSurveyBuilder.Common/PollSurveyBuilder.Common.csproj", "PollSurveyBuilder.Common/"]
COPY ["PollSurveyBuilder.Application/PollSurveyBuilder.Application.csproj", "PollSurveyBuilder.Application/"]
COPY ["PollSurveyBuilder.Infrastructure/PollSurveyBuilder.Infrastructure.csproj", "PollSurveyBuilder.Infrastructure/"]
COPY ["PollSurveyBuilder.API/PollSurveyBuilder.API.csproj", "PollSurveyBuilder.API/"]
COPY ["PollSurveyBuilder.Web/PollSurveyBuilder.Web.csproj", "PollSurveyBuilder.Web/"]

RUN dotnet restore "PollSurveyBuilder.API/PollSurveyBuilder.API.csproj"

# Copy remaining source code and publish
COPY . .
WORKDIR "/src/PollSurveyBuilder.API"
RUN dotnet publish "PollSurveyBuilder.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "PollSurveyBuilder.API.dll"]
