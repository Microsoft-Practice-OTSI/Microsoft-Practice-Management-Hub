# Microsoft Practice Management Hub

## 1. Introduction (Overview)
The **Microsoft Practice Management Hub** is a centralized, modern web portal designed to help practice leadership seamlessly manage resources, track project delivery, and monitor the overall health and skills of the practice.

## 2. Current Capabilities (What We Have Right Now)
* **Resource Management:** View and track all practice consultants, their designations, experience levels, current availability (bench vs. allocated), and skills.
* **Project & Allocation Tracking:** Monitor active enterprise projects, their required staffing levels, project health, and currently allocated team members.
* **Dynamic Dashboard & Navigation:** A central dashboard providing high-level metrics. The sidebar navigation now dynamically updates in real-time to reflect the exact number of active resources and projects currently in the database.
* **Cloud Infrastructure:** The system is fully integrated with **Microsoft Azure Storage** (Table Storage and Blob Storage) for secure, scalable, and reliable data management in the cloud.

## 3. How to Run the Project (Getting Started)

### Step 1: Prerequisites
Ensure you have the **.NET 8.0 SDK** installed on your machine. You will also need an IDE such as Visual Studio 2022 or Visual Studio Code.

### Step 2: Configure Database (Azure Storage)
1. Open the `appsettings.json` file located in the `MicrosoftPracticeManagement.Web` folder.
2. Ensure the `TableStorageConnection` and `BlobStorageConnection` strings contain a valid Azure Storage connection string. *(If you prefer local development, you can use the Microsoft Azurite emulator)*.

### Step 3: Build the Solution
Open your terminal or command prompt at the project root directory and run:
```bash
dotnet build
```

### Step 4: Run the Application
Navigate into the Web project folder and start the app:
```bash
cd MicrosoftPracticeManagement.Web
dotnet run
```

### Step 5: Access the Portal
Open your web browser and navigate to the `localhost` URL provided in your terminal output (typically `https://localhost:7193` or similar).

## 4. Next Steps
1. Planning to implement the logic to allocate the project to resources.
2. Will move to new module implementation as a next step.
