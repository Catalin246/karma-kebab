from flask import Flask, request, abort

app = Flask(__name__)

class GatewayHeaderMiddleware:
    """
    GatewayHeaderMiddleware is a middleware that validates the X-From-Gateway header.

    Attributes:
        app (Flask): The Flask application instance.

    Methods:
        __init__(self, app):
            Initializes the middleware with the given app instance.
        
        __call__(self, environ, start_response):
            Processes the request to validate the X-From-Gateway header and passes it to the next middleware or view.
    """
    def __init__(self, app):
        self.app = app

    """def __call__(self, environ, start_response):
        request = environ['werkzeug.request']
        if request.headers.get('X-From-Gateway') != 'true':
            abort(403, description="Forbidden: Invalid Gateway Header")
        return self.app(environ, start_response)"""
    
    def __call__(self, environ, start_response):
        # Use the environ to manually extract the header
        from werkzeug.wrappers import Request
        request = Request(environ)

        # Validate the X-From-Gateway header
        if request.headers.get('X-From-Gateway') != 'true':
            return abort(403, description="Forbidden: Invalid Gateway Header")

        # Pass control to the next middleware or view
        return self.app(environ, start_response)


app.wsgi_app = GatewayHeaderMiddleware(app.wsgi_app)

@app.route('/trucks')
def trucks():
    return "Trucks endpoint"

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=3006)