# Use Azure SQL Edge which supports ARM64 (M1/M2/M3 chips)
FROM mcr.microsoft.com/azure-sql-edge:latest

ENV ACCEPT_EULA=Y
ENV MSSQL_PID=Developer

# Switch to root to change permissions
USER root
WORKDIR /usr/src/app

COPY ./init-db.sql .
COPY ./entrypoint.sh .

# Ensure the script is executable
RUN chmod +x ./entrypoint.sh

# Switch back to the default user (mssql)
USER mssql

EXPOSE 1433

ENTRYPOINT ["./entrypoint.sh"]