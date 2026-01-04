# Zebra Label Editor

A modern WPF application for automating Zebra Label (ZPL) printing and generation. This tool allows users to map dynamic fields in a ZPL template to columns in an Excel sheet, enabling batch printing or mass generation of label files.

![Import View Screenshot](Screenshots/import_view.png) 
*(Place screenshot of the Import View here)*

## Features

* **ZPL Template Parsing:** Automatically detects placeholders (e.g., `[ProductCode]`) inside raw ZPL files.
* **Excel Integration:** Load `.xlsx` or `.csv` files and map columns to label placeholders.
* **Smart Mapping:**
    * Map dynamic Excel columns.
    * Set static "Constant Values" for specific fields.
    * Leave fields empty if needed.
* **Dual Output Modes:**
    * 🖨️ **Direct Print:** Sends raw ZPL code directly to any installed Zebra printer (bypassing the print driver's image processing).
    * 💾 **Save to File:** Generates batch files (`.zpl`, `.txt`, `.prn`) for later processing.
* **Batch Processing:** Handles single-label generation or bulk processing with a responsive progress bar.

## Technology Stack

* **Framework:** .NET 8 (WPF)
* **Pattern:** MVVM (Model-View-ViewModel)
* **Libraries:**
    * `CommunityToolkit.Mvvm` (Messaging & Commands)
    * `ExcelDataReader` (Excel parsing)
    * `System.Drawing.Common` (Printer discovery)
* **Printing:** Native Win32 Spooler API (`winspool.Drv`) for raw data transmission.

## Application Flow

### 1. Import Template
Validates the loaded file to ensure it contains valid ZPL commands (checks for `^XA` header).
> ![Import View](Screenshots/import_view.png)

### 2. Map Data
The core interface. Users map extracted placeholders to Excel headers or define constant values.
> ![Mapping View](Screenshots/mapping_view.png)

### 3. Select Output
Choose between printing directly to hardware or saving files.
> ![Selection View](Screenshots/selection_view.png)

### 4. Configuration & Processing
Configure printer settings or file paths, then watch the progress.
> ![Processing View](Screenshots/processing_view.png)

---

## Architecture Overview

This project follows a strict **MVVM** architecture with **Dependency Injection**.

### Navigation (The "Traffic Cop")
Navigation is handled by `MainViewModel.cs`. It acts as a central hub that manages the state of all child ViewModels (`ImportViewModel`, `MappingViewModel`, etc.).
* **State Persistence:** Child ViewModels are instantiated once in `App.xaml.cs`. This ensures data (like the loaded Excel file) is preserved when the user clicks "Back".

### Key Services
* **`ZplService`**: Handles regex parsing of placeholders and merging data into the ZPL string.
* **`ExcelService`**: Wraps `ExcelDataReader` to extract column headers and row data.
* **`RawPrinterHelper`**: A static helper using `DllImport("winspool.Drv")` to send raw bytes to the printer. This is critical for ZPL, as standard .NET printing sends images, which renders barcodes unreadable by scanners.

### Project Structure
```text
ZebraLabelEditor/
├── Models/              # Data structures (MappingItem, etc.)
├── Services/            # Business Logic (ZplService, ExcelService)
│   ├── Interfaces/      # Interfaces for DI
│   └── Implementations/ # Concrete logic
├── ViewModels/          # UI Logic (VMs inheriting ObservableObject)
├── Views/               # XAML UserControls
├── Assets/              # Images and Icons
├── App.xaml.cs          # Composition Root (Dependency Injection Setup)
└── MainWindow.xaml      # Main shell hosting the CurrentViewModel
```
## Getting Started
Prerequisites
- Visual Studio 2022 (or later)
- .NET 8 SDK
- A Zebra Printer driver installed (for testing Print mode)

Installation
1. Clone the repository.

2. Open the solution in Visual Studio.

3. Restore NuGet packages:

```Bash

dotnet restore
Build and Run.
```

## Common Issues & Fixes
Printer not listing? Ensure the printer is installed in Windows "Printers & Scanners". The app uses PrinterSettings.InstalledPrinters to fetch this list.

"Value cannot be null" Error during processing? This usually happens if the Excel file path is lost. Ensure MappingViewModel retains its state. (Fixed in v1.0 logic by passing data explicitly via MainViewModel).

## Contributing
Fork the project.

Create your Feature Branch (git checkout -b feature/AmazingFeature).

Commit your changes (git commit -m 'Add some AmazingFeature').

Push to the branch (git push origin feature/AmazingFeature).

Open a Pull Request.

## License
Distributed under the MIT License. See LICENSE for more information.