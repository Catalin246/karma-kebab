FROM nginx:latest

# Copy both configuration files
COPY nginx.local.conf /etc/nginx/nginx.local.conf
COPY nginx.conf /etc/nginx/nginx.conf

# Default configuration file
ARG CONFIG=local
RUN cp /etc/nginx/nginx.${CONFIG}.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
