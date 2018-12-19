FROM microsoft/dotnet:2.1-sdk

ENV BLUESTAR_PACKAGE https://www.myget.org/F/starcounter/api/v2/package/runtime.linux-x64.runtime.native.Starcounter.Bluestar2/2.0.2

WORKDIR /app

# Copy csproj, NuGet.Config, and restore as distinct layers
COPY *.csproj ./
COPY NuGet.Config ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Install packages required to install and use bluestar binaries
RUN apt-get -qq update && apt-get -qq install -y \
    unzip \ 
    swi-prolog-nox \
    libaio1

# Install Bluestar binaries and make them executable
RUN curl -L -o /opt/bluestar.zip ${BLUESTAR_PACKAGE}
RUN unzip -j /opt/bluestar.zip 'runtimes/linux-x64/native/*' -d /opt/starcounter \ 
    && rm /opt/bluestar.zip \
    && chmod 700 /opt/starcounter/*

# Make it possible for Nova to find Bluestar binaries
ENV PATH ${PATH}:/opt/starcounter
ENV LD_LIBRARY_PATH /opt/starcounter

ENTRYPOINT [ "dotnet", "out/Starcounter.Nova.Samples.dll" ]