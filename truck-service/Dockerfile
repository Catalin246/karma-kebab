FROM python:3.11 AS build
RUN apt-get update && apt-get install -y \
    libpq-dev \
    gcc \
    g++ \
    make \
    python3-dev

COPY requirements.txt /
RUN pip install --target /home/site/wwwroot -r /requirements.txt

FROM mcr.microsoft.com/azure-functions/python:4-python3.11

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    WEBSITES_INCLUDE_CLOUD_CERTS=true


    RUN apt-get update && apt-get install -y \
    libpq-dev \
    gcc \
    g++ \
    make \
    python3-dev

COPY --from=build /home/site/wwwroot /home/site/wwwroot
COPY . /home/site/wwwroot

RUN pip install -r /home/site/wwwroot/requirements.txt