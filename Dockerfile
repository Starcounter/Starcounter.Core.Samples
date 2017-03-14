FROM microsoft/dotnet:runtime
MAINTAINER Johan Lindh <johan@linkdata.se>

RUN export DEBIAN_FRONTEND='noninteractive' && \
	apt-get update -q && \
	apt-get install -qy --no-install-recommends \
		libboost-all-dev \
		&& \
	apt-get clean && \
	rm -rf /var/lib/apt/lists/* /tmp/*

CMD dotnet restore && dotnet run
