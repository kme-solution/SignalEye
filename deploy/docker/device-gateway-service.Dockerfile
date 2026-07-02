FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish services/device-gateway-service/device-gateway-service.csproj \
    --configuration Release \
    --output /app/publish \
    --no-self-contained

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

RUN mkdir -p /var/lib/signaleye/logs /app/config/modbus \
    && chown -R app:app /var/lib/signaleye /app/config

COPY --from=build /app/publish .
COPY --chown=app:app config/modbus/edge-EN.csv /app/config/modbus/edge-EN.csv

USER app
ENV DOTNET_ENVIRONMENT=Production \
    TelemetryLogging__Directory=/var/lib/signaleye/logs \
    Gateway__Modbus__MappingPath=/app/config/modbus/edge-EN.csv

ENTRYPOINT ["dotnet", "device-gateway-service.dll"]
