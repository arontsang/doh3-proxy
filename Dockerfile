

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["doh-client.csproj", "./"]
RUN dotnet restore "doh-client.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "doh-client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
ARG BUILDARCH
COPY --chmod=744 /install-gcc.sh /tmp/install-gcc.sh
RUN /tmp/install-gcc.sh

RUN dotnet publish "doh-client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-noble-chiseled AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ["./doh-client"]
