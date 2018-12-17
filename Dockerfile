FROM ubuntu:16.04
LABEL maintainer="johan@linkdata.se"

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1
ENV NUGET_XMLDOC_MODE skip
RUN export DEBIAN_FRONTEND='noninteractive' && \
	apt-get update -q && \
	apt-get install -qy \
		apt-utils \
		apt-transport-https \
		libboost-all-dev \
		swi-prolog-nox \
		libaio1 \
		libstdc++6 \
        && \
	apt-get install -qy software-properties-common && \
	add-apt-repository ppa:ubuntu-toolchain-r/test && \
	apt-get update -q && \
	apt-get install gcc-4.9 -qy && \
	apt-get upgrade libstdc++6 -qy && \		
	echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ xenial main" > /etc/apt/sources.list.d/dotnetdev.list && \
	apt-key adv --keyserver apt-mo.trafficmanager.net --recv-keys 417A0893 && \
	apt-get update -q && \
	apt-get install -qy dotnet-dev-1.0.4 dotnet-sdk-2.0.0 && \
	mkdir /Starcounter.Nova.Samples && \
	mkdir dotnet-warmup && \
	cd dotnet-warmup && \
	dotnet new xunit && \
	cd .. && \
	rm -rf dotnet-warmup && \
	apt-get clean && \

RUN curl -L -o binaries.zip https://www.myget.org/F/starcounter/api/v2/package/runtime.linux-x64.runtime.native.Starcounter.Bluestar2/2.0.2 && \
	unzip binaries.zip

ENV PATH $PATH:/runtimes/linux-x64/native
ENV LD_LIBRARY_PATH /runtimes/linux-x64/native

COPY Program.cs /Starcounter.Nova.Samples
COPY Starcounter.Nova.Samples.csproj /Starcounter.Nova.Samples
COPY NuGet.Config /Starcounter.Nova.Samples

CMD cd /Starcounter.Nova.Samples && \
	dotnet restore && \
	dotnet run
