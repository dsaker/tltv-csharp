FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
EXPOSE 8080
WORKDIR /src
COPY . .
RUN dotnet publish "TalkLikeTv.Mvc/TalkLikeTv.Mvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM build AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TalkLikeTv.Mvc.dll"]