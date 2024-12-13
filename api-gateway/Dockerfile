FROM nginx:latest

# Copy both configuration files
COPY nginx.conf /etc/nginx/nginx.conf

# Default configuration file
RUN cp /etc/nginx/nginx.conf /etc/nginx/nginx.conf

EXPOSE 3007
CMD ["nginx", "-g", "daemon off;"]
