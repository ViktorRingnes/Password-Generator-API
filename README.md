# Password Generator API

## Overview
The Password Generator API is a .NET Core application designed to create secure and random passwords. Developed and thoroughly tested on a Debian 12 virtual machine, this project showcases advanced .NET development skills and proficiency in Linux system administration.

## Key Features
- Generates random passwords with a blend of characters (lowercase, uppercase, numeric, and special).
- MVC architecture with a focus on the Controller for handling HTTP requests.
- Scalable and easy to integrate into various systems.

## Environment
- **Development & Testing**: Debian 12 VM
- **Framework**: .NET Core

## Setup and Configuration
To deploy the Password Generator API on a Debian system, follow these steps:

### Service Setup
To ensure that the Password Generator API runs continuously as a service on your Debian system, you'll need to create a systemd service file and configure it to start on boot. Here are the steps to set it up:

1. Create a new service file in `/etc/systemd/system/` named `passwordgeneratorapi.service` using your favorite text editor. You can use `nano` or `vi` for this purpose:

    ```bash
    sudo nano /etc/systemd/system/passwordgeneratorapi.service
    ```

2. Add the following content to the `passwordgeneratorapi.service` file:

    ```ini
    [Unit]
    Description=Password Generator API Service
    After=network.target

    [Service]
    User=www-data
    Group=www-data
    ExecStart=/usr/bin/dotnet /var/www/PasswordGeneratorAPI/publish/PasswordGeneratorAPI.dll
    Restart=always
    RestartSec=10
    KillSignal=SIGINT
    Environment="ASPNETCORE_ENVIRONMENT=Production"
    Environment="DOTNET_PRINT_TELEMETRY_MESSAGE=false"

    [Install]
    WantedBy=multi-user.target
    ```

    Replace `/var/www/PasswordGeneratorAPI/publish/PasswordGeneratorAPI.dll` with the actual path to your published DLL file.

3. Save the file and exit the text editor.

4. Reload the systemd manager configuration to read the newly created service file:

    ```bash
    sudo systemctl daemon-reload
    ```

5. Enable the service to start on boot:

    ```bash
    sudo systemctl enable passwordgeneratorapi.service
    ```

6. Start the service:

    ```bash
    sudo systemctl start passwordgeneratorapi.service
    ```

7. Check the status of your service to ensure it's active and running:

    ```bash
    sudo systemctl status passwordgeneratorapi.service
    ```

By following these steps, your Password Generator API will be configured to run as a background service on your Debian system.


### Apache2 Reverse Proxy Configuration
Set up a reverse proxy with Apache2 to route requests to the API. Add the following configuration to your Apache2 sites-available:

```bash
# Reverse Proxy Settings for /api/ endpoint
ProxyPreserveHost On
ProxyPass /api/ http://127.0.0.1:5000/api/
ProxyPassReverse /api/ http://127.0.0.1:5000/api/

RequestHeader set X-Forwarded-Proto expr=%{REQUEST_SCHEME}
RequestHeader set X-Forwarded-For expr=%{REMOTE_ADDR}
```

### Alias for Easy Deployment
For convenient updates and deployments, set up an alias in your bash profile:
 Run the command: 
```bash
sudo nano ~/.bashrc
   ```
Add the following alias anywhere that doesn't interfere with existing code.
```bash
alias pushapi='cd /var/www/html/PasswordGeneratorAPI && \
               dotnet build && \
               sudo dotnet publish -c Release -o /var/www/PasswordGeneratorAPI/publish && \
               sudo systemctl stop passwordgeneratorapi.service && \
               sudo systemctl start passwordgeneratorapi.service'
