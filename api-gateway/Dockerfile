FROM nginx:latest

# Copy both configuration files
COPY nginx.conf /etc/nginx/nginx.conf
COPY nginx.local.conf /etc/nginx/nginx.local.conf

# Set an environment variable for local development (default to production). Chnage this var to nginx.local.conf for local development.
 ENV NGINX_CONFIG=nginx.conf 

# Choose which config to use based on the environment variable
RUN cp /etc/nginx/$NGINX_CONFIG /etc/nginx/nginx.conf

EXPOSE 3007

CMD ["nginx", "-g", "daemon off;"]
