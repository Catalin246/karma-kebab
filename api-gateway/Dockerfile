FROM nginx:latest

# Copy both configuration files
COPY nginx.local.conf /etc/nginx/nginx.local.conf

# Default configuration file
ARG CONFIG=local
RUN cp /etc/nginx/nginx.${CONFIG}.conf /etc/nginx/nginx.conf

EXPOSE 5000
CMD ["nginx", "-g", "daemon off;"]
