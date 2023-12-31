## Installation and Setup

### Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) installed on your machine.
- API keys for `INFURA_API_KEY` and `ALCHEMY_API_KEY` (if applicable).

### Steps

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/hrantt/APIReverSeProxy.git
   ```

2. **Set Environment Variables:**
   - Set the environment variables for your API keys:
     ```bash
     export INFURA_API_KEY=your_infura_api_key_here
     export ALCHEMY_API_KEY=your_alchemy_api_key_here
     ```
     Replace `your_infura_api_key_here` and `your_alchemy_api_key_here` with your actual API keys.

3. **Navigate to Project Directory:**
   ```bash
   cd APIReverseProxy/APIReverseProxy
   ```

4. **Build the Project:**
   ```bash
   dotnet build APIReverseProxy.csproj
   ```

5. **Run the Application:**
   ```bash
   dotnet run
   ```

6. **Sending POST Requests:**
   - Once the application is running, you can send POST requests to the endpoint "/v1" using tools like `curl`, Postman, or any HTTP client, including libraries in various programming languages.
   - Ensure the requests sent contain valid JSON-RPC content to trigger the proxy functionality.

### Example Usage:

Sending a POST request using `curl`:
```bash
curl http://3.72.8.51:5000/v1/   -X POST   -H "Content-Type: application/json"   -d '{"jsonrpc":"2.0","method":"eth_gasPrice","params": [],"id":1}'
```

By default when you deploy it will use `http://localhost:5000`

