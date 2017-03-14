FROM ubuntu:16.04
MAINTAINER Johan Lindh <johan@linkdata.se>

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1
ENV NUGET_XMLDOC_MODE skip
RUN export DEBIAN_FRONTEND='noninteractive' && \
	apt-get update -q && \
	apt-get install -qy \
		apt-utils \
		apt-transport-https \
        libboost-system-dev \
        && \
	echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ xenial main" > /etc/apt/sources.list.d/dotnetdev.list && \
	apt-key adv --keyserver apt-mo.trafficmanager.net --recv-keys 417A0893 && \
	apt-get update -q && \
	apt-get install -qy dotnet-dev-1.0.0-rc4-004771 && \
    mkdir /starcounter.core.samples && \
	mkdir dotnet-warmup && \
	cd dotnet-warmup && \
	dotnet new xunit && \
	cd .. && \
	rm -rf dotnet-warmup && \
	apt-get clean && \
	rm -rf /var/lib/apt/lists/* /tmp/*

COPY Program.cs /starcounter.core.samples
COPY Starcounter.Core.Samples.csproj /starcounter.core.samples
COPY NuGet.Config /starcounter.core.samples

CMD cd /starcounter.core.samples && \
    dotnet restore && \
    dotnet run
