FROM nginx:latest

# Copy both configuration files
COPY nginx.local.conf /etc/nginx/nginx.local.conf
COPY nginx.prod.conf /etc/nginx/nginx.conf

# Default configuration file
ARG CONFIG=prod
RUN cp /etc/nginx/nginx.${CONFIG}.conf /etc/nginx/nginx.conf
RUN cp /etc/nginx/nginx.${CONFIG}.conf /opt/app-root/etc/nginx.d/nginx.prod.conf

EXPOSE 5000
CMD ["nginx", "-g", "daemon off;"]
