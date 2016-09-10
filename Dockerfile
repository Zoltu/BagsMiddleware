FROM zoltu/aspnetcore

COPY . /app
WORKDIR /app
RUN dotnet restore
RUN dotnet build

EXPOSE 80

ENTRYPOINT ["dotnet", "run"]
