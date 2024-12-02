# Use the official Keycloak image as the base
FROM quay.io/keycloak/keycloak:latest

# Set environment variables for admin user
ENV KEYCLOAK_ADMIN=admin
ENV KEYCLOAK_ADMIN_PASSWORD=admin

# Copy realm configuration into the container
COPY /realm-config/realm-config.json /opt/keycloak/data/import/realm-config.json

# Set environment variable to import realm configuration
ENV KEYCLOAK_IMPORT=/opt/keycloak/data/import/realm-config.json

# Expose the default Keycloak port
EXPOSE 8080

# Run Keycloak in development mode
ENTRYPOINT ["/opt/keycloak/bin/kc.sh", "start-dev", "--import-realm"]
