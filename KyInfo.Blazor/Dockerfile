# =====================================================
# KyInfo Blazor Dockerfile
# =====================================================

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

# 构建前检查：缺少文件时立刻报错，便于排查
RUN test -f KyInfo.Blazor/KyInfo.Blazor.csproj || (echo "缺少 KyInfo.Blazor.csproj" && exit 1) && \
    test -f src/KyInfo.Contracts/KyInfo.Contracts.csproj || (echo "缺少 KyInfo.Contracts.csproj" && exit 1) && \
    grep -q "Microsoft.NET.Sdk.Web" KyInfo.Blazor/KyInfo.Blazor.csproj || (echo "csproj 未使用 Sdk.Web，请重新上传" && exit 1)

WORKDIR /src/KyInfo.Blazor
RUN dotnet publish "KyInfo.Blazor.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:NuGetAudit=false \
    /p:RestoreIgnoreFailedSources=true

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "KyInfo.Blazor.dll"]
