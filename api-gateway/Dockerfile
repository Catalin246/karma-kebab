FROM nginx:latest

# Copy both configuration files
COPY nginx.prod.conf /etc/nginx/nginx.prod.conf

# Default configuration file
ARG CONFIG=prod
RUN cp /etc/nginx/nginx.${CONFIG}.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
