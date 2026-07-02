FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish services/mqtt-protocol-service/mqtt-protocol-service.csproj \
    --configuration Release \
    --output /app/publish \
    --no-self-contained

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

RUN mkdir -p /var/lib/signaleye/logs \
    && chown -R app:app /var/lib/signaleye

COPY --from=build /app/publish .

USER app
ENV DOTNET_ENVIRONMENT=Production \
    TelemetryLogging__Directory=/var/lib/signaleye/logs

EXPOSE 1883
ENTRYPOINT ["dotnet", "mqtt-protocol-service.dll"]
