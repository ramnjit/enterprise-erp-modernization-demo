# 🏭 Enterprise ERP Modernization (Strangler Fig & CQRS)

**Live Interactive Demo:** [romanboparai.com/erp-modernization](https://romanboparai.com/erp-modernization)

This repository demonstrates the modernization of a legacy on-premise Enterprise Resource Planning (ERP) system using the **Strangler Fig Pattern** and **CQRS** (Command Query Responsibility Segregation). 

By incrementally migrating read-heavy inventory operations from a highly relational monolithic database to an optimized Azure Serverless NoSQL backend, this architecture achieves massive performance gains and unlocks horizontal scalability.



---

## 🚀 The Business Problem
Legacy on-premise systems often rely on massive relational databases (SQL). As data grows, complex `JOIN` operations bottleneck the entire system, slowing down critical user-facing applications (like product catalogs or inventory lookups).

## 💡 The Architectural Solution
Instead of a risky "Big Bang" rewrite, this project implements the **Strangler Fig Pattern** to safely and incrementally route traffic away from the legacy system. 



By applying **CQRS**, we separate the heavy write operations (which remain in the legacy system) from the read operations. The read data is asynchronously transformed (ETL) and flattened into a highly optimized NoSQL Read Model hosted on Azure Serverless.

### The Head-to-Head Comparison
* **The Legacy Monolith (The Old Way):** An ASP.NET Core API querying a heavily normalized SQLite database. Requires computing expensive SQL JOINs on every single request.
* **The Cloud-Native Read Model (The New Way):** An Azure Function querying a pre-computed NoSQL partition. Achieves **O(1) time complexity** for instant data retrieval.

---

## 🛠️ Tech Stack & Patterns
* **Language/Framework:** C#, .NET 9
* **Cloud & Hosting:** Azure Functions (Serverless), Render (Monolith API)
* **Architecture Patterns:** Strangler Fig, CQRS, Event-Driven/ETL Concepts
* **Databases:** Relational (SQLite / Entity Framework Core) & NoSQL Read Models
* **Frontend Demo:** Astro, Vanilla JS (Fetch API)

---

## 🏎️ The Interactive Demo
I built a custom frontend dashboard to visually prove the performance difference between the two architectures. You can run a live "race" between the two APIs directly in your browser.

**Features of the demo:**
* **Real-time Benchmarking:** Compares server execution times side-by-side.
* **Cold-Start Mitigation:** Implements a silent background warm-up ping to wake up the Azure serverless container upon page load.
* **Keep-Alive Heartbeat:** Utilizes a CRON job (UptimeRobot) to ensure the legacy free-tier Linux container remains awake for accurate benchmarking.

👉 **[Try the Demo Here](https://romanboparai.com/erp-modernization)**

---

## 💻 How to Run Locally

If you would like to clone this repository and test the APIs locally:

1. **Clone the repo:**
   ```bash
   git clone [https://github.com/Ramnjit/enterprise_erp_modernization.git](https://github.com/Ramnjit/enterprise_erp_modernization.git)
   cd enterprise_erp_modernization
   ```

2. **Run the Legacy Monolith API:**
   ```bash
   cd legacy_onsight_erp_monolith
   dotnet run
   ```
    *Note: The root localhost URL will return empty. To test the API, navigate directly to the endpoint in your browser or Postman (Check your console output in case your local machine assigns a different port):*
    * 👉 **Catalog Fetch:** `http://localhost:5249/api/legacy/all` 
    * 👉 **Single SKU Lookup:** `http://localhost:5249/api/legacy/product/0005860631142`

3. **Run the Modern Azure Function API:**
   ```bash
   cd modern_cloud_api
   func start
   ```
   *The Azure Functions Core Tools will output the exact local URLs for your endpoints in the console (typically starting at `http://localhost:7071`).*
   
   *Test the endpoints:*
   * 👉 **Catalog Fetch:** `http://localhost:7071/api/modern/all`
   * 👉 **Single SKU Lookup:** `http://localhost:7071/api/modern/product/0005860631142`

*(Prerequisites: Ensure you have the .NET 9 SDK and Azure Functions Core Tools installed on your machine).*