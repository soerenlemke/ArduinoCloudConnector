# Arduino Cloud Connector Library

This library provides a .NET interface for connecting to the Arduino IoT Cloud, allowing you to interact with your Things and their properties.

## Features

- Fetch and display properties of a Thing from the Arduino IoT Cloud.
- Handle authentication and access token retrieval automatically.
- Error handling and retry mechanism for robust API communication.

## Requirements

- .NET 6.0 or later
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)
- [DotNetEnv](https://www.nuget.org/packages/DotNetEnv/)

## Installation

1. Clone the repository:

    ```sh
    git clone https://github.com/yourusername/arduino-cloud-connector.git
    ```

2. Navigate to the project directory:

    ```sh
    cd arduino-cloud-connector
    ```

3. Install the required NuGet packages:

    ```sh
    dotnet add package Newtonsoft.Json
    dotnet add package DotNetEnv
    ```

## Usage

### Library

To use the library, include it in your project and reference the `ArduinoCloudClient` class. Below is an example of how to use the library in a console application.

### Example Console Application

The `ArduinoCloudConnector.Console` project demonstrates how to use the library to fetch and display properties of a Thing.

#### Configuration

Create a `.env` file in the `ArduinoCloudConnector.Console` project directory with the following content:
```sh
CLIENT_ID=your_client_id
CLIENT_SECRET=your_client_secret
THING_ID=your_thing_id
```
Replace `your_client_id`, `your_client_secret`, and `your_thing_id` with your actual Arduino IoT Cloud credentials and Thing ID.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or new features.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
