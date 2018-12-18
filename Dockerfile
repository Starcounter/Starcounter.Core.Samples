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

# Install Bluestar binaries
WORKDIR /opt/starcounter/bluestar
RUN apt-get update && apt-get install -y \
    && --no-install-recommends apt-utils \
    && unzip \ 
    && swi-prolog-nox \
    && libaio1

# Can this be simplified into one line? Should be possible to pipe
RUN curl -L -o bluestar.zip ${BLUESTAR_PACKAGE}
RUN unzip bluestar.zip

# Here we should move the result into the correct directories:
# - executables to /usr/local/bin
# - shared libraries to /usr/local/lib
# - everything else to /opt/starcounter

# This shouldn't be necessary if we put the stuff in the right places
ENV PATH ${PATH}:/opt/starcounter/bluestar/runtimes/linux-x64/native
ENV LD_LIBRARY_PATH /opt/starcounter/bluestar/runtimes/linux-x64/native

WORKDIR /app

# We should use the runtime image microsoft/dotnet:2.1-runtime
# and copy only the things we need such as the /app/out directory
# and the bluestar binaries

ENTRYPOINT [ "dotnet", "out/Starcounter.Nova.Samples.dll" ]