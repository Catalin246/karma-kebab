FROM nginx:latest

# Copy both configuration files
COPY nginx.conf /etc/nginx/nginx.conf

# Default configuration file
ARG CONFIG=local
RUN cp /etc/nginx/nginx.conf /etc/nginx/nginx.conf

EXPOSE 5000
CMD ["nginx", "-g", "daemon off;"]
