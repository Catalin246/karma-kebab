FROM nginx:latest

# Copy both configuration files
COPY nginx.local.conf /etc/nginx/nginx.local.conf
COPY nginx.prod.conf /etc/nginx/nginx.prod.conf

# Default configuration file
ARG CONFIG=local
RUN cp /etc/nginx/nginx.${CONFIG}.conf /etc/nginx/nginx.conf

EXPOSE 3007
CMD ["nginx", "-g", "daemon off;"]
